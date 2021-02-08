using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace FoxTunes
{
    [Component("B889313D-4F21-4794-8D16-C2FAE6A7B305", ComponentSlots.UserInterface, priority: ComponentAttribute.PRIORITY_LOW)]
    public class WindowsUserInterface : UserInterface, IConfigurableComponent, IDisposable
    {
        public static readonly Type[] References = new[]
        {
            typeof(global::System.Windows.Interactivity.Interaction)
        };

        public WindowsUserInterface()
        {
            this.Application = new Application();
            this.Application.DispatcherUnhandledException += this.OnApplicationDispatcherUnhandledException;
            WindowBase.Created += this.OnWindowCreated;
            WindowBase.Destroyed += this.OnWindowDestroyed;
        }

        private Application _Application { get; set; }

        public Application Application
        {
            get
            {
                return this._Application;
            }
            private set
            {
                this._Application = value;
                this.OnApplicationChanged();
            }
        }

        protected virtual void OnApplicationChanged()
        {
            if (this.ApplicationChanged != null)
            {
                this.ApplicationChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Application");
        }

        public event EventHandler ApplicationChanged;

        public ICore Core { get; private set; }

        public IOutput Output { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Core = core;
            this.Output = core.Components.Output;
            base.InitializeComponent(core);
        }

        public override IEnumerable<IUserInterfaceWindow> Windows
        {
            get
            {
                return WindowBase.Active;
            }
        }

        public override Task Show()
        {
            if (global::FoxTunes.Windows.IsMiniWindowCreated)
            {
                global::FoxTunes.Windows.MiniWindow.DataContext = this.Core;
                this.Application.Run(global::FoxTunes.Windows.MiniWindow);
            }
            else
            {
                global::FoxTunes.Windows.MainWindow.DataContext = this.Core;
                this.Application.Run(global::FoxTunes.Windows.MainWindow);
            }
            return global::FoxTunes.Windows.Shutdown();
        }

        public override void Activate()
        {
            global::FoxTunes.Windows.Invoke(() =>
            {
                if (global::FoxTunes.Windows.ActiveWindow != null)
                {
                    if (global::FoxTunes.Windows.ActiveWindow.WindowState == WindowState.Minimized)
                    {
                        global::FoxTunes.Windows.ActiveWindow.WindowState = WindowState.Normal;
                    }
                    global::FoxTunes.Windows.ActiveWindow.Activate();
                }
            });
        }

        public override void Warn(string message)
        {
            MessageBox.Show(message, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        public override void Fatal(Exception exception)
        {
            var message = exception.Message + Environment.NewLine + Environment.NewLine + exception.StackTrace;
            MessageBox.Show(message, "Fatal", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public override bool Confirm(string message)
        {
            return MessageBox.Show(message, "Confirm", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK;
        }

        public override string Prompt(string message)
        {
            var result = default(string);
            //TODO: Bad .Wait().
            global::FoxTunes.Windows.Invoke(() => result = InputBox.ShowDialog(message)).Wait();
            return result;
        }

        public override void Restart()
        {
            MessageBox.Show("Restart is required.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        protected virtual void OnApplicationDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Logger.Write(this, LogLevel.Fatal, e.Exception.Message, e);
            //Don't crash out.
            e.Handled = true;
        }

        protected virtual void OnWindowCreated(object sender, EventArgs e)
        {
            var window = sender as WindowBase;
            if (window == null)
            {
                return;
            }
            this.OnWindowCreated(window);
        }

        protected virtual void OnWindowDestroyed(object sender, EventArgs e)
        {
            var window = sender as WindowBase;
            if (window == null)
            {
                return;
            }
            this.OnWindowDestroyed(window);
        }

        public IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return WindowsUserInterfaceConfiguration.GetConfigurationSections();
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
            WindowBase.Created -= this.OnWindowCreated;
            WindowBase.Destroyed -= this.OnWindowDestroyed;
        }

        ~WindowsUserInterface()
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
    }
}
