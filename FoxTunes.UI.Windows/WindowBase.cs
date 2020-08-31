using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Linq;
using FoxTunes.Interfaces;

#if NET40
using Microsoft.Windows.Shell;
#else
using System.Windows.Shell;
#endif


namespace FoxTunes
{
    public abstract class WindowBase : Window, IUserInterfaceWindow
    {
        static WindowBase()
        {
            Instances = new List<WeakReference<WindowBase>>();
        }

        private static IList<WeakReference<WindowBase>> Instances { get; set; }

        public static IEnumerable<WindowBase> Active
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

        protected static void OnActiveChanged(WindowBase sender)
        {
            if (ActiveChanged == null)
            {
                return;
            }
            ActiveChanged(sender, EventArgs.Empty);
        }

        public static event EventHandler ActiveChanged;

        public WindowBase()
        {
            this.InitializeComponent();
        }

        new protected virtual bool ApplyTemplate
        {
            get
            {
                return true;
            }
        }

        private void InitializeComponent()
        {
            this.WindowStyle = WindowStyle.None;
            this.AllowsTransparency = true;
            this.Background = Brushes.Transparent;
            if (this.ApplyTemplate)
            {
                this.Template = TemplateFactory.Template;
                this.SetValue(WindowChrome.WindowChromeProperty, new WindowChrome()
                {
                    CaptionHeight = 30,
                    ResizeBorderThickness = new Thickness(5)
                });
            }
        }

        protected override void OnContentRendered(EventArgs e)
        {
            if (this.Handle == IntPtr.Zero)
            {
                this.Handle = this.GetHandle();
                lock (Instances)
                {
                    Instances.Add(new WeakReference<WindowBase>(this));
                }
                OnActiveChanged(this);
                if (Created != null)
                {
                    Created(this, EventArgs.Empty);
                }
            }
            base.OnContentRendered(e);
        }

        protected override void OnClosed(EventArgs e)
        {
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
            if (Destroyed != null)
            {
                Destroyed(this, EventArgs.Empty);
            }
            base.OnClosed(e);
        }

        public static event EventHandler Created;

        public static event EventHandler Destroyed;

        #region IUserInterfaceWindow

        public virtual IntPtr Handle { get; private set; }

        public virtual UserInterfaceWindowRole Role
        {
            get
            {
                return UserInterfaceWindowRole.None;
            }
        }

        #endregion

        private static class TemplateFactory
        {
            private static Lazy<ControlTemplate> _Template = new Lazy<ControlTemplate>(GetTemplate);

            public static ControlTemplate Template
            {
                get
                {
                    return _Template.Value;
                }
            }

            private static ControlTemplate GetTemplate()
            {
                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(global::FoxTunes.Resources.WindowBase)))
                {
                    return (ControlTemplate)XamlReader.Load(stream);
                }
            }
        }
    }
}
