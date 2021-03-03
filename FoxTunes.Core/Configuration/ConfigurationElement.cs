using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace FoxTunes
{
    public abstract class ConfigurationElement : INotifyPropertyChanged
    {
        protected static ILogger Logger
        {
            get
            {
                return LogManager.Logger;
            }
        }

        private ConfigurationElement()
        {
            this.IsHidden = false;
            this.Flags = ConfigurationElementFlags.None;
        }

        protected ConfigurationElement(string id, string name = null, string description = null, string path = null)
            : this()
        {
            this.Id = id;
            this.Name = name;
            this.Description = description;
            this.Path = path;
        }

        public string Id { get; private set; }

        public string Name { get; private set; }

        public string Description { get; private set; }

        public string Path { get; private set; }

        private bool _IsHidden { get; set; }

        public bool IsHidden
        {
            get
            {
                return this._IsHidden;
            }
            set
            {
                this._IsHidden = value;
                this.OnIsHiddenChanged();
            }
        }

        protected virtual void OnIsHiddenChanged()
        {
            if (this.IsHiddenChanged != null)
            {
                this.IsHiddenChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("IsHidden");
        }

        public event EventHandler IsHiddenChanged;

        private ObservableCollection<ValidationRule> _ValidationRules;

        public ObservableCollection<ValidationRule> ValidationRules
        {
            get
            {
                return this._ValidationRules;
            }
            set
            {
                this._ValidationRules = value;
                this.OnValidationRulesChanged();
            }
        }

        protected virtual void OnValidationRulesChanged()
        {
            if (this.ValidationRulesChanged != null)
            {
                this.ValidationRulesChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("ValidationRules");
        }

        public event EventHandler ValidationRulesChanged;

        public ConfigurationElement WithValidationRule(ValidationRule validationRule)
        {
            if (this.ValidationRules == null)
            {
                this.ValidationRules = new ObservableCollection<ValidationRule>();
            }
            this.ValidationRules.Add(validationRule);
            return this;
        }

        private ConfigurationElementFlags _Flags { get; set; }

        public ConfigurationElementFlags Flags
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

        public ConfigurationElement WithFlags(ConfigurationElementFlags flags)
        {
            this.Flags = flags;
            return this;
        }

        private ObservableCollection<Dependency> _Dependencies;

        public ObservableCollection<Dependency> Dependencies
        {
            get
            {
                return this._Dependencies;
            }
            private set
            {
                this._Dependencies = value;
                this.OnDependenciesChanged();
            }
        }

        protected virtual void OnDependenciesChanged()
        {
            if (this.DependenciesChanged != null)
            {
                this.DependenciesChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Dependencies");
        }

        public event EventHandler DependenciesChanged;

        public abstract void InitializeComponent();

        public virtual void Update(ConfigurationElement element)
        {
            if (string.IsNullOrEmpty(this.Name) && !string.IsNullOrEmpty(element.Name))
            {
                this.Name = element.Name;
            }
            if (string.IsNullOrEmpty(this.Description) && !string.IsNullOrEmpty(element.Description))
            {
                this.Description = element.Description;
            }
            if (string.IsNullOrEmpty(this.Path) && !string.IsNullOrEmpty(element.Path))
            {
                this.Path = element.Path;
            }
            if (element.ValidationRules != null && element.ValidationRules.Count > 0)
            {
                if (this.ValidationRules == null)
                {
                    this.ValidationRules = new ObservableCollection<ValidationRule>(element.ValidationRules);
                }
                else
                {
                    this.ValidationRules.AddRange(element.ValidationRules);
                }
            }
            this.Flags |= element.Flags;
            if (element.Dependencies != null && element.Dependencies.Count > 0)
            {
                if (this.Dependencies == null)
                {
                    this.Dependencies = new ObservableCollection<Dependency>(element.Dependencies);
                }
                else
                {
                    this.Dependencies.AddRange(element.Dependencies);
                }
            }
        }

        public ConfigurationElement Hide()
        {
            this.IsHidden = true;
            return this;
        }

        public ConfigurationElement Show()
        {
            this.IsHidden = false;
            return this;
        }

        public abstract bool IsModified { get; }

        public ConfigurationElement DependsOn(string sectionId, string elementId)
        {
            return this.DependsOn(new BooleanDependency(sectionId, elementId));
        }

        public ConfigurationElement DependsOn(string sectionId, string elementId, string optionId)
        {
            return this.DependsOn(new SelectionDependency(sectionId, elementId, optionId));
        }

        public ConfigurationElement DependsOn(Dependency dependency)
        {
            if (this.Dependencies == null)
            {
                this.Dependencies = new ObservableCollection<Dependency>();
            }
            this.Dependencies.Add(dependency);
            return this;
        }

        public abstract void Reset();

        public abstract string GetPersistentValue();

        public abstract void SetPersistentValue(string value);

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

    public abstract class ConfigurationElement<T> : ConfigurationElement
    {
        protected ConfigurationElement(string id, string name = null, string description = null, string path = null) : base(id, name, description, path)
        {

        }

        public T DefaultValue { get; private set; }

        private T _Value { get; set; }

        public virtual T Value
        {
            get
            {
                return this._Value;
            }
            set
            {
                if (EqualityComparer<T>.Default.Equals(this.Value, value))
                {
                    return;
                }
                this._Value = value;
                this.OnValueChanged();
            }
        }

        protected virtual void OnValueChanged()
        {
            if (this.ValueChanged != null)
            {
                this.ValueChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Value");
        }

        public event EventHandler ValueChanged;

        public ConfigurationElement<T> WithValue(T value)
        {
            this.Value = value;
            return this;
        }

        public ConfigurationElement<T> ConnectValue(Action<T> action)
        {
            var handler = new EventHandler((sender, e) =>
            {
                try
                {
                    action(this.Value);
                }
                catch (Exception exception)
                {
                    Logger.Write(
                        typeof(ConfigurationElement),
                        LogLevel.Warn,
                        "Failed to connect configuration value \"{0}\" = \"{1}\": {2}",
                        this.Id,
                        Convert.ToString(this.Value),
                        exception.Message
                    );
                }
            });
            handler(this, EventArgs.Empty);
            this.ValueChanged += handler;
            return this;
        }

        public override void InitializeComponent()
        {
            Logger.Write(typeof(ConfigurationSection), LogLevel.Trace, "Setting default value for configuration element \"{0}\": {1}", this.Name, Convert.ToString(this.Value));
            this.DefaultValue = this.Value;
        }

        public override void Update(ConfigurationElement element)
        {
            if (element is ConfigurationElement<T>)
            {
                this.Update(element as ConfigurationElement<T>);
            }
            base.Update(element);
        }

        protected virtual void Update(ConfigurationElement<T> element)
        {
            if (EqualityComparer<T>.Default.Equals(element.Value, default(T)))
            {
                return;
            }
            this.Value = element.Value;
        }

        public override void Reset()
        {
            this.Value = this.DefaultValue;
        }

        public override bool IsModified
        {
            get
            {
                if (EqualityComparer<T>.Default.Equals(this.Value, this.DefaultValue))
                {
                    return false;
                }
                return true;
            }
        }

        public override string GetPersistentValue()
        {
            return Convert.ToString(this.Value);
        }

        public override void SetPersistentValue(string value)
        {
            this.Value = (T)Convert.ChangeType(value, typeof(T));
        }
    }

    [Flags]
    public enum ConfigurationElementFlags : byte
    {
        None = 0,
        MultiLine = 1,
        FileName = 2,
        FolderName = 4
    }
}
