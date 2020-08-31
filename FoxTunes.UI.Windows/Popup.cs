using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace FoxTunes
{
    public class Popup : global::System.Windows.Controls.Primitives.Popup
    {
        public Popup()
        {
            this.Loaded += this.OnPopupLoaded;
            this.Unloaded += this.OnPopupUnloaded;
        }

        public bool Topmost { get; private set; }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    this.IsOpen = false;
                    break;
            }
            base.OnKeyUp(e);
        }

        protected virtual void OnPopupLoaded(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            if (window == null)
            {
                return;
            }
            window.Activated += this.OnParentWindowActivated;
            window.Deactivated += this.OnParentWindowDeactivated;
        }

        protected virtual void OnPopupUnloaded(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            if (window == null)
            {
                return;
            }
            window.Activated -= this.OnParentWindowActivated;
            window.Deactivated -= this.OnParentWindowDeactivated;
        }

        protected virtual void OnParentWindowActivated(object sender, EventArgs e)
        {
            this.SetTopmost(true);
        }

        protected virtual void OnParentWindowDeactivated(object sender, EventArgs e)
        {
            this.SetTopmost(false);
        }

        protected override void OnOpened(EventArgs e)
        {
            this.SetTopmost(true);
            base.OnOpened(e);
        }

        private void SetTopmost(bool value)
        {
            if (this.Topmost == value || this.Child == null || !this.Child.IsVisible)
            {
                return;
            }

            var rect = default(RECT);
            var source = (HwndSource)HwndSource.FromVisual(this.Child);
            var handle = source.Handle;

            if (!GetWindowRect(handle, out rect))
            {
                return;
            }

            if (value)
            {
                SetWindowPos(handle, HWND_TOPMOST, rect.Left, rect.Top, (int)this.Width, (int)this.Height, TOPMOST_FLAGS);
            }
            else
            {
                SetWindowPos(handle, HWND_BOTTOM, rect.Left, rect.Top, (int)this.Width, (int)this.Height, TOPMOST_FLAGS);
                SetWindowPos(handle, HWND_TOP, rect.Left, rect.Top, (int)this.Width, (int)this.Height, TOPMOST_FLAGS);
                SetWindowPos(handle, HWND_NOTOPMOST, rect.Left, rect.Top, (int)this.Width, (int)this.Height, TOPMOST_FLAGS);
            }

            this.Topmost = value;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
        static readonly IntPtr HWND_TOP = new IntPtr(0);
        static readonly IntPtr HWND_BOTTOM = new IntPtr(1);

        const uint SWP_NOSIZE = 0x0001;
        const uint SWP_NOMOVE = 0x0002;
        const uint SWP_NOREDRAW = 0x0008;
        const uint SWP_NOACTIVATE = 0x0010;

        const uint SWP_NOOWNERZORDER = 0x0200;
        const uint SWP_NOSENDCHANGING = 0x0400;

        const uint TOPMOST_FLAGS = SWP_NOACTIVATE | SWP_NOOWNERZORDER | SWP_NOSIZE | SWP_NOMOVE | SWP_NOREDRAW | SWP_NOSENDCHANGING;
    }
}
