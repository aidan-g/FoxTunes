using System;
using System.Runtime.InteropServices;

namespace FoxTunes
{
    public static class MouseHelper
    {
        public static bool IsWindowsVista = Environment.OSVersion.Version.Major >= 6;

        public static void GetPosition(out int x, out int y)
        {
            var point = new Point();
            if (IsWindowsVista)
            {
                GetPhysicalCursorPos(ref point);
            }
            else
            {
                GetCursorPos(ref point);
            }
            x = point.X;
            y = point.Y;
        }


        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool GetPhysicalCursorPos(ref Point lpPoint);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool GetCursorPos(ref Point lpPoint);

        [StructLayout(LayoutKind.Sequential)]
        public struct Point
        {
            public int X;
            public int Y;
        }
    }
}
