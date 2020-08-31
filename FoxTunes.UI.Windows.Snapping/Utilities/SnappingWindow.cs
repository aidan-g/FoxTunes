using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Interop;

namespace FoxTunes
{
    public class SnappingWindow : BaseComponent, IDisposable
    {
        public const int WM_NCLBUTTONDOWN = 0x00A1;

        public const int WM_KEYDOWN = 0x0100;

        public const int WM_MOUSEMOVE = 0x0200;

        public const int WM_LBUTTONUP = 0x0202;

        public const int HT_CAPTION = 2;

        public const int HT_LEFT = 10;

        public const int HT_RIGHT = 11;

        public const int HT_TOP = 12;

        public const int HT_TOPLEFT = 13;

        public const int HT_TOPRIGHT = 14;

        public const int HT_BOTTOM = 15;

        public const int HT_BOTTOMLEFT = 16;

        public const int HT_BOTTOMRIGHT = 17;

        public const int VK_ESCAPE = 0x1B;

        static SnappingWindow()
        {
            Instances = new List<WeakReference<SnappingWindow>>();
        }

        private static IList<WeakReference<SnappingWindow>> Instances { get; set; }

        public static IEnumerable<SnappingWindow> Active
        {
            get
            {
                lock (Instances)
                {
                    return Instances
                        .Where(instance => instance != null && instance.IsAlive)
                        .Select(instance => instance.Target)
                        .ToArray();
                }
            }
        }

        protected static void OnActiveChanged(SnappingWindow sender)
        {
            if (ActiveChanged == null)
            {
                return;
            }
            ActiveChanged(sender, EventArgs.Empty);
        }

        public static event EventHandler ActiveChanged;


        public SnappingWindow(IntPtr handle)
        {
            this.Handle = handle;
        }

        public IntPtr Handle { get; private set; }

        public SnappingAdapter Adapter { get; private set; }

        private bool IsMoving;

        private bool IsResizing;

        private ResizeDirection ResizeDirection;

        public Point MouseOrigin;

        private Rectangle PreviousBounds;

        public override void InitializeComponent(ICore core)
        {
            this.Adapter = new SnappingAdapter(this.Handle);
            this.Adapter.InitializeComponent(core);
            this.SetHook(this.DefaultHook);

            lock (Instances)
            {
                Instances.Add(new WeakReference<SnappingWindow>(this));
            }
            OnActiveChanged(this);

            base.InitializeComponent(core);
        }

        protected virtual void SetHook(HwndSourceHook hook)
        {
            this.Adapter.RemoveHook(this.DefaultHook);
            this.Adapter.RemoveHook(this.MoveHook);
            this.Adapter.RemoveHook(this.ResizeHook);
            if (hook != null)
            {
                this.Adapter.AddHook(hook);
            }
        }

        protected virtual IntPtr DefaultHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case WM_NCLBUTTONDOWN:
                    this.Adapter.Activate();
                    if (this.OnNonClientLeftButtonDown(wParam.ToInt32()))
                    {
                        handled = true;
                        if (this.IsMoving || this.IsResizing)
                        {
                            return new IntPtr(1);
                        }
                    }
                    break;
            }

