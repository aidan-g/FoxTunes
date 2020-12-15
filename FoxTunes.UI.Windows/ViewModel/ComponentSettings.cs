using FoxTunes.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace FoxTunes.ViewModel
{
    public class ComponentSettings : ViewModelBase
    {
        public ComponentSettings()
        {
            this.Sections = new ObservableCollection<ConfigurationSection>();
        }

        public IConfiguration Configuration { get; private set; }

        public ObservableCollection<ConfigurationSection> Sections { get; private set; }

        private ConfigurationSection _SelectedSection { get; set; }

        public ConfigurationSection SelectedSection
        {
            get
            {
                return this._SelectedSection;
            }
            set
            {
                if (object.ReferenceEquals(this._SelectedSection, value))
                {
                    return;
                }
                this.OnSelectedSectionChanging();
                this._SelectedSection = value;
                this.OnSelectedSectionChanged();
            }
        }

        protected virtual void OnSelectedSectionChanging()
        {
            if (this.SelectedSection != null)
            {
                this.SelectedSection.IsSelected = false;
            }
            if (this.SelectedSectionChanging != null)
            {
                this.SelectedSectionChanging(this, EventArgs.Empty);
            }
            this.OnPropertyChanging("SelectedSection");
        }

        public event EventHandler SelectedSectionChanging;

        protected virtual void OnSelectedSectionChanged()
        {
            if (this.SelectedSection != null)
            {
                this.SelectedSection.IsSelected = true;
            }
            if (this.SelectedSectionChanged != null)
            {
                this.SelectedSectionChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("SelectedSection");
        }

        public event EventHandler SelectedSectionChanged;

        private string _Filter { get; set; }

        public string Filter
        {
            get
            {
                return this._Filter;
            }
            set
            {
                if (string.Equals(this._Filter, value, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }
                this._Filter = value;
                this.OnFilterChanged();
            }
        }

        protected virtual void OnFilterChanged()
        {
            this.Dispatch(this.Refresh);
            if (this.FilterChanged != null)
            {
                this.FilterChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Filter");
        }

        public event EventHandler FilterChanged;

        public ICommand ResetAllCommand
        {
            get
            {
                return CommandFactory.Instance.CreateCommand(
                     () => this.Configuration.Reset(),
                     () => this.Configuration != null
                 );
            }
        }

        public ICommand ResetPageCommand
        {
            get
            {
                return new Command(
                    () => this.SelectedSection.Reset(),
                    () => this.Configuration != null && this.SelectedSection != null
                );
            }
        }

        public ICommand SaveCommand
        {
            get
            {
                var command = new Command(
                    () => this.Configuration.Save(),
                    () => this.Configuration != null
                );
                command.Tag = CommandHints.DISMISS;
                return command;
            }
        }

        public override void InitializeComponent(ICore core)
        {
            this.Configuration = this.Core.Components.Configuration;
            this.Dispatch(this.Refresh);
            base.InitializeComponent(core);
        }

        protected virtual Task Refresh()
        {
            return Windows.Invoke(() =>
            {
                this.Sections.Clear();
                foreach (var section in this.Configuration.Sections.OrderBy(section => section.Id))
                {
                    if (section.Flags.HasFlag(ConfigurationSectionFlags.System))
                    {
                        //System config should not be presented to the user.
                        continue;
                    }
                    if (!this.MatchesFilter(section, true))
                    {
                        continue;
                    }
                    this.Sections.Add(new ConfigurationSection(this, section));
                }
                this.SelectedSection = this.Sections.FirstOrDefault();
            });
        }

        public virtual bool MatchesFilter(global::FoxTunes.ConfigurationSection section, bool elements)
        {
            if (string.IsNullOrEmpty(this.Filter))
            {
                return true;
            }
            var values = new[] { section.Name, section.Description };
            foreach (var value in values)
            {
                if (!string.IsNullOrEmpty(value) && value.Contains(this.Filter, true))
                {
                    return true;
                }
            }
            if (elements)
            {
                return section.Elements.Any(element => this.MatchesFilter(element));
            }
            else
            {
                return false;
            }
        }

        public virtual bool MatchesFilter(global::FoxTunes.ConfigurationElement element)
        {
            if (string.IsNullOrEmpty(this.Filter))
            {
                return true;
            }
            var values = new[] { element.Name, element.Description, element.Path };
            foreach (var value in values)
            {
                if (!string.IsNullOrEmpty(value) && value.Contains(this.Filter, true))
                {
                    return true;
                }
            }
            return false;
        }

        protected override Freezable CreateInstanceCore()
        {
            return new Settings();
        }
    }

    public class ConfigurationSection : ViewModelBase, ISelectable, IExpandable
    {
        private ConfigurationSection()
        {
            this.Elements = new ObservableCollection<ConfigurationElement>();
            this.Children = new ObservableCollection<ConfigurationSection>();
            this.IsExpanded = true;
        }

        public ConfigurationSection(ComponentSettings componentSettings, global::FoxTunes.ConfigurationSection section, string path = null) : this()
        {
            this.ComponentSettings = componentSettings;
            this.Section = section;
            this.Path = path;
            if (string.IsNullOrEmpty(this.Path))
            {
                var elements = this.Section.Elements
                    //Always put "Advanced" settings last.
                    .OrderBy(element => string.IsNullOrEmpty(element.Path) || element.Path.Contains("Advanced", true))
                    .ThenBy(element => element.Id);
                foreach (var element in elements)
                {
                    if (!componentSettings.MatchesFilter(this.Section, false) && !componentSettings.MatchesFilter(element))
                    {
                        continue;
                    }
                    this.AddElement(element);
                }
            }
        }

        public ComponentSettings ComponentSettings { get; private set; }

        public ObservableCollection<ConfigurationElement> Elements { get; private set; }

        public ObservableCollection<ConfigurationSection> Children { get; private set; }

        public global::FoxTunes.ConfigurationSection Section { get; private set; }

        public string Path { get; private set; }

        public string Name
        {
            get
            {
                if (string.IsNullOrEmpty(this.Path))
                {
                    return this.Section.Name;
                }
                return this.Path;
            }
        }

        private bool _IsExpanded { get; set; }

        public bool IsExpanded
        {
            get
            {
                return this._IsExpanded;
            }
            set
            {
                this._IsExpanded = value;
                this.OnIsExpandedChanged();
            }
        }

        protected virtual void OnIsExpandedChanged()
        {
            if (this.IsExpandedChanged != null)
            {
                this.IsExpandedChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("IsExpanded");
        }

        public event EventHandler IsExpandedChanged;

        private bool _IsSelected { get; set; }

        public bool IsSelected
        {
            get
            {
                return this._IsSelected;
            }
            set
            {
                this._IsSelected = value;
                this.OnIsSelectedChanged();
            }
        }

        protected virtual void OnIsSelectedChanged()
        {
            if (this.IsSelectedChanged != null)
            {
                this.IsSelectedChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("IsSelected");
        }

        public event EventHandler IsSelectedChanged;

        public bool HasElements
        {
            get
            {
                return this.Elements.Any();
            }
        }

        protected virtual void AddElement(ConfigurationElement element)
        {
            if (string.IsNullOrEmpty(element.Path))
            {
                this.Elements.Add(element);
                return;
            }
            var path = element.Path.Split(global::System.IO.Path.DirectorySeparatorChar, global::System.IO.Path.AltDirectorySeparatorChar);
            this.GetSection(path).Elements.Add(element);
        }

        protected virtual ConfigurationSection GetSection(params string[] path)
        {
            var section = this;
            foreach (var segment in path)
            {
                section = this.GetSection(section, segment);
            }
            return section;
        }

        protected virtual ConfigurationSection GetSection(ConfigurationSection section, string path)
        {
            foreach (var child in section.Children)
            {
                if (string.Equals(child.Path, path, StringComparison.OrdinalIgnoreCase))
                {
                    return child;
                }
            }
            {
                var child = new ConfigurationSection(this.ComponentSettings, this.Section, path);
                section.Children.Add(child);
                return child;
            }
        }

        public void Reset()
        {
            foreach (var element in this.Elements)
            {
                element.Reset();
            }
        }

        protected override Freezable CreateInstanceCore()
        {
            return new ConfigurationSection();
        }
    }
}
