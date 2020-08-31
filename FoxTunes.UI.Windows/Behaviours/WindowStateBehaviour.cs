using FoxTunes.Interfaces;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.UserInterface)]
    public class WindowStateBehaviour : StandardBehaviour, IDisposable
    {
        const int WM_GETMINMAXINFO = 0x0024;

        public WindowStateBehaviour()
        {
            this.Behaviours = new ConditionalWeakTable<IUserInterfaceWindow, MinMaxBehaviour>();
        }

        public ConditionalWeakTable<IUserInterfaceWindow, MinMaxBehaviour> Behaviours { get; private set; }

        public ICore Core { get; private set; }

        public IUserInterface UserInterface { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Core = core;
            this.UserInterface = core.Components.UserInterface;
            this.UserInterface.WindowCreated += this.OnWindowCreated;
            this.UserInterface.WindowDestroyed += this.OnWindowDestroyed;
            base.InitializeComponent(core);
        }

        protected virtual void OnWindowCreated(object sender, UserInterfaceWindowEventArgs e)
        {
            var window = GetWindow(e.Window.Handle);
            if (window == null)
            {
                return;
            }
            if (window.ResizeMode != ResizeMode.CanResize && window.ResizeMode != ResizeMode.CanResizeWithGrip)
            {
                return;
            }
            this.Enable(e.Window);
        }

        protected virtual void OnWindowDestroyed(object sender, UserInterfaceWindowEventArgs e)
        {
            this.Disable(e.Window);
        }

        protected virtual void Enable(IUserInterfaceWindow window)
        {
            var behaviour = new MinMaxBehaviour(window.Handle);
            behaviour.InitializeComponent(this.Core);
            this.Behaviours.Add(window, behaviour);
        }

        protected virtual void Disable(IUserInterfaceWindow window)
        {
            var behaviour = default(MinMaxBehaviour);
            if (!this.Behaviours.TryRemove(window, out behaviour))
            {
                return;
            }
            behaviour.Dispose();
        }

        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.IsDisposed || !disposing)
            {
                return;
            }
            this.OnDisposing();
            this.IsDisposed = true;
        }

        protected virtual void OnDisposing()
        {
            if (this.UserInterface != null)
            {
                this.UserInterface.WindowCreated -= this.OnWindowCreated;
                this.UserInterface.WindowDestroyed -= this.OnWindowDestroyed;
            }
            foreach (var window in this.UserInterface.Windows)
            {
                this.Disable(window);
            }
        }

        ~WindowStateBehaviour()
        {
            Logger.Write(this, LogLevel.Error, "Component was not disposed: {0}", this.GetType().Name);
            try
            {
                this.Dispose(true);
            }
            catch
            {
                //Nothing can be done, never throw on GC thread.
            }
        }

        public static Window GetWindow(IntPtr handle)
        {
            var windows = Application.Current.Windows;
            for (var a = 0; a < windows.Count; a++)
            {
                var window = windows[a];
                if (window.GetHandle() == handle)
                {
                    return window;
                }
            }
            return null;
        }

        public class MinMaxBehaviour : BaseComponent, IDisposable
        {
            private MinMaxBehaviour()
            {
                this.Hook = new HwndSourceHook(this.WindowProc);
            }

            public MinMaxBehaviour(IntPtr handle) : this()
            {
                this.Handle = handle;
            }

            public HwndSourceHook Hook { get; private set; }

            public IntPtr Handle { get; private set; }

            public override void InitializeComponent(ICore core)
            {
                this.Enable();
                base.InitializeComponent(core);
            }

            public void Enable()
            {
                var source = HwndSource.FromHwnd(this.Handle);
                if (source == null)
                {
                    return;
                }
                source.AddHook(this.Hook);
            }

            public void Disable()
            {
                var source = HwndSource.FromHwnd(this.Handle);
                if (source == null)
                {
                    return;
                }
                source.RemoveHook(this.Hook);
            }

            protected virtual IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
            {
                switch (msg)
                {
                    case WM_GETMINMAXINFO:
                        this.WmGetMinMaxInfo(hwnd, lParam);
                        break;
                }
                return IntPtr.Zero;
            }

            protected virtual void WmGetMinMaxInfo(IntPtr hwnd, IntPtr lParam)
            {
                var mousePosition = default(POINT);
                GetCursorPos(out mousePosition);

                var primaryScreen = MonitorFromPoint(new POINT(0, 0), MonitorOptions.MONITOR_DEFAULTTOPRIMARY);
                var primaryScreenInfo = new MONITORINFO();
                if (GetMonitorInfo(primaryScreen, primaryScreenInfo) == false)
                {
                    return;
                }

                var currentScreen = MonitorFromPoint(mousePosition, MonitorOptions.MONITOR_DEFAULTTONEAREST);
                var minMaxInfo = (MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(MINMAXINFO));

                if (primaryScreen == currentScreen)
                {
                    minMaxInfo.MaxPosition.X = primaryScreenInfo.Work.Left;
                    minMaxInfo.MaxPosition.Y = primaryScreenInfo.Work.Top;
                    minMaxInfo.MaxSize.X = primaryScreenInfo.Work.Right - primaryScreenInfo.Work.Left;
                    minMaxInfo.MaxSize.Y = primaryScreenInfo.Work.Bottom - primaryScreenInfo.Work.Top;
                }
                else
                {
                    minMaxInfo.MaxPosition.X = primaryScreenInfo.Monitor.Left;
                    minMaxInfo.MaxPosition.Y = primaryScreenInfo.Monitor.Top;
                    minMaxInfo.MaxSize.X = primaryScreenInfo.Monitor.Right - primaryScreenInfo.Monitor.Left;
                    minMaxInfo.MaxSize.Y = primaryScreenInfo.Monitor.Bottom - primaryScreenInfo.Monitor.Top;
                }

                Marshal.StructureToPtr(minMaxInfo, lParam, true);
            }

            public bool IsDisposed { get; private set; }

            public void Dispose()
            {
                this.Dispose(true);
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool disposing)
            {
                if (this.IsDisposed || !disposing)
                {
                    return;
                }
                this.OnDisposing();
                this.IsDisposed = true;
            }

            protected virtual void OnDisposing()
            {
                this.Disable();
            }

            ~MinMaxBehaviour()
            {
                Logger.Write(this.GetType(), LogLevel.Error, "Component was not disposed: {0}", this.GetType().Name);
                try
                {
                    this.Dispose(true);
                }
                catch
                {
                    //Nothing can be done, never throw on GC thread.
                }
            }

            [DllImport("user32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool GetCursorPos(out POINT lpPoint);

            [DllImport("user32.dll", SetLastError = true)]
            public static extern IntPtr MonitorFromPoint(POINT pt, MonitorOptions dwFlags);

            [DllImport("user32.dll")]
            public static extern bool GetMonitorInfo(IntPtr hMonitor, MONITORINFO lpmi);

            public enum MonitorOptions : uint
            {
                MONITOR_DEFAULTTONULL = 0x00000000,
                MONITOR_DEFAULTTOPRIMARY = 0x00000001,
                MONITOR_DEFAULTTONEAREST = 0x00000002
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct POINT
            {
                public int X;
                public int Y;

                public POINT(int x, int y)
                {
                    this.X = x;
                    this.Y = y;
                }
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct MINMAXINFO
            {
                public POINT Reserved;
                public POINT MaxSize;
                public POINT MaxPosition;
                public POINT MinTrackSize;
                public POINT MaxTrackSize;
            };

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
            public class MONITORINFO
            {
                public int cbSize = Marshal.SizeOf(typeof(MONITORINFO));
                public RECT Monitor = new RECT();
                public RECT Work = new RECT();
                public int Flags = 0;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct RECT
            {
                public int Left;
                public int Top;
                public int Right;
                public int Bottom;
            }
        }
    }
}
