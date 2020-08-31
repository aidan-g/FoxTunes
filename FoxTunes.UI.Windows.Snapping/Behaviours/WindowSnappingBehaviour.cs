using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.UserInterface)]
    public class WindowSnappingBehaviour : StandardBehaviour, IConfigurableComponent, IDisposable
    {
        public WindowSnappingBehaviour()
        {
            this.Windows = new ConditionalWeakTable<IUserInterfaceWindow, SnappingWindow>();
        }

        public bool Enabled { get; private set; }

        public ConditionalWeakTable<IUserInterfaceWindow, SnappingWindow> Windows { get; private set; }

        public ICore Core { get; private set; }

        public IUserInterface UserInterface { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Core = core;
            this.UserInterface = core.Components.UserInterface;
            this.UserInterface.WindowCreated += this.OnWindowCreated;
            this.UserInterface.WindowDestroyed += this.OnWindowDestroyed;
            this.Configuration = core.Components.Configuration;
            this.Configuration.GetElement<BooleanConfigurationElement>(
                WindowSnappingBehaviourConfiguration.SECTION,
                WindowSnappingBehaviourConfiguration.ENABLED
            ).ConnectValue(value =>
            {
                if (value)
                {
                    this.Enable();
                }
                else
                {
                    this.Disable();
                }
            });
            base.InitializeComponent(core);
        }

        protected virtual void OnWindowCreated(object sender, UserInterfaceWindowEventArgs e)
        {
            if (!this.Enabled)
            {
                return;
            }
            this.Enable(e.Window);
        }

        protected virtual void OnWindowDestroyed(object sender, UserInterfaceWindowEventArgs e)
        {
            if (!this.Enabled)
            {
                return;
            }
            this.Disable(e.Window);
        }

        public void Enable()
        {
            this.Enabled = true;
            foreach (var window in this.UserInterface.Windows)
            {
                this.Enable(window);
            }
        }

        protected virtual void Enable(IUserInterfaceWindow window)
        {
            var snappingWindow = new SnappingWindow(window.Handle);
            snappingWindow.InitializeComponent(this.Core);
            this.Windows.Add(window, snappingWindow);
        }

        public void Disable()
        {
            this.Enabled = false;
            foreach (var window in this.UserInterface.Windows)
            {
                this.Disable(window);
            }
        }

        protected virtual void Disable(IUserInterfaceWindow window)
        {
            var snappingWindow = default(SnappingWindow);
            if (!this.Windows.TryRemove(window, out snappingWindow))
            {
                return;
            }
            snappingWindow.Dispose();
        }

        public IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return WindowSnappingBehaviourConfiguration.GetConfigurationSections();
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
        }

        ~WindowSnappingBehaviour()
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
