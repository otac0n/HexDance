// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace HexDance
{
    using System.Runtime.InteropServices;

    internal static class NativeMethods
    {
        public const int WS_EX_NOACTIVATE = 0x08000000;
        public const int WS_EX_TRANSPARENT = 0x20;
        public const int WH_MOUSE_LL = 14;
        public const int WM_LBUTTONDOWN = 0x0201;
        public const int WM_LBUTTONUP = 0x0202;
        public const int WM_RBUTTONDOWN = 0x0204;
        public const int WM_RBUTTONUP = 0x0205;
        public const int WM_MBUTTONDOWN = 0x0207;
        public const int WM_MBUTTONUP = 0x0208;
        public const int WM_XBUTTONDOWN = 0x020A;
        public const int WM_XBUTTONUP = 0x020B;
        public const int ATTACH_PARENT_PROCESS = -1;

        public delegate nint LowLevelMouseProc(int nCode, nint wParam, nint lParam);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool DestroyIcon(nint hIcon);

        [DllImport("user32.dll")]
        public static extern nint GetCapture();

        [DllImport("user32.dll")]
        public static extern nint GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool GetWindowRect(nint hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        public static extern nint SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, nint hMod, uint dwThreadId);

        [DllImport("user32.dll")]
        public static extern bool UnhookWindowsHookEx(nint hHook);

        [DllImport("user32.dll")]
        public static extern nint CallNextHookEx(nint hHook, int nCode, nint wParam, nint lParam);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool AttachConsole(int dwProcessId);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        public static extern nint GetModuleHandle(string lpModuleName);

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MSLLHOOKSTRUCT
        {
            public POINT point;
            public uint mouseData;
            public uint flags;
            public uint time;
            public nint dwExtraInfo;
        }
    }
}
