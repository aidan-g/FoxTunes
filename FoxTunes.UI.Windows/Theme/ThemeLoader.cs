using FoxTunes.Interfaces;
using System;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.UserInterface)]
    public class ThemeLoader : StandardComponent
    {
        private Lazy<ITheme> _Theme { get; set; }

        public ITheme Theme
        {
            get
            {
                if (this._Theme.Value == null)
                {
                    return null;
                }
                return this._Theme.Value;
            }
        }

        protected virtual void SetTheme(Func<ITheme> factory)
        {
            if (this._Theme != null && this._Theme.IsValueCreated)
            {
                this._Theme.Value.Disable();
            }
            this._Theme = new Lazy<ITheme>(() =>
            {
                var theme = factory();
                theme.Enable();
                return theme;
            });
            this.OnThemeChanged();
        }

        protected virtual void OnThemeChanged()
        {
            if (this.ThemeChanged != null)
            {
                this.ThemeChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Theme");
        }

        public event EventHandler ThemeChanged;

        public IConfiguration Configuration { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Configuration = core.Components.Configuration;
            this.Configuration.GetElement<SelectionConfigurationElement>(
                WindowsUserInterfaceConfiguration.SECTION,
                WindowsUserInterfaceConfiguration.THEME_ELEMENT
            ).ConnectValue(value => this.SetTheme(() => WindowsUserInterfaceConfiguration.GetTheme(value)));
            base.InitializeComponent(core);
        }

        public void EnsureTheme()
        {
            var theme = this.Theme;
        }
    }
}
