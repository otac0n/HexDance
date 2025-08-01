// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace HexDance
{
    using System.Diagnostics;
    using System.Drawing.Drawing2D;

    public partial class HexDisplay : Form
    {
        private static readonly int QueueLength = 15;
        private static readonly int SegmentCount = 10;
        private static readonly float SegmentLength = 10f;

        private readonly Color[] palette;
        private readonly List<GraphicsPath> paths = new(2 * QueueLength);
        private readonly Stopwatch iconUpdate = new();
        private PointF lastCursor;

        public HexDisplay()
        {
            this.InitializeComponent();
            this.CoverAllScreens();

            var palette = new Color[QueueLength];
            for (var i = 0; i < QueueLength; i++)
            {
                palette[i] = Blend((float)i / QueueLength, Color.FromArgb(0x92ECD2), Color.Black);
            }

            this.palette = palette;
            this.lastCursor = Cursor.Position;
        }

        protected override bool ShowWithoutActivation => true;

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

        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            var paths = this.paths;

            var cursor = Cursor.Position;
            var last = this.lastCursor;

            var distance = new SizeF(cursor.X - last.X, cursor.Y - last.Y);
            var lerpCount = Math.Max(1, Math.Min(QueueLength, MathF.Sqrt(distance.Width * distance.Width + distance.Height * distance.Height) / (SegmentLength * MathF.Sqrt(SegmentCount) / 2)));
            for (var h = 1; h <= lerpCount; h++)
            {
                var path = new GraphicsPath();
                var origin = Lerp(h / lerpCount, last, cursor);
                var c = GetNearestHex(origin, SegmentLength);
                var (x, y) = (c.X, c.Y);
                for (var i = 0; i < SegmentCount; i++)
                {
                    var dir = Random.Shared.Next(3) * MathF.Tau / 3 + (i % 2 == 0 ? MathF.Tau / 6 : 0);
                    var (dx, dy) = MathF.SinCos(dir);
                    dx *= SegmentLength;
                    dy *= SegmentLength;
                    path.AddLine(x, y, x + dx, y + dy);
                    (x, y) = (x + dx, y + dy);
                }

                paths.Add(path);
            }

            this.lastCursor = cursor;
            this.Invalidate();
        }

        private void Form_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            var paths = this.paths;

            var state = g.Save();
            try
            {
                using var clearPen = new Pen(this.BackColor);
                while (paths.Count > QueueLength)
                {
                    g.DrawPath(clearPen, paths[0]);
                    paths.RemoveAt(0);
                }

                for (var i = 0; i < paths.Count; i++)
                {
                    using var pen = new Pen(this.palette[i + (QueueLength - paths.Count)]);
                    g.DrawPath(pen, paths[i]);
                }
            }
            finally
            {
                g.Restore(state);
            }

            if (!this.iconUpdate.IsRunning || this.iconUpdate.Elapsed > TimeSpan.FromSeconds(1))
            {
                this.iconUpdate.Restart();
                this.UpdateNotifyIcon();
                this.CoverAllScreens();
            }
        }

        private void UpdateNotifyIcon()
        {
            var paths = this.paths;

            using var bmp = new Bitmap(24, 24);
            using (var g = Graphics.FromImage(bmp))
            {
                var offset = new Matrix();
                var scale = 3 / SegmentLength;
                offset.Scale(scale, scale);
                offset.Translate(-Cursor.Position.X + 12 / scale, -Cursor.Position.Y + 12 / scale);
                g.Transform = offset;
                g.Clear(Color.Black);

                for (var i = 0; i < paths.Count; i++)
                {
                    using var pen = new Pen(this.palette[i + (QueueLength - paths.Count)]);
                    g.DrawPath(pen, paths[i]);
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
    }
}
