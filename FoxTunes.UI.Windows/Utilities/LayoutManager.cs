using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.UserInterface)]
    public class LayoutManager : StandardComponent
    {
        public static readonly Type PLACEHOLDER = typeof(object);

        private static readonly Lazy<IEnumerable<UIComponent>> _Components = new Lazy<IEnumerable<UIComponent>>(() =>
        {
            var components = new List<UIComponent>();
            foreach (var type in ComponentScanner.Instance.GetComponents(typeof(IUIComponent)))
            {
                var attribute = default(UIComponentAttribute);
                if (!type.HasCustomAttribute<UIComponentAttribute>(false, out attribute))
                {
                    //We don't really want to expose components without annotations.
                    //attribute = new UIComponentAttribute(type.AssemblyQualifiedName, UIComponentSlots.NONE, type.Name);
                    continue;
                }
                components.Add(new UIComponent(attribute, type));
            }
            return components.OrderBy(
                component => component.Name
            ).ToArray();
        });

        public LayoutManager()
        {
            this.Providers = new HashSet<IUILayoutProvider>();
            Instance = this;
        }

        public HashSet<IUILayoutProvider> Providers { get; private set; }

        private IUILayoutProvider _Provider { get; set; }

        public IUILayoutProvider Provider
        {
            get
            {
                return this._Provider;
            }
            set
            {
                if (object.ReferenceEquals(this.Provider, value))
                {
                    return;
                }
                this._Provider = value;
                this.OnProviderChanged();
            }
        }

        protected virtual void OnProviderChanged()
        {
            if (this.ProviderChanged != null)
            {
                this.ProviderChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Provider");
        }

        public event EventHandler ProviderChanged;

        public IEnumerable<UIComponent> Components
        {
            get
            {
                return _Components.Value;
            }
        }

        public bool IsComponentActive(string id)
        {
            if (this.Provider == null)
            {
                return false;
            }
            return this.Provider.IsComponentActive(id);
        }

        public IConfiguration Configuration { get; private set; }

        public SelectionConfigurationElement Layout { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Configuration = core.Components.Configuration;
            this.Layout = this.Configuration.GetElement<SelectionConfigurationElement>(
                WindowsUserInterfaceConfiguration.SECTION,
                WindowsUserInterfaceConfiguration.LAYOUT_ELEMENT
            );
            this.Layout.ConnectValue(value => this.UpdateProvider());
            base.InitializeComponent(core);
        }

        protected virtual void UpdateProvider()
        {
            var provider = default(IUILayoutProvider);
            if (this.Layout != null && this.Layout.Value != null)
            {
                provider = this.Providers.FirstOrDefault(
                    _provider => string.Equals(_provider.Id, this.Layout.Value.Id, StringComparison.OrdinalIgnoreCase)
                );
            }
            if (provider == null)
            {
                provider = this.Providers.FirstOrDefault();
            }
            this.Provider = provider;
        }

        public UIComponent GetComponent(string id)
        {
            return this.Components.FirstOrDefault(component => string.Equals(component.Id, id, StringComparison.OrdinalIgnoreCase));
        }

        public void Register(IUILayoutProvider layoutProvider)
        {
            this.Providers.Add(layoutProvider);
            this.UpdateProvider();
        }

        public UIComponentBase Load(UILayoutTemplate template)
        {
            var provider = this.Provider;
            if (provider == null)
            {
                return null;
            }
            return provider.Load(template);
        }

        public static LayoutManager Instance { get; private set; }
    }
}
