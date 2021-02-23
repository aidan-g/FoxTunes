using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace FoxTunes
{
    public abstract class ThemeBase : StandardComponent, ITheme
    {
        private ThemeBase()
        {
            this.ResourceDictionary = new Lazy<ResourceDictionary>(this.GetResourceDictionary);
            this.ResourceDictionaries = new Dictionary<ConfigurationElement, Lazy<ResourceDictionary>>();
        }

        protected ThemeBase(string id, string name = null, string description = null, ReleaseType releaseType = ReleaseType.Default) : this()
        {
            this.Id = id;
            this.Name = name;
            this.Description = description;
            this.ReleaseType = releaseType;
        }

        public Lazy<ResourceDictionary> ResourceDictionary { get; private set; }

        public IDictionary<ConfigurationElement, Lazy<ResourceDictionary>> ResourceDictionaries { get; private set; }

        public string Id { get; private set; }

        public string Name { get; private set; }

        public string Description { get; private set; }

        public ReleaseType ReleaseType { get; }

        public bool IsEnabled { get; private set; }

        public abstract ResourceDictionary GetResourceDictionary();

        public abstract Stream GetArtworkPlaceholder();

        public IConfiguration Configuration { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Configuration = core.Components.Configuration;
            base.InitializeComponent(core);
        }

        public void Enable()
        {
            try
            {
                //TODO: BeginInit/EndInit seem to be be buggy, not all changes are detected.
                //Application.Current.Resources.BeginInit();
                Application.Current.Resources.MergedDictionaries.Add(this.ResourceDictionary.Value);
                foreach (var pair in this.ResourceDictionaries)
                {
                    var element = pair.Key as BooleanConfigurationElement;
                    if (element == null)
                    {
                        continue;
                    }
                    if (element.Value)
                    {
                        Application.Current.Resources.MergedDictionaries.Add(pair.Value.Value);
                    }
                }
            }
            finally
            {
                //Application.Current.Resources.EndInit();
                this.IsEnabled = true;
            }
        }

        public void Disable()
        {
            try
            {
                //TODO: BeginInit/EndInit seem to be be buggy, not all changes are detected.
                //Application.Current.Resources.BeginInit();
                if (this.ResourceDictionary.IsValueCreated)
                {
                    Application.Current.Resources.MergedDictionaries.Remove(this.ResourceDictionary.Value);
                }
                foreach (var pair in this.ResourceDictionaries)
                {
                    if (pair.Value.IsValueCreated)
                    {
                        Application.Current.Resources.MergedDictionaries.Remove(pair.Value.Value);
                    }
                }
            }
            finally
            {
                //Application.Current.Resources.EndInit();
                this.IsEnabled = false;
            }
        }

        protected virtual void ConnectSetting(string sectionId, string elementId, Func<ResourceDictionary> resourceDictionary)
        {
            var element = this.Configuration.GetElement<BooleanConfigurationElement>(
                sectionId,
                elementId
            );
            this.ResourceDictionaries[element] = new Lazy<ResourceDictionary>(resourceDictionary);
            element.ValueChanged += this.OnValueChanged;
        }

        protected virtual void OnValueChanged(object sender, EventArgs e)
        {
            if (!this.IsEnabled)
            {
                return;
            }
            var element = sender as BooleanConfigurationElement;
            if (element == null)
            {
                return;
            }
            var resourceDictionary = default(Lazy<ResourceDictionary>);
            if (!this.ResourceDictionaries.TryGetValue(element, out resourceDictionary))
            {
                return;
            }
            if (element.Value)
            {
                Application.Current.Resources.MergedDictionaries.Add(resourceDictionary.Value);
            }
            else
            {
                if (resourceDictionary.IsValueCreated)
                {
                    Application.Current.Resources.MergedDictionaries.Remove(resourceDictionary.Value);
                }
            }
        }
    }
}
