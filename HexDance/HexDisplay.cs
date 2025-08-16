// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace HexDance
{
    using System.Diagnostics;
    using System.Drawing.Drawing2D;
    using System.Runtime.InteropServices;
    using HexDance.Properties;

    public partial class HexDisplay : Form
    {
        private readonly Settings settings;

        private readonly List<(TimeSpan Time, GraphicsPath Path)> paths = [];
        private readonly List<(TimeSpan Time, Point Origin, SizeF Direction, float Angle, float Distance)> particles = [];
        private readonly GraphicsPath singleHex;
        private readonly Stopwatch clock = Stopwatch.StartNew();
        private readonly NativeMethods.LowLevelMouseProc mouseProc;
        private TimeSpan iconUpdate = TimeSpan.Zero;
        private bool render = true;
        private MouseButtons buttonState;
        private PointF lastCursor;
        private nint mouseHandle;

        public HexDisplay(Settings settings)
        {
            this.settings = settings;

            this.InitializeComponent();
            this.CoverAllScreens();
            this.updateTimer.Interval = (int)Math.Round(settings.UpdateInterval.TotalMilliseconds);
            this.TransparencyKey = this.BackColor = settings.ChromaKeyColor;
            this.DoubleBuffered = settings.DoubleBuffered;
            this.Opacity = settings.Opacity;

            this.lastCursor = Cursor.Position;

            this.singleHex = MakeHex(settings.EffectHexSize);

            var mainModule = Process.GetCurrentProcess().MainModule!;
            this.mouseProc = this.MouseHook;
            this.mouseHandle = NativeMethods.SetWindowsHookEx(NativeMethods.WH_MOUSE_LL, this.mouseProc, NativeMethods.GetModuleHandle(mainModule.ModuleName), 0);
        }

        private static GraphicsPath MakeHex(float radius)
        {
            PointF HexPoint(int i)
            {
                var (x, y) = MathF.SinCos(MathF.Tau * i / 6);
                return new(x * radius, y * radius);
            }

            var singleHex = new GraphicsPath();
            singleHex.StartFigure();
            for (var i = 0; i < 5; i++)
            {
                singleHex.AddLine(HexPoint(i), HexPoint(i + 1));
            }

            singleHex.CloseFigure();
            return singleHex;
        }

        /// <inheritdoc/>
        protected override bool ShowWithoutActivation => true;

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
            }

            if (this.mouseHandle != nint.Zero)
            {
                NativeMethods.UnhookWindowsHookEx(this.mouseHandle);
                this.mouseHandle = nint.Zero;
            }

            base.Dispose(disposing);
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= NativeMethods.WS_EX_TRANSPARENT | NativeMethods.WS_EX_NOACTIVATE;
                return cp;
            }
        }

        private void CoverAllScreens()
        {
            var extents = Screen.AllScreens.Select(s => s.Bounds).Aggregate(Rectangle.Union);
            this.Location = extents.Location;
            this.Size = extents.Size;
        }

        private void CloseMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private nint MouseHook(int nCode, nint wParam, nint lParam)
        {
            if (nCode >= 0)
            {
                var msg = (int)wParam;
                var hookStruct = Marshal.PtrToStructure<NativeMethods.MSLLHOOKSTRUCT>(lParam);
                this.Invoke(() => this.OnMouseEvent(msg, hookStruct));
            }

            return NativeMethods.CallNextHookEx(this.mouseHandle, nCode, wParam, lParam);
        }

        private void OnMouseEvent(int message, NativeMethods.MSLLHOOKSTRUCT info)
        {
            this.buttonState = message switch
            {
                NativeMethods.WM_LBUTTONDOWN => this.buttonState | MouseButtons.Left,
                NativeMethods.WM_LBUTTONUP => this.buttonState & ~MouseButtons.Left,
                NativeMethods.WM_RBUTTONDOWN => this.buttonState | MouseButtons.Right,
                NativeMethods.WM_RBUTTONUP => this.buttonState & ~MouseButtons.Right,
                NativeMethods.WM_MBUTTONDOWN => this.buttonState | MouseButtons.Middle,
                NativeMethods.WM_MBUTTONUP => this.buttonState & ~MouseButtons.Middle,
                NativeMethods.WM_XBUTTONDOWN => (info.mouseData >> 16) switch
                {
                    1 => this.buttonState | MouseButtons.XButton1,
                    2 => this.buttonState | MouseButtons.XButton2,
                    _ => this.buttonState,
                },
                NativeMethods.WM_XBUTTONUP => (info.mouseData >> 16) switch
                {
                    1 => this.buttonState & ~MouseButtons.XButton1,
                    2 => this.buttonState & ~MouseButtons.XButton2,
                    _ => this.buttonState,
                },
                _ => this.buttonState,
            };
        }

        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            var now = this.clock.Elapsed;
            var paths = this.paths;

            var cursor = Cursor.Position;
            var last = this.lastCursor;

            var distance = new SizeF(cursor.X - last.X, cursor.Y - last.Y);
            var randomWalkDistance = this.settings.GridHexSize * MathF.Sqrt(this.settings.GridSegmentCount);
            var lerpCount = Math.Max(1, Math.Min(this.settings.GridQueueLength, 2 * MathF.Sqrt(distance.Width * distance.Width + distance.Height * distance.Height) / randomWalkDistance));
            for (var h = 1; h <= lerpCount; h++)
            {
                var path = new GraphicsPath();
                var origin = Lerp(h / lerpCount, last, cursor);
                var c = GetNearestHex(origin, this.settings.GridHexSize);
                var (x, y) = (c.X, c.Y);
                for (var i = 0; i < this.settings.GridSegmentCount; i++)
                {
                    var dir = Random.Shared.Next(3) * MathF.Tau / 3 + (i % 2 == 0 ? MathF.Tau / 6 : 0);
                    var (dx, dy) = MathF.SinCos(dir);
                    dx *= this.settings.GridHexSize;
                    dy *= this.settings.GridHexSize;
                    path.AddLine(x, y, x + dx, y + dy);
                    (x, y) = (x + dx, y + dy);
                }

                paths.Add((now, path));
            }

            if (this.buttonState != MouseButtons.None && this.settings.EffectHexSize >= 0)
            {
                var origin = cursor;
                var (dx, dy) = MathF.SinCos(Random.Shared.NextSingle() * MathF.Tau);
                var dist = Random.Shared.NextSingle() * 0.8f + 0.2f;
                var angle = Random.Shared.NextSingle() * MathF.Tau;
                this.particles.Add((now, origin, new(dx, dy), angle, dist));
            }

            this.lastCursor = cursor;
            this.Invalidate();
        }

        private void Form_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            var paths = this.paths;
            var particles = this.particles;
            var now = this.clock.Elapsed;
            var gridExpireTime = now - this.settings.GridDisplayTime;
            var effectExpireTime = now - this.settings.EffectDisplayTime;

            paths.RemoveAll(entry => entry.Time <= gridExpireTime);
            particles.RemoveAll(entry => entry.Time <= effectExpireTime);
            var extra = paths.Count - this.settings.GridQueueLength;
            if (extra > 0)
            {
                paths.RemoveRange(0, extra);
            }

            if (this.render)
            {
                for (var i = 0; i < paths.Count; i++)
                {
                    var entry = paths[i];
                    using var pen = new Pen(Palette(now - entry.Time));
                    g.DrawPath(pen, entry.Path);
                }

                var state = g.Save();
                try
                {
                    for (var i = particles.Count - 1; i >= 0; i--)
                    {
                        var particle = particles[i];
                        var amount = (float)((now - particle.Time) / this.settings.EffectDisplayTime);
                        var distance = particle.Direction * amount * particle.Distance * this.settings.EffectDistance;
                        var angle = MathF.Atan2(particle.Direction.Width, particle.Direction.Height) * 360 / MathF.Tau;
                        var matrix = new Matrix();
                        matrix.Translate(particle.Origin.X + distance.Width, particle.Origin.Y + distance.Height);
                        matrix.Rotate(-angle);
                        matrix.Scale(1, 1 - amount);
                        matrix.Rotate(particle.Angle);

                        using var brush = new SolidBrush(this.settings.EffectFillColor);
                        using var pen = new Pen(this.settings.EffectLineColor);

                        g.Transform = matrix;
                        g.FillPath(brush, this.singleHex);
                        g.DrawPath(pen, this.singleHex);
                    }
                }
                finally
                {
                    g.Restore(state);
                }
            }

            if (now >= this.iconUpdate)
            {
                this.iconUpdate = now + TimeSpan.FromSeconds(1);
                this.render = !MouseIsCaptured() && !this.FocusIsFullscreen();
                this.UpdateNotifyIcon(now);
                this.CoverAllScreens();
            }
        }

        private void UpdateNotifyIcon(TimeSpan now)
        {
            var paths = this.paths;

            using var bmp = new Bitmap(24, 24);
            using (var g = Graphics.FromImage(bmp))
            {
                var offset = new Matrix();
                var scale = 3 / this.settings.GridHexSize;
                offset.Scale(scale, scale);
                offset.Translate(-Cursor.Position.X + 12 / scale, -Cursor.Position.Y + 12 / scale);
                g.Transform = offset;
                g.Clear(Color.Black);

                for (var i = 0; i < paths.Count; i++)
                {
                    var entry = this.paths[i];
                    using var pen = new Pen(Palette(now - entry.Time));
                    g.DrawPath(pen, entry.Path);
                }
            }

            var oldIcon = this.notifyIcon.Icon;
            var hIcon = bmp.GetHicon();
            this.notifyIcon.Icon = Icon.FromHandle(hIcon);

            if (oldIcon != null)
            {
                NativeMethods.DestroyIcon(oldIcon.Handle);
            }
        }

        public Color Palette(TimeSpan elapsed) => Blend(elapsed / this.settings.GridDisplayTime, this.settings.GridDarkColor, this.settings.GridBrightColor);

        public static Color Blend(double amount, Color a, Color b) => Blend((float)amount, a, b);

        public static Color Blend(float amount, Color a, Color b)
        {
            const float Gamma = 1.8f;

            var red = 0.0f;
            var green = 0.0f;
            var blue = 0.0f;

            red += MathF.Pow(a.R, Gamma) * amount;
            green += MathF.Pow(a.G, Gamma) * amount;
            blue += MathF.Pow(a.B, Gamma) * amount;

            red += MathF.Pow(b.R, Gamma) * (1 - amount);
            green += MathF.Pow(b.G, Gamma) * (1 - amount);
            blue += MathF.Pow(b.B, Gamma) * (1 - amount);

            return Color.FromArgb(
                (int)Math.Round(Math.Pow(red, 1 / Gamma)),
                (int)Math.Round(Math.Pow(green, 1 / Gamma)),
                (int)Math.Round(Math.Pow(blue, 1 / Gamma)));
        }

        public static PointF GetNearestHex(PointF point, float hexRadius)
        {
            var x = point.X / hexRadius;
            var y = point.Y / hexRadius;
            var q = MathF.Sqrt(3) / 3 * x - 1f / 3 * y;
            var r = 2f / 3 * y;
            var s = -(q + r);

            var qRound = (int)MathF.Round(q);
            var rRound = (int)MathF.Round(r);
            var sRound = (int)MathF.Round(s);
            var qDiff = MathF.Abs(qRound - q);
            var rDiff = MathF.Abs(rRound - r);
            var sDiff = MathF.Abs(sRound - s);

            if (qDiff > rDiff && qDiff > sDiff)
            {
                q = -(rRound + sRound);
                r = rRound;
            }
            else if (rDiff > sDiff)
            {
                q = qRound;
                r = -(qRound + sRound);
            }
            else
            {
                q = qRound;
                r = rRound;
            }

            x = MathF.Sqrt(3) * (q * hexRadius) + MathF.Sqrt(3) / 2 * (r * hexRadius);
            y = 3f / 2 * (r * hexRadius);
            return new(x, y);
        }

        public static PointF Lerp(float amount, PointF start, PointF endPoint) =>
            new(start.X + amount * (endPoint.X - start.X), start.Y + amount * (endPoint.Y - start.Y));

        public static bool MouseIsCaptured() =>
            NativeMethods.GetCapture() != nint.Zero;

        public bool FocusIsFullscreen()
        {
            var hWnd = NativeMethods.GetForegroundWindow();
            if (hWnd == nint.Zero ||
                hWnd == this.Handle ||
                !NativeMethods.GetWindowRect(hWnd, out var rect))
            {
                return false;
            }

            var screen = Screen.FromHandle(hWnd);
            var bounds = screen.Bounds;

            return rect.Left <= bounds.Left &&
                   rect.Top <= bounds.Top &&
                   rect.Right >= bounds.Right &&
                   rect.Bottom >= bounds.Bottom;
        }
    }
}
