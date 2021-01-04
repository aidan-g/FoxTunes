using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;

namespace FoxTunes.ViewModel
{
    public class ComponentSettingsPage : ViewModelBase, ISelectable, IExpandable
    {
        private ComponentSettingsPage()
        {
            this.Elements = new ObservableCollection<ConfigurationElement>();
            this.Children = new ObservableCollection<ComponentSettingsPage>();
            this.IsExpanded = true;
        }

        protected ComponentSettingsPage(string name) : this()
        {
            this.Name = name;
        }

        public ComponentSettingsPage(string name, IEnumerable<ConfigurationElement> elements) : this(name)
        {
            elements = elements //Always put "Advanced" settings last.
                .OrderBy(element => string.IsNullOrEmpty(element.Path) || element.Path.Contains("Advanced", true))
                .ThenBy(element => element.Id);
            foreach (var element in elements)
            {
                this.AddElement(element);
            }
        }

        public string Name { get; private set; }


        public bool HasElements
        {
            get
            {
                return this.Elements.Any();
            }
        }

        public ObservableCollection<ConfigurationElement> Elements { get; private set; }

        public ObservableCollection<ComponentSettingsPage> Children { get; private set; }

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

        public bool IsVisible
        {
            get
            {
                return this.Elements.Any(element => !element.IsHidden) || this.Children.Any(child => child.IsVisible);
            }
        }

        protected virtual void OnIsVisibleChanged()
        {
            if (this.IsVisibleChanged != null)
            {
                this.IsVisibleChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("IsVisible");
        }

        public event EventHandler IsVisibleChanged;

        protected virtual void AddElement(ConfigurationElement element)
        {
            if (string.IsNullOrEmpty(element.Path))
            {
                this.Elements.Add(element);
            }
            else
            {
                var page = this.GetOrAddPage(element.Path);
                page.Elements.Add(element);
            }
        }

        protected virtual ComponentSettingsPage GetOrAddPage(string path)
        {
            var page = default(ComponentSettingsPage);
            var pages = this.Children;
            var segments = path.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            foreach (var segment in segments)
            {
                page = pages.FirstOrDefault(
                    _page => string.Equals(_page.Name, segment, StringComparison.OrdinalIgnoreCase)
                );
                if (page == null)
                {
                    page = new ComponentSettingsPage(segment);
                    pages.Add(page);
                }
                pages = page.Children;
            }
            return page;
        }

        public override void InitializeComponent(ICore core)
        {
            foreach (var element in this.Elements)
            {
                element.IsHiddenChanged += this.OnIsHiddenChanged;
            }
            foreach (var child in this.Children)
            {
                child.InitializeComponent(core);
                child.IsVisibleChanged += this.OnIsVisibleChanged;
            }
            base.InitializeComponent(core);
        }

        protected virtual void OnIsHiddenChanged(object sender, EventArgs e)
        {
            this.OnIsVisibleChanged();
        }

        protected virtual void OnIsVisibleChanged(object sender, EventArgs e)
        {
            this.OnIsVisibleChanged();
        }

        public void Reset()
        {
            foreach (var element in this.Elements)
            {
                element.Reset();
            }
        }

        protected override void OnDisposing()
        {
            foreach (var element in this.Elements)
            {
                element.IsHiddenChanged -= this.OnIsHiddenChanged;
            }
            foreach (var child in this.Children)
            {
                child.Dispose();
                child.IsVisibleChanged -= this.OnIsVisibleChanged;
            }
            base.OnDisposing();
        }

        protected override Freezable CreateInstanceCore()
        {
            return new ComponentSettingsPage();
        }
    }
}