            return IntPtr.Zero;
        }

        protected virtual IntPtr MoveHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (!this.Adapter.Capture)
            {
                this.Cancel();
                return IntPtr.Zero;
            }

            switch (msg)
            {
                case WM_MOUSEMOVE:
                    this.Move();
                    break;
                case WM_LBUTTONUP:
                    this.EndMove();
                    break;
                case WM_KEYDOWN:
                    if (wParam.ToInt32() == VK_ESCAPE)
                    {
                        this.Adapter.Bounds = this.PreviousBounds;
                        this.Cancel();
                    }
                    break;
            }

            return IntPtr.Zero;
        }

        protected virtual IntPtr ResizeHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (!this.Adapter.Capture)
            {
                this.Cancel();
                return IntPtr.Zero;
            }

            switch (msg)
            {
                case WM_MOUSEMOVE:
                    this.Resize();
                    break;
                case WM_LBUTTONUP:
                    this.EndResize();
                    break;
                case WM_KEYDOWN:
                    if (wParam.ToInt32() == VK_ESCAPE)
                    {
                        this.Adapter.Bounds = this.PreviousBounds;
                        this.Cancel();
                    }
                    break;
            }

            return IntPtr.Zero;
        }

        private bool OnNonClientLeftButtonDown(int area)
        {
            switch (area)
            {
                case HT_CAPTION:
                    this.StartMove();
                    return true;
                case HT_TOPLEFT:
                    this.StartResize(ResizeDirection.Top | ResizeDirection.Left);
                    return true;
                case HT_TOP:
                    this.StartResize(ResizeDirection.Top);
                    return true;
                case HT_TOPRIGHT:
                    this.StartResize(ResizeDirection.Top | ResizeDirection.Right);
                    return true;
                case HT_RIGHT:
                    this.StartResize(ResizeDirection.Right);
                    return true;
                case HT_BOTTOMRIGHT:
                    this.StartResize(ResizeDirection.Bottom | ResizeDirection.Right);
                    return true;
                case HT_BOTTOM:
                    this.StartResize(ResizeDirection.Bottom);
                    return true;
                case HT_BOTTOMLEFT:
                    this.StartResize(ResizeDirection.Bottom | ResizeDirection.Left);
                    return true;
                case HT_LEFT:
                    this.StartResize(ResizeDirection.Left);
                    return true;
            }

            return false;
        }

        private void StartMove()
        {
            if (!this.Adapter.Capture)
            {
                this.Adapter.Capture = true;
            }

            this.MouseOrigin = GetMousePosition();
            this.PreviousBounds = this.Adapter.Bounds;
            this.ResizeDirection = ResizeDirection.None;
            this.SetHook(this.MoveHook);
        }

        private void Move()
        {
            var point = GetMousePosition();
            var bounds = this.Adapter.Bounds;
            var offset = Point.Empty;

            point.Offset(-this.MouseOrigin.X, -this.MouseOrigin.Y);
            point.Offset(this.PreviousBounds.X, this.PreviousBounds.Y);

            bounds.Location = point;

            //TODO: Use only WPF frameworks.
            var screen = global::System.Windows.Forms.Screen.FromHandle(this.Handle);
            var direction = SnappingHelper.Snap(bounds, screen.WorkingArea, ref offset, true);

            foreach (var snappingWindow in Active)
            {
                if (object.ReferenceEquals(snappingWindow, this))
                {
                    continue;
                }
                direction |= SnappingHelper.Snap(bounds, snappingWindow.Adapter.Bounds, ref offset, false);
            }

            if (direction.HasFlag(SnapDirection.Left) || direction.HasFlag(SnapDirection.Right))
            {
                bounds.X += offset.X;
            }
            if (direction.HasFlag(SnapDirection.Top) || direction.HasFlag(SnapDirection.Bottom))
            {
                bounds.Y += offset.Y;
            }

            if (this.Adapter.Bounds != bounds)
            {
                this.Adapter.Bounds = bounds;
            }
        }

        protected virtual void EndMove()
        {
            this.Cancel();
        }

        protected virtual void StartResize(ResizeDirection direction)
        {
            if (!this.Adapter.Capture)
            {
                this.Adapter.Capture = true;
            }

            this.MouseOrigin = GetMousePosition();
            this.PreviousBounds = this.Adapter.Bounds;
            this.ResizeDirection = direction;
            this.SetHook(this.ResizeHook);
        }

        protected virtual void Resize()
        {
            var point = GetMousePosition();
            var bounds = this.Adapter.Bounds;
            var offset = Point.Empty;

            point.Offset(-this.MouseOrigin.X, -this.MouseOrigin.Y);

            if ((ResizeDirection & ResizeDirection.Left) == ResizeDirection.Left)
            {
                bounds.X = this.PreviousBounds.X + point.X;
                bounds.Width = this.PreviousBounds.Width - point.X;
            }
            if ((ResizeDirection & ResizeDirection.Right) == ResizeDirection.Right)
            {
                bounds.Width = this.PreviousBounds.Width + point.X;
            }
            if ((ResizeDirection & ResizeDirection.Top) == ResizeDirection.Top)
            {
                bounds.Y = this.PreviousBounds.Y + point.Y;
                bounds.Height = this.PreviousBounds.Height - point.Y;
            }
            if ((ResizeDirection & ResizeDirection.Bottom) == ResizeDirection.Bottom)
            {
                bounds.Height = this.PreviousBounds.Height + point.Y;
            }

            //TODO: Use only WPF frameworks.
            var screen = global::System.Windows.Forms.Screen.FromHandle(this.Handle);
            var direction = SnappingHelper.Snap(bounds, screen.WorkingArea, ref offset, true);

            foreach (var snappingWindow in Active)
            {
                if (object.ReferenceEquals(snappingWindow, this))
                {
                    continue;
                }
                direction |= SnappingHelper.Snap(bounds, snappingWindow.Adapter.Bounds, ref offset, false);
            }

            if (direction.HasFlag(SnapDirection.Left))
            {
                bounds.X += offset.X;
                bounds.Width += -offset.X;
            }
            if (direction.HasFlag(SnapDirection.Right))
            {
                bounds.Width += offset.X;
            }
            if (direction.HasFlag(SnapDirection.Top))
            {
                bounds.Y += offset.Y;
                bounds.Height += -offset.Y;
            }
            if (direction.HasFlag(SnapDirection.Bottom))
            {
                bounds.Height += offset.Y;
            }

            if (this.Adapter.Bounds != bounds)
            {
                this.Adapter.Bounds = bounds;
            }
        }

        protected virtual void EndResize()
        {
            this.Cancel();
        }

        protected virtual void Cancel()
        {
            this.Adapter.Capture = false;
            this.IsMoving = false;
            this.IsResizing = false;
            this.ResizeDirection = ResizeDirection.None;

            this.SetHook(this.DefaultHook);
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
            this.SetHook(null);
            lock (Instances)
            {
                for (var a = Instances.Count - 1; a >= 0; a--)
                {
                    var instance = Instances[a];
                    if (instance == null || !instance.IsAlive)
                    {
                        Instances.RemoveAt(a);
                    }
                    else if (object.ReferenceEquals(this, instance.Target))
                    {
                        Instances.RemoveAt(a);
                    }
                }
            }
            OnActiveChanged(this);
        }

        ~SnappingWindow()
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

        public static Point GetMousePosition()
        {
            var x = default(int);
            var y = default(int);
            MouseHelper.GetPosition(out x, out y);
            return new Point(x, y);
        }
    }
}