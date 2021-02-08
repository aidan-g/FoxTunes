using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.UserInterface)]
    public class ToolWindowBehaviour : StandardBehaviour, IInvocableComponent, IConfigurableComponent, IDisposable
    {
        public const string NEW = "BBBB";

        public const string MANAGE = "CCCC";

        static ToolWindowBehaviour()
        {
            Reset();
        }

        public ToolWindowBehaviour()
        {
            this.Windows = new Dictionary<ToolWindowConfiguration, ToolWindow>();
        }

        public bool Enabled
        {
            get
            {
                return this.Layout != null && string.Equals(this.Layout.Value.Id, UIComponentLayoutProviderConfiguration.ID);
            }
        }

        public ICore Core { get; private set; }

        public LayoutDesignerBehaviour LayoutDesignerBehaviour { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public SelectionConfigurationElement Layout { get; private set; }

        public TextConfigurationElement Element { get; private set; }

        public IDictionary<ToolWindowConfiguration, ToolWindow> Windows { get; private set; }

        public bool IsLoaded { get; private set; }

        protected virtual async Task Load()
        {
            //Whatever happens we're ready.
            this.IsLoaded = true;
            if (!string.IsNullOrEmpty(this.Element.Value))
            {
                try
                {
                    using (var stream = new MemoryStream(Encoding.Default.GetBytes(this.Element.Value)))
                    {
                        var configs = Serializer.LoadWindows(stream);
                        foreach (var config in configs)
                        {
                            await this.Load(config).ConfigureAwait(false);
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.Write(this, LogLevel.Warn, "Failed to load config: {0}", e.Message);
                }
            }
        }

        protected virtual async Task<ToolWindow> Load(ToolWindowConfiguration config)
        {
            try
            {
                if (!ScreenHelper.WindowBoundsVisible(new Rect(config.Left, config.Top, config.Width, config.Height)))
                {
                    //Prevent window from opening off screen.
                    config.Left = 0;
                    config.Top = 0;
                }
                var window = default(ToolWindow);
                await global::FoxTunes.Windows.Invoke(() =>
                {
                    window = new ToolWindow();
                    window.DataContext = this.Core;
                    window.Configuration = config;
                    window.ShowActivated = false;
                    //Don't set owner as it causes an "always on top" type of behaviour.
                    //We have an option for that.
                    //window.Owner = global::FoxTunes.Windows.ActiveWindow;
                    window.Closed += this.OnClosed;
                }).ConfigureAwait(false);
                this.Windows[config] = window;
                this.OnLoaded(config, window);
                return window;
            }
            catch (Exception e)
            {
                Logger.Write(this, LogLevel.Warn, "Failed to load config: {0}", e.Message);
                return null;
            }
        }

        protected virtual void OnLoaded(ToolWindowConfiguration config, ToolWindow window)
        {
            if (this.Loaded == null)
            {
                return;
            }
            this.Loaded(this, new ToolWindowConfigurationEventArgs(config, window));
        }

        public event ToolWindowConfigurationEventHandler Loaded;

        protected virtual async Task Show()
        {
            foreach (var pair in this.Windows)
            {
                await this.Show(pair.Key, pair.Value).ConfigureAwait(false);
            }
        }

        protected virtual Task Show(ToolWindowConfiguration config, ToolWindow window)
        {
            var show = false;
            var main = global::FoxTunes.Windows.IsMainWindowCreated && global::FoxTunes.Windows.MainWindow.IsVisible;
            var mini = global::FoxTunes.Windows.IsMiniWindowCreated && global::FoxTunes.Windows.MiniWindow.IsVisible;
            if (config.ShowWithMainWindow && main)
            {
                show = true;
            }
            else if (config.ShowWithMiniWindow && mini)
            {
                show = true;
            }
            if (show)
            {
                return global::FoxTunes.Windows.Invoke(window.Show);
            }
            else
            {
                return global::FoxTunes.Windows.Invoke(window.Hide);
            }
        }

        protected virtual void Unload(ToolWindowConfiguration config, ToolWindow window)
        {
            UIDisposer.Dispose(window);
            this.Windows.TryGetValue(config, out window);
            if (!this.Windows.Remove(config))
            {
                return;
            }
            this.OnUnloaded(config, window);
        }

        protected virtual void OnUnloaded(ToolWindowConfiguration config, ToolWindow window)
        {
            if (this.Unloaded == null)
            {
                return;
            }
            this.Unloaded(this, new ToolWindowConfigurationEventArgs(config, window));
        }

        public event ToolWindowConfigurationEventHandler Unloaded;

        protected virtual void Save()
        {
            if (this.Configuration == null || this.Element == null)
            {
                return;
            }
            if (!this.Windows.Keys.Any())
            {
                this.Element.Value = null;
                return;
            }
            try
            {
                using (var stream = new MemoryStream())
                {
                    Serializer.Save(stream, this.Windows.Keys);
                    this.Element.Value = Encoding.Default.GetString(stream.ToArray());
                }
                this.Configuration.Save();
            }
            catch (Exception e)
            {
                Logger.Write(this, LogLevel.Warn, "Failed to save config: {0}", e.Message);
            }
        }

        protected virtual void OnClosed(object sender, EventArgs e)
        {
            var window = sender as ToolWindow;
            if (window == null)
            {
                return;
            }
            this.Unload(window.Configuration, window);
        }

        public override void InitializeComponent(ICore core)
        {
            global::FoxTunes.LayoutManager.Instance.ProviderChanged += this.OnProviderChanged;
            global::FoxTunes.Windows.ActiveWindowChanged += this.OnActiveWindowChanged;
            global::FoxTunes.Windows.ShuttingDown += this.OnShuttingDown;
            this.Core = core;
            this.LayoutDesignerBehaviour = ComponentRegistry.Instance.GetComponent<LayoutDesignerBehaviour>();
            this.LayoutDesignerBehaviour.IsDesigningChanged += this.OnIsDesigningChanged;
            this.Configuration = core.Components.Configuration;
            this.Layout = this.Configuration.GetElement<SelectionConfigurationElement>(
                WindowsUserInterfaceConfiguration.SECTION,
                WindowsUserInterfaceConfiguration.LAYOUT_ELEMENT
            );
            this.Element = this.Configuration.GetElement<TextConfigurationElement>(
                ToolWindowBehaviourConfiguration.SECTION,
                ToolWindowBehaviourConfiguration.ELEMENT
            );
            if (this.Enabled)
            {
                var task = this.Load();
            }
            base.InitializeComponent(core);
        }

        protected virtual void OnProviderChanged(object sender, EventArgs e)
        {
            if (this.Enabled)
            {
                this.Dispatch(async () =>
                {
                    if (!this.IsLoaded)
                    {
                        await this.Load().ConfigureAwait(false);
                    }
                    await this.Show().ConfigureAwait(false);
                });
            }
            else if (this.IsLoaded)
            {
                this.Save();
                var task = this.Shutdown();
            }
        }

        protected virtual void OnActiveWindowChanged(object sender, EventArgs e)
        {
            if (!this.Enabled)
            {
                return;
            }
            this.Dispatch(async () =>
            {
                if (!this.IsLoaded)
                {
                    await this.Load().ConfigureAwait(false);
                }
                await this.Show().ConfigureAwait(false);
            });
        }

        protected virtual void OnShuttingDown(object sender, EventArgs e)
        {
            if (!this.IsLoaded)
            {
                return;
            }
            this.Save();
            var task = this.Shutdown();
        }

        protected virtual void OnIsDesigningChanged(object sender, EventArgs e)
        {
            if (this.LayoutDesignerBehaviour.IsDesigning)
            {
                return;
            }
            this.Save();
        }

        public IEnumerable<IInvocationComponent> Invocations
        {
            get
            {
                if (this.Enabled)
                {
                    yield return new InvocationComponent(InvocationComponent.CATEGORY_SETTINGS, NEW, "New Window");
                    yield return new InvocationComponent(InvocationComponent.CATEGORY_SETTINGS, MANAGE, "Manage Windows", attributes: InvocationComponent.ATTRIBUTE_SEPARATOR);
                }
            }
        }

        public Task InvokeAsync(IInvocationComponent component)
        {
            switch (component.Id)
            {
                case NEW:
                    return this.New();
                case MANAGE:
                    return this.Manage();
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        public async Task<ToolWindowConfiguration> New()
        {
            var configs = this.Windows.Keys;
            var config = new ToolWindowConfiguration()
            {
                Title = ToolWindowConfiguration.GetTitle(configs),
                Width = 400,
                Height = 250,
                ShowWithMainWindow = true,
                ShowWithMiniWindow = false
            };
            var window = await this.Load(config).ConfigureAwait(false);
            if (window == null)
            {
                return null;
            }
            await this.Show(config, window).ConfigureAwait(false);
            return config;
        }

        public Task Manage()
        {
            return global::FoxTunes.Windows.Invoke(() =>
            {
                if (!IsToolWindowManagerWindowCreated)
                {
                    ToolWindowManagerWindow.DataContext = this.Core;
                    ToolWindowManagerWindow.Show();
                }
            });
        }

        public Task Shutdown()
        {
            this.IsLoaded = false;
            return global::FoxTunes.Windows.Invoke(() =>
            {
                if (IsToolWindowManagerWindowCreated)
                {
                    ToolWindowManagerWindow.Close();
                }
                foreach (var window in this.Windows.Values)
                {
                    window.Closed -= this.OnClosed;
                    window.Close();
                }
                this.Windows.Clear();
            });
        }

        public async Task Refresh()
        {
            foreach (var pair in this.Windows)
            {
                await this.Show(pair.Key, pair.Value).ConfigureAwait(false);
            }
        }

        public IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return ToolWindowBehaviourConfiguration.GetConfigurationSections();
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
            global::FoxTunes.LayoutManager.Instance.ProviderChanged -= this.OnProviderChanged;
            global::FoxTunes.Windows.ActiveWindowChanged -= this.OnActiveWindowChanged;
            global::FoxTunes.Windows.ShuttingDown -= this.OnShuttingDown;
        }

        ~ToolWindowBehaviour()
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

        private static Lazy<Window> _ToolWindowManagerWindow { get; set; }

        public static bool IsToolWindowManagerWindowCreated
        {
            get
            {
                return _ToolWindowManagerWindow.IsValueCreated;
            }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public static Window ToolWindowManagerWindow
        {
            get
            {
                var raiseEvent = !IsToolWindowManagerWindowCreated;
                try
                {
                    return _ToolWindowManagerWindow.Value;
                }
                finally
                {
                    if (IsToolWindowManagerWindowCreated && raiseEvent)
                    {
                        OnToolWindowManagerWindowCreated();
                    }
                }
            }
        }

        private static void OnToolWindowManagerWindowCreated()
        {
            ToolWindowManagerWindow.Closed += OnToolWindowManagerWindowClosed;
            if (ToolWindowManagerWindowCreated == null)
            {
                return;
            }
            ToolWindowManagerWindowCreated(ToolWindowManagerWindow, EventArgs.Empty);
        }

        public static event EventHandler ToolWindowManagerWindowCreated;

        private static void OnToolWindowManagerWindowClosed(object sender, EventArgs e)
        {
            if (IsToolWindowManagerWindowCreated)
            {
                UIDisposer.Dispose(ToolWindowManagerWindow);
            }
            _ToolWindowManagerWindow = new Lazy<Window>(() => new ToolWindowManagerWindow() { Owner = global::FoxTunes.Windows.ActiveWindow });
            if (ToolWindowManagerWindowClosed == null)
            {
                return;
            }
            ToolWindowManagerWindowClosed(typeof(ToolWindowManagerWindow), EventArgs.Empty);
        }

        public static event EventHandler ToolWindowManagerWindowClosed;

        private static void Reset()
        {
            _ToolWindowManagerWindow = new Lazy<Window>(() => new ToolWindowManagerWindow() { Owner = global::FoxTunes.Windows.ActiveWindow });
        }
    }

    public class ToolWindowConfiguration
    {
        private string _Title { get; set; }

        public string Title
        {
            get
            {
                return this._Title;
            }
            set
            {
                this._Title = value;
                this.OnTitleChanged();
            }
        }

        protected virtual void OnTitleChanged()
        {
            if (this.TitleChanged != null)
            {
                this.TitleChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Title");
        }

        public event EventHandler TitleChanged;

        private int _Left { get; set; }

        public int Left
        {
            get
            {
                return this._Left;
            }
            set
            {
                this._Left = value;
                this.OnLeftChanged();
            }
        }

        protected virtual void OnLeftChanged()
        {
            if (this.LeftChanged != null)
            {
                this.LeftChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Left");
        }

        public event EventHandler LeftChanged;

        private int _Top { get; set; }

        public int Top
        {
            get
            {
                return this._Top;
            }
            set
            {
                this._Top = value;
                this.OnTopChanged();
            }
        }

        protected virtual void OnTopChanged()
        {
            if (this.TopChanged != null)
            {
                this.TopChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Top");
        }

        public event EventHandler TopChanged;

        private int _Width { get; set; }

        public int Width
        {
            get
            {
                return this._Width;
            }
            set
            {
                this._Width = value;
                this.OnWidthChanged();
            }
        }

        protected virtual void OnWidthChanged()
        {
            if (this.WidthChanged != null)
            {
                this.WidthChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Width");
        }

        public event EventHandler WidthChanged;

        private int _Height { get; set; }

        public int Height
        {
            get
            {
                return this._Height;
            }
            set
            {
                this._Height = value;
                this.OnHeightChanged();
            }
        }

        protected virtual void OnHeightChanged()
        {
            if (this.HeightChanged != null)
            {
                this.HeightChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Height");
        }

        public event EventHandler HeightChanged;

        private UIComponentConfiguration _Component { get; set; }

        public UIComponentConfiguration Component
        {
            get
            {
                return this._Component;
            }
            set
            {
                this._Component = value;
                this.OnComponentChanged();
            }
        }

        protected virtual void OnComponentChanged()
        {
            if (this.ComponentChanged != null)
            {
                this.ComponentChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Component");
        }

        public event EventHandler ComponentChanged;

        private bool _ShowWithMainWindow { get; set; }

        public bool ShowWithMainWindow
        {
            get
            {
                return this._ShowWithMainWindow;
            }
            set
            {
                this._ShowWithMainWindow = value;
                this.OnShowWithMainWindowChanged();
            }
        }

        protected virtual void OnShowWithMainWindowChanged()
        {
            if (this.ShowWithMainWindowChanged != null)
            {
                this.ShowWithMainWindowChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("ShowWithMainWindow");
        }

        public event EventHandler ShowWithMainWindowChanged;

        private bool _ShowWithMiniWindow { get; set; }

        public bool ShowWithMiniWindow
        {
            get
            {
                return this._ShowWithMiniWindow;
            }
            set
            {
                this._ShowWithMiniWindow = value;
                this.OnShowWithMiniWindowChanged();
            }
        }

        protected virtual void OnShowWithMiniWindowChanged()
        {
            if (this.ShowWithMiniWindowChanged != null)
            {
                this.ShowWithMiniWindowChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("ShowWithMiniWindow");
        }

        public event EventHandler ShowWithMiniWindowChanged;

        private bool _AlwaysOnTop { get; set; }

        public bool AlwaysOnTop
        {
            get
            {
                return this._AlwaysOnTop;
            }
            set
            {
                this._AlwaysOnTop = value;
                this.OnAlwaysOnTopChanged();
            }
        }

        protected virtual void OnAlwaysOnTopChanged()
        {
            if (this.AlwaysOnTopChanged != null)
            {
                this.AlwaysOnTopChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("AlwaysOnTop");
        }

        public event EventHandler AlwaysOnTopChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged == null)
            {
                return;
            }
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public static string GetTitle(IEnumerable<ToolWindowConfiguration> configs)
        {
            var title = "New Window";
            for (var a = 1; a < 100; a++)
            {
                var success = true;
                foreach (var config in configs)
                {
                    if (string.Equals(config.Title, title, StringComparison.OrdinalIgnoreCase))
                    {
                        title = string.Format("New Window ({0})", a);
                        success = false;
                        break;
                    }
                }
                if (success)
                {
                    return title;
                }
            }
            return title;
        }
    }

    public delegate void ToolWindowConfigurationEventHandler(object sender, ToolWindowConfigurationEventArgs e);

    public class ToolWindowConfigurationEventArgs : EventArgs
    {
        public ToolWindowConfigurationEventArgs(ToolWindowConfiguration configuration, ToolWindow window)
        {
            this.Configuration = configuration;
            this.Window = window;
        }

        public ToolWindowConfiguration Configuration { get; private set; }

        public ToolWindow Window { get; private set; }
    }
}
