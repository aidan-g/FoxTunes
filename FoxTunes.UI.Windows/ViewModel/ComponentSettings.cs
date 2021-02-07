using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
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
            this.Pages = new ObservableCollection<ComponentSettingsPage>();
        }

        public IConfiguration Configuration { get; private set; }

        public ObservableCollection<ComponentSettingsPage> Pages { get; private set; }

        private ComponentSettingsPage _SelectedPage { get; set; }

        public ComponentSettingsPage SelectedPage
        {
            get
            {
                return this._SelectedPage;
            }
            set
            {
                if (object.ReferenceEquals(this._SelectedPage, value))
                {
                    return;
                }
                this.OnSelectedPageChanging();
                this._SelectedPage = value;
                this.OnSelectedPageChanged();
            }
        }

        protected virtual void OnSelectedPageChanging()
        {
            if (this.SelectedPage != null)
            {
                this.SelectedPage.IsSelected = false;
            }
            if (this.SelectedPageChanging != null)
            {
                this.SelectedPageChanging(this, EventArgs.Empty);
            }
            this.OnPropertyChanging("SelectedPage");
        }

        public event EventHandler SelectedPageChanging;

        protected virtual void OnSelectedPageChanged()
        {
            if (this.SelectedPage != null)
            {
                this.SelectedPage.IsSelected = true;
            }
            if (this.SelectedPageChanged != null)
            {
                this.SelectedPageChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("SelectedPage");
        }

        public event EventHandler SelectedPageChanged;

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
                    () => this.SelectedPage.Reset(),
                    () => this.SelectedPage != null
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
                this.Pages.Clear();
                foreach (var section in this.Configuration.Sections.OrderBy(section => section.Id))
                {
                    if (section.Flags.HasFlag(ConfigurationSectionFlags.System))
                    {
                        //System config should not be presented to the user.
                        continue;
                    }
                    var sectionMatches = this.MatchesFilter(section);
                    var elements = default(IEnumerable<ConfigurationElement>);
                    if (sectionMatches)
                    {
                        elements = section.Elements;
                    }
                    else if (!this.MatchesFilter(section.Elements, out elements))
                    {
                        continue;
                    }
                    var page = new ComponentSettingsPage(section.Name, elements);
                    page.InitializeComponent(this.Core);
                    this.Pages.Add(page);
                }
                this.SelectedPage = this.Pages.FirstOrDefault();
            });
        }

        public virtual bool MatchesFilter(global::FoxTunes.ConfigurationSection section)
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
            return false;
        }

        public virtual bool MatchesFilter(IEnumerable<ConfigurationElement> inputElements, out IEnumerable<ConfigurationElement> outputElements)
        {
            return (outputElements = inputElements.Where(element => this.MatchesFilter(element))).Any();
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
}
