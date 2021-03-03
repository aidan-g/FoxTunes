using FoxTunes.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace FoxTunes
{
    public class ConfigurationSection : INotifyPropertyChanged
    {
        protected static ILogger Logger
        {
            get
            {
                return LogManager.Logger;
            }
        }

        public ConfigurationSection()
        {
            this.Elements = new ObservableCollection<ConfigurationElement>();
            this.Flags = ConfigurationSectionFlags.None;
        }

        public ConfigurationSection(string id, string name = null, string description = null)
            : this()
        {
            this.Id = id;
            this.Name = name;
            this.Description = description;
        }

        public string Id { get; set; }

        public string Name { get; private set; }

        public string Description { get; private set; }

        public ObservableCollection<ConfigurationElement> Elements { get; private set; }

        public bool IsInitialized { get; private set; }

        public ConfigurationSection WithElement(ConfigurationElement element)
        {
            if (this.Contains(element.Id))
            {
                this.Update(element);
            }
            else
            {
                this.Add(element);
            }
            return this;
        }

        public bool Contains(string id)
        {
            return this.GetElement(id) != null;
        }

        private ConfigurationSectionFlags _Flags { get; set; }

        public ConfigurationSectionFlags Flags
        {
            get
            {
                return this._Flags;
            }
            set
            {
                this._Flags = value;
                this.OnFlagsChanged();
            }
        }

        protected virtual void OnFlagsChanged()
        {
            if (this.FlagsChanged != null)
            {
                this.FlagsChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Flags");
        }

        public event EventHandler FlagsChanged;

        public ConfigurationSection WithFlags(ConfigurationSectionFlags flags)
        {
            this.Flags = flags;
            return this;
        }

        public void InitializeComponent()
        {
            foreach (var element in this.Elements)
            {
                element.InitializeComponent();
            }
            this.IsInitialized = true;
        }

        public void Update(ConfigurationSection section)
        {
            if (string.IsNullOrEmpty(this.Name) && !string.IsNullOrEmpty(section.Name))
            {
                this.Name = section.Name;
            }
            if (string.IsNullOrEmpty(this.Description) && !string.IsNullOrEmpty(section.Description))
            {
                this.Description = section.Description;
            }
            this.Flags |= section.Flags;
            foreach (var element in section.Elements)
            {
                if (!this.Contains(element.Id))
                {
                    this.Add(element);
                }
                else
                {
                    this.Update(element);
                }
            }
        }

        private void Add(ConfigurationElement element)
        {
            Logger.Write(typeof(ConfigurationSection), LogLevel.Debug, "Adding configuration element: {0} => {1}", element.Id, element.Name);
            this.Elements.Add(element);
        }

        private void Update(ConfigurationElement element)
        {
            Logger.Write(typeof(ConfigurationSection), LogLevel.Debug, "Updating configuration element: {0} => {1}", element.Id, element.Name);
            var existing = this.GetElement(element.Id);
            existing.Update(element);
        }

        private void Hide(ConfigurationElement element)
        {
            Logger.Write(typeof(ConfigurationSection), LogLevel.Debug, "Hiding configuration element: {0} => {1}", element.Id, element.Name);
            var existing = this.GetElement(element.Id);
            existing.Hide();
        }

        public T GetElement<T>(string elementId) where T : ConfigurationElement
        {
            return this.GetElement(elementId) as T;
        }

        public ConfigurationElement GetElement(string elementId)
        {
            return this.Elements.FirstOrDefault(element => string.Equals(element.Id, elementId, StringComparison.OrdinalIgnoreCase));
        }

        public void Reset()
        {
            foreach (var element in this.Elements)
            {
                element.Reset();
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged == null)
            {
                return;
            }
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    [Flags]
    public enum ConfigurationSectionFlags : byte
    {
        None = 0,
        System = 1
    }
}
