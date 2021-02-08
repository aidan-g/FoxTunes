using FoxTunes;
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
    public class Menu : ViewModelBase
    {
        public static readonly DependencyProperty CategoryProperty = DependencyProperty.Register(
            "Category",
            typeof(string),
            typeof(Menu),
            new PropertyMetadata(new PropertyChangedCallback(OnCategoryChanged))
        );

        public static string GetCategory(Menu source)
        {
            return (string)source.GetValue(CategoryProperty);
        }

        public static void SetCategory(Menu source, string value)
        {
            source.SetValue(CategoryProperty, value);
        }

        public static void OnCategoryChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var menu = sender as Menu;
            if (menu == null)
            {
                return;
            }
            menu.OnCategoryChanged();
        }

        public static readonly DependencyProperty ComponentsProperty = DependencyProperty.Register(
            "Components",
            typeof(ObservableCollection<IInvocableComponent>),
            typeof(Menu),
            new PropertyMetadata(new PropertyChangedCallback(OnComponentsChanged))
        );

        public static ObservableCollection<IInvocableComponent> GetComponents(Menu source)
        {
            return (ObservableCollection<IInvocableComponent>)source.GetValue(ComponentsProperty);
        }

        public static void SetComponents(Menu source, ObservableCollection<IInvocableComponent> value)
        {
            source.SetValue(ComponentsProperty, value);
        }

        public static void OnComponentsChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var menu = sender as Menu;
            if (menu == null)
            {
                return;
            }
            menu.OnComponentsChanged();
        }

        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
            "Source",
            typeof(object),
            typeof(Menu),
            new PropertyMetadata(new PropertyChangedCallback(OnSourceChanged))
        );

        public static object GetSource(Menu source)
        {
            return source.GetValue(SourceProperty);
        }

        public static void SetSource(Menu source, object value)
        {
            source.SetValue(SourceProperty, value);
        }

        public static void OnSourceChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var menu = sender as Menu;
            if (menu == null)
            {
                return;
            }
            menu.OnSourceChanged();
        }

        public static readonly DependencyProperty MenuVisibleProperty = DependencyProperty.Register(
            "MenuVisible",
            typeof(bool),
            typeof(Menu),
            new PropertyMetadata(new PropertyChangedCallback(OnMenuVisibleChanged))
        );

        public static bool GetMenuVisible(Menu source)
        {
            return (bool)source.GetValue(MenuVisibleProperty);
        }

        public static void SetMenuVisible(Menu source, bool value)
        {
            source.SetValue(MenuVisibleProperty, value);
        }

        public static void OnMenuVisibleChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var menu = sender as Menu;
            if (menu == null)
            {
                return;
            }
            menu.OnMenuVisibleChanged();
        }

        public Menu()
        {
            this.Items = new ObservableCollection<MenuItem>();
        }

        public string Category
        {
            get
            {
                return this.GetValue(CategoryProperty) as string;
            }
            set
            {
                this.SetValue(CategoryProperty, value);
            }
        }

        protected virtual void OnCategoryChanged()
        {
            if (this.CategoryChanged != null)
            {
                this.CategoryChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Category");
        }

        public event EventHandler CategoryChanged;

        public ObservableCollection<IInvocableComponent> Components
        {
            get
            {
                return this.GetValue(ComponentsProperty) as ObservableCollection<IInvocableComponent>;
            }
            set
            {
                this.SetValue(ComponentsProperty, value);
            }
        }

        protected virtual void OnComponentsChanged()
        {
            if (this.ComponentsChanged != null)
            {
                this.ComponentsChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Components");
        }

        public event EventHandler ComponentsChanged;

        public object Source
        {
            get
            {
                return this.GetValue(SourceProperty);
            }
            set
            {
                this.SetValue(SourceProperty, value);
            }
        }

        protected virtual void OnSourceChanged()
        {
            if (this.SourceChanged != null)
            {
                this.SourceChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Source");
        }

        public event EventHandler SourceChanged;

        public bool MenuVisible
        {
            get
            {
                return (bool)this.GetValue(MenuVisibleProperty);
            }
            set
            {
                this.SetValue(MenuVisibleProperty, value);
            }
        }

        protected virtual void OnMenuVisibleChanged()
        {
            if (this.MenuVisible)
            {
                this.Refresh();
            }
            if (this.MenuVisibleChanged != null)
            {
                this.MenuVisibleChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("MenuVisible");
        }

        public event EventHandler MenuVisibleChanged;

        public ObservableCollection<MenuItem> Items { get; set; }

        public virtual IEnumerable<MenuItem> GetItems()
        {
            if (this.Components == null)
            {
                this.Components = new ObservableCollection<IInvocableComponent>(ComponentRegistry.Instance.GetComponents<IInvocableComponent>());
            }
            var items = new List<MenuItem>();
            foreach (var component in this.Components)
            {
                foreach (var invocation in component.Invocations)
                {
                    if (!string.IsNullOrEmpty(this.Category) && !string.Equals(this.Category, invocation.Category, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    //TODO: Hack to set some kind of invocation context.
                    invocation.Source = this.Source;

                    var item = new MenuItem(component, invocation);
                    item.Core = this.Core;
                    if (string.IsNullOrEmpty(invocation.Path))
                    {
                        items.Add(item);
                        continue;
                    }
                    var path = invocation.Path.Split(global::System.IO.Path.DirectorySeparatorChar, global::System.IO.Path.AltDirectorySeparatorChar);
                    var children = this.GetItem(items, invocation, path).Children;
                    if (item.Separator)
                    {
                        children.Add(null);
                    }
                    children.Add(item);
                }
            }
            return items;
        }

        protected virtual MenuItem GetItem(IList<MenuItem> items, IInvocationComponent invocation, string[] path)
        {
            var item = default(MenuItem);
            foreach (var segment in path)
            {
                item = this.GetItem(items, invocation, segment);
                items = item.Children;
            }
            return item;
        }

        protected virtual MenuItem GetItem(IList<MenuItem> items, IInvocationComponent invocation, string path)
        {
            foreach (var item in items)
            {
                if (string.Equals(item.Invocation.Name, path, StringComparison.OrdinalIgnoreCase))
                {
                    return item;
                }
            }
            {
                var item = new MenuItem(
                    null,
                    new InvocationComponent(
                        invocation.Category,
                        invocation.Id,
                        name: path,
                        attributes: (byte)(invocation.Attributes & ~InvocationComponent.ATTRIBUTE_SELECTED)
                    )
                );
                items.Add(item);
                return item;
            }
        }

        protected virtual void Refresh()
        {
            foreach (var item in this.Items)
            {
                if (item == null)
                {
                    continue;
                }
                item.Dispose();
            }
            this.Items.Clear();
            foreach (var item in this.GetItems().OrderBy(item => item.Invocation.Category).ThenBy(item => item.Invocation.Id))
            {
                if (item.Separator && this.Items.Count > 0)
                {
                    this.Items.Add(null);
                }
                this.Items.Add(item);
            }
        }

        protected override Freezable CreateInstanceCore()
        {
            return new Menu();
        }
    }

    public class MenuItem : ViewModelBase
    {
        private MenuItem()
        {
            this.Children = new ObservableCollection<MenuItem>();
        }

        public MenuItem(IInvocableComponent component, IInvocationComponent invocation)
            : this()
        {
            this.Component = component;
            this.Invocation = invocation;
            this.Invocation.AttributesChanged += this.OnAttributesChanged;
        }

        protected virtual void OnAttributesChanged(object sender, EventArgs e)
        {
            var task = Windows.Invoke(new Action(this.OnSelectedChanged));
        }

        public IInvocableComponent Component { get; private set; }

        public IInvocationComponent Invocation { get; private set; }

        public ICommand Command
        {
            get
            {
                return CommandFactory.Instance.CreateCommand(
                    new Func<Task>(this.OnInvoke)
                );
            }
        }

        public bool Separator
        {
            get
            {
                return (this.Invocation.Attributes & InvocationComponent.ATTRIBUTE_SEPARATOR) == InvocationComponent.ATTRIBUTE_SEPARATOR;
            }
        }

        public bool Selected
        {
            get
            {
                return (this.Invocation.Attributes & InvocationComponent.ATTRIBUTE_SELECTED) == InvocationComponent.ATTRIBUTE_SELECTED;
            }
        }

        protected virtual void OnSelectedChanged()
        {
            if (this.SelectedChanged != null)
            {
                this.SelectedChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Selected");
        }

        public event EventHandler SelectedChanged;

        public ObservableCollection<MenuItem> Children { get; private set; }

        protected virtual Task OnInvoke()
        {
            return this.Component.InvokeAsync(this.Invocation);
        }

        protected override void OnDisposing()
        {
            if (this.Invocation != null)
            {
                this.Invocation.AttributesChanged -= this.OnAttributesChanged;
            }
            if (this.Children != null)
            {
                this.Children.ForEach(child =>
                {
                    if (child == null)
                    {
                        return;
                    }
                    child.Dispose();
                });
            }
            base.OnDisposing();
        }

        protected override Freezable CreateInstanceCore()
        {
            return new MenuItem();
        }
    }
}
