using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace FoxTunes
{
    /// <summary>
    /// Interaction logic for UIComponentVerticalSplitContainer.xaml
    /// </summary>
    [UIComponent("18E98420-F039-4504-A116-3D0F26BEAAD5", "Vertical Split")]
    public partial class UIComponentVerticalSplitContainer : UIComponentPanel
    {
        const string FREEZE_LEFT = "AAAA";

        const string FREEZE_RIGHT = "BBBB";

        const string COLLAPSE_LEFT = "CCCC";

        const string COLLAPSE_RIGHT = "DDDD";

        public static readonly DependencyProperty LeftComponentProperty = DependencyProperty.Register(
            "LeftComponent",
            typeof(UIComponentConfiguration),
            typeof(UIComponentVerticalSplitContainer),
            new PropertyMetadata(new PropertyChangedCallback(OnLeftComponentChanged))
        );

        public static UIComponentConfiguration GetLeftComponent(UIComponentVerticalSplitContainer source)
        {
            return (UIComponentConfiguration)source.GetValue(LeftComponentProperty);
        }

        public static void SetLeftComponent(UIComponentVerticalSplitContainer source, UIComponentConfiguration value)
        {
            source.SetValue(LeftComponentProperty, value);
        }

        public static void OnLeftComponentChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var container = sender as UIComponentVerticalSplitContainer;
            if (container == null)
            {
                return;
            }
            container.OnLeftComponentChanged();
        }

        public static readonly DependencyProperty RightComponentProperty = DependencyProperty.Register(
            "RightComponent",
            typeof(UIComponentConfiguration),
            typeof(UIComponentVerticalSplitContainer),
            new PropertyMetadata(new PropertyChangedCallback(OnRightComponentChanged))
        );

        public static UIComponentConfiguration GetRightComponent(UIComponentVerticalSplitContainer source)
        {
            return (UIComponentConfiguration)source.GetValue(RightComponentProperty);
        }

        public static void SetRightComponent(UIComponentVerticalSplitContainer source, UIComponentConfiguration value)
        {
            source.SetValue(RightComponentProperty, value);
        }

        public static void OnRightComponentChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var container = sender as UIComponentVerticalSplitContainer;
            if (container == null)
            {
                return;
            }
            container.OnRightComponentChanged();
        }

        public static readonly DependencyProperty LeftEnabledProperty = DependencyProperty.Register(
            "LeftEnabled",
            typeof(bool),
            typeof(UIComponentVerticalSplitContainer),
            new PropertyMetadata(true, new PropertyChangedCallback(OnLeftEnabledChanged))
        );

        public static bool GetLeftEnabled(UIComponentVerticalSplitContainer source)
        {
            return (bool)source.GetValue(LeftEnabledProperty);
        }

        public static void SetLeftEnabled(UIComponentVerticalSplitContainer source, bool value)
        {
            source.SetValue(LeftEnabledProperty, value);
        }

        public static void OnLeftEnabledChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var container = sender as UIComponentVerticalSplitContainer;
            if (container == null)
            {
                return;
            }
            container.OnLeftEnabledChanged();
        }

        public static readonly DependencyProperty RightEnabledProperty = DependencyProperty.Register(
            "RightEnabled",
            typeof(bool),
            typeof(UIComponentVerticalSplitContainer),
            new PropertyMetadata(true, new PropertyChangedCallback(OnRightEnabledChanged))
        );

        public static bool GetRightEnabled(UIComponentVerticalSplitContainer source)
        {
            return (bool)source.GetValue(RightEnabledProperty);
        }

        public static void SetRightEnabled(UIComponentVerticalSplitContainer source, bool value)
        {
            source.SetValue(RightEnabledProperty, value);
        }

        public static void OnRightEnabledChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var container = sender as UIComponentVerticalSplitContainer;
            if (container == null)
            {
                return;
            }
            container.OnRightEnabledChanged();
        }

        public static readonly DependencyProperty SplitterDistanceProperty = DependencyProperty.Register(
            "SplitterDistance",
            typeof(string),
            typeof(UIComponentVerticalSplitContainer),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnSplitterDistanceChanged))
        );

        public static string GetSplitterDistance(UIComponentVerticalSplitContainer source)
        {
            return (string)source.GetValue(SplitterDistanceProperty);
        }

        public static void SetSplitterDistance(UIComponentVerticalSplitContainer source, string value)
        {
            source.SetValue(SplitterDistanceProperty, value);
        }

        public static void OnSplitterDistanceChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var container = sender as UIComponentVerticalSplitContainer;
            if (container == null)
            {
                return;
            }
            container.OnSplitterDistanceChanged();
        }

        public static readonly DependencyProperty SplitterDirectionProperty = DependencyProperty.Register(
            "SplitterDirection",
            typeof(string),
            typeof(UIComponentVerticalSplitContainer),
            new PropertyMetadata(new PropertyChangedCallback(OnSplitterDirectionChanged))
        );

        public static string GetSplitterDirection(UIComponentVerticalSplitContainer source)
        {
            return (string)source.GetValue(SplitterDirectionProperty);
        }

        public static void SetSplitterDirection(UIComponentVerticalSplitContainer source, string value)
        {
            source.SetValue(SplitterDirectionProperty, value);
        }

        public static void OnSplitterDirectionChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var container = sender as UIComponentVerticalSplitContainer;
            if (container == null)
            {
                return;
            }
            container.OnSplitterDirectionChanged();
        }

        public static readonly DependencyProperty CollapseLeftProperty = DependencyProperty.Register(
            "CollapseLeft",
            typeof(bool),
            typeof(UIComponentVerticalSplitContainer),
            new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnCollapseLeftChanged))
        );

        public static bool GetCollapseLeft(UIComponentVerticalSplitContainer source)
        {
            return (bool)source.GetValue(CollapseLeftProperty);
        }

        public static void SetCollapseLeft(UIComponentVerticalSplitContainer source, bool value)
        {
            source.SetValue(CollapseLeftProperty, value);
        }

        public static void OnCollapseLeftChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var container = sender as UIComponentVerticalSplitContainer;
            if (container == null)
            {
                return;
            }
            container.OnCollapseLeftChanged();
        }

        public static readonly DependencyProperty CollapseRightProperty = DependencyProperty.Register(
            "CollapseRight",
            typeof(bool),
            typeof(UIComponentVerticalSplitContainer),
            new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnCollapseRightChanged))
        );

        public static bool GetCollapseRight(UIComponentVerticalSplitContainer source)
        {
            return (bool)source.GetValue(CollapseRightProperty);
        }

        public static void SetCollapseRight(UIComponentVerticalSplitContainer source, bool value)
        {
            source.SetValue(CollapseRightProperty, value);
        }

        public static void OnCollapseRightChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var container = sender as UIComponentVerticalSplitContainer;
            if (container == null)
            {
                return;
            }
            container.OnCollapseRightChanged();
        }

        public UIComponentVerticalSplitContainer()
        {
            this.InitializeComponent();
            this.CreateBindings();
        }

        new protected virtual void CreateBindings()
        {
            this.SetBinding(
                UIComponentVerticalSplitContainer.LeftEnabledProperty,
                new Binding()
                {
                    Source = this.LeftContainer,
                    Path = new PropertyPath("Content.IsComponentEnabled"),
                    FallbackValue = true
                }
            );
            this.SetBinding(
                UIComponentVerticalSplitContainer.RightEnabledProperty,
                new Binding()
                {
                    Source = this.RightContainer,
                    Path = new PropertyPath("Content.IsComponentEnabled"),
                    FallbackValue = true
                }
            );
        }

        protected virtual void UpdateBindings()
        {
            BindingOperations.ClearBinding(
                this.LeftColumn,
                ColumnDefinition.WidthProperty
            );
            BindingOperations.ClearBinding(
                this.RightColumn,
                ColumnDefinition.WidthProperty
            );
            if (this.CollapseLeft && !this.LeftEnabled && this.CollapseRight && !this.RightEnabled && !this.IsInDesignMode)
            {
                this.IsComponentEnabled = false;
            }
            else
            {
                if (this.CollapseLeft && !this.LeftEnabled && !this.IsInDesignMode)
                {
                    this.LeftContainer.Visibility = Visibility.Collapsed;
                    this.LeftColumn.Width = new GridLength(0, GridUnitType.Pixel);
                    this.SplitterColumn.Width = new GridLength(0, GridUnitType.Pixel);
                }
                else if (this.CollapseRight && !this.RightEnabled && !this.IsInDesignMode)
                {
                    this.RightContainer.Visibility = Visibility.Collapsed;
                    this.SplitterColumn.Width = new GridLength(0, GridUnitType.Pixel);
                    this.RightColumn.Width = new GridLength(0, GridUnitType.Pixel);
                }
                else
                {
                    this.SplitterColumn.Width = new GridLength(4, GridUnitType.Pixel);
                    if (string.Equals(this.SplitterDirection, Enum.GetName(typeof(Dock), Dock.Left), StringComparison.OrdinalIgnoreCase))
                    {
                        this.LeftColumn.SetBinding(
                            ColumnDefinition.WidthProperty,
                            new Binding()
                            {
                                Source = this,
                                Path = new PropertyPath(nameof(this.SplitterDistance)),
                                Converter = new global::FoxTunes.ViewModel.GridLengthConverter(),
                                Mode = BindingMode.TwoWay
                            }
                        );
                        this.RightColumn.Width = new GridLength(1, GridUnitType.Star);
                    }
                    else
                    {
                        this.LeftColumn.Width = new GridLength(1, GridUnitType.Star);
                        this.RightColumn.SetBinding(
                            ColumnDefinition.WidthProperty,
                            new Binding()
                            {
                                Source = this,
                                Path = new PropertyPath(nameof(this.SplitterDistance)),
                                Converter = new global::FoxTunes.ViewModel.GridLengthConverter(),
                                Mode = BindingMode.TwoWay
                            }
                        );
                    }
                    this.LeftContainer.Visibility = Visibility.Visible;
                    this.RightContainer.Visibility = Visibility.Visible;
                }
                this.IsComponentEnabled = true;
            }
        }

        protected override void OnIsInDesignModeChanged()
        {
            this.UpdateBindings();
            base.OnIsInDesignModeChanged();
        }

        protected override void OnComponentChanged()
        {
            if (this.Component != null)
            {
                this.UpdateMetaData();
                this.UpdateChildren();
            }
            base.OnComponentChanged();
        }

        protected virtual void UpdateChildren()
        {
            if (this.Component.Children != null && this.Component.Children.Count == 2)
            {
                this.LeftComponent = this.Component.Children[0];
                this.RightComponent = this.Component.Children[1];
            }
            else
            {
                this.Component.Children = new ObservableCollection<UIComponentConfiguration>()
                {
                    new UIComponentConfiguration(),
                    new UIComponentConfiguration()
                };
            }
        }

        protected virtual void UpdateMetaData()
        {
            var splitterDistance = default(string);
            var splitterDirection = default(string);
            var collapseLeft = default(string);
            var collapseRight = default(string);
            if (this.Component.MetaData.TryGetValue(nameof(this.SplitterDistance), out splitterDistance))
            {
                this.SplitterDistance = splitterDistance;
            }
            else
            {
                this.SplitterDistance = "1*";
            }
            if (this.Component.MetaData.TryGetValue(nameof(this.SplitterDirection), out splitterDirection) && IsSplitterDirectionValid(splitterDirection))
            {
                this.SplitterDirection = splitterDirection;
            }
            else
            {
                this.SplitterDirection = Enum.GetName(typeof(Dock), Dock.Left);
            }
            if (this.Component.MetaData.TryGetValue(nameof(this.CollapseLeft), out collapseLeft))
            {
                this.CollapseLeft = Convert.ToBoolean(collapseLeft);
            }
            else
            {
                this.CollapseLeft = false;
            }
            if (this.Component.MetaData.TryGetValue(nameof(this.CollapseRight), out collapseRight))
            {
                this.CollapseRight = Convert.ToBoolean(collapseRight);
            }
            else
            {
                this.CollapseRight = false;
            }
        }

        public UIComponentConfiguration LeftComponent
        {
            get
            {
                return this.GetValue(LeftComponentProperty) as UIComponentConfiguration;
            }
            set
            {
                this.SetValue(LeftComponentProperty, value);
            }
        }

        protected virtual void OnLeftComponentChanged()
        {
            if (this.Component != null && this.Component.Children.Count == 2 && this.LeftComponent != null)
            {
                this.Component.Children[0] = this.LeftComponent;
            }
            if (this.LeftComponentChanged != null)
            {
                this.LeftComponentChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("LeftComponent");
        }

        public event EventHandler LeftComponentChanged;

        public UIComponentConfiguration RightComponent
        {
            get
            {
                return this.GetValue(RightComponentProperty) as UIComponentConfiguration;
            }
            set
            {
                this.SetValue(RightComponentProperty, value);
            }
        }

        protected virtual void OnRightComponentChanged()
        {
            if (this.Component != null && this.Component.Children.Count == 2 && this.RightComponent != null)
            {
                this.Component.Children[1] = this.RightComponent;
            }
            if (this.RightComponentChanged != null)
            {
                this.RightComponentChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("RightComponent");
        }

        public event EventHandler RightComponentChanged;

        public bool LeftEnabled
        {
            get
            {
                return (bool)this.GetValue(LeftEnabledProperty);
            }
            set
            {
                this.SetValue(LeftEnabledProperty, value);
            }
        }

        protected virtual void OnLeftEnabledChanged()
        {
            this.UpdateBindings();
            if (this.LeftEnabledChanged != null)
            {
                this.LeftEnabledChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("LeftEnabled");
        }

        public event EventHandler LeftEnabledChanged;

        public bool RightEnabled
        {
            get
            {
                return (bool)this.GetValue(RightEnabledProperty);
            }
            set
            {
                this.SetValue(RightEnabledProperty, value);
            }
        }

        protected virtual void OnRightEnabledChanged()
        {
            this.UpdateBindings();
            if (this.RightEnabledChanged != null)
            {
                this.RightEnabledChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("RightEnabled");
        }

        public event EventHandler RightEnabledChanged;

        public string SplitterDistance
        {
            get
            {
                return (string)this.GetValue(SplitterDistanceProperty);
            }
            set
            {
                this.SetValue(SplitterDistanceProperty, value);
            }
        }

        protected virtual void OnSplitterDistanceChanged()
        {
            if (this.Component != null)
            {
                this.Component.MetaData.AddOrUpdate(
                    nameof(this.SplitterDistance),
                    this.SplitterDistance
                );
            }
            if (this.SplitterDistanceChanged != null)
            {
                this.SplitterDistanceChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("SplitterDistance");
        }

        public event EventHandler SplitterDistanceChanged;

        public string SplitterDirection
        {
            get
            {
                return (string)this.GetValue(SplitterDirectionProperty);
            }
            set
            {
                this.SetValue(SplitterDirectionProperty, value);
            }
        }

        protected virtual void OnSplitterDirectionChanged()
        {
            if (this.Component != null)
            {
                this.Component.MetaData.AddOrUpdate(
                    nameof(this.SplitterDirection),
                    this.SplitterDirection
                );
            }
            this.UpdateBindings();
            if (this.SplitterDirectionChanged != null)
            {
                this.SplitterDirectionChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("SplitterDirection");
        }

        public event EventHandler SplitterDirectionChanged;

        public bool CollapseLeft
        {
            get
            {
                return (bool)this.GetValue(CollapseLeftProperty);
            }
            set
            {
                this.SetValue(CollapseLeftProperty, value);
            }
        }

        protected virtual void OnCollapseLeftChanged()
        {
            if (this.Component != null)
            {
                this.Component.MetaData.AddOrUpdate(
                    nameof(this.CollapseLeft),
                    Convert.ToString(this.CollapseLeft)
                );
            }
            this.UpdateBindings();
            if (this.CollapseLeftChanged != null)
            {
                this.CollapseLeftChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("CollapseLeft");
        }

        public event EventHandler CollapseLeftChanged;

        public bool CollapseRight
        {
            get
            {
                return (bool)this.GetValue(CollapseRightProperty);
            }
            set
            {
                this.SetValue(CollapseRightProperty, value);
            }
        }

        protected virtual void OnCollapseRightChanged()
        {
            if (this.Component != null)
            {
                this.Component.MetaData.AddOrUpdate(
                    nameof(this.CollapseRight),
                    Convert.ToString(this.CollapseRight)
                );
            }
            this.UpdateBindings();
            if (this.CollapseRightChanged != null)
            {
                this.CollapseRightChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("CollapseRight");
        }

        public event EventHandler CollapseRightChanged;

        public override IEnumerable<IInvocationComponent> Invocations
        {
            get
            {
                yield return new InvocationComponent(
                    InvocationComponent.CATEGORY_GLOBAL,
                    FREEZE_LEFT,
                    "Freeze Left",
                    attributes: string.Equals(this.SplitterDirection, Enum.GetName(typeof(Dock), Dock.Left), StringComparison.OrdinalIgnoreCase) ? InvocationComponent.ATTRIBUTE_SELECTED : InvocationComponent.ATTRIBUTE_NONE
                );
                yield return new InvocationComponent(
                    InvocationComponent.CATEGORY_GLOBAL,
                    FREEZE_RIGHT,
                    "Freeze Right",
                    attributes: string.Equals(this.SplitterDirection, Enum.GetName(typeof(Dock), Dock.Right), StringComparison.OrdinalIgnoreCase) ? InvocationComponent.ATTRIBUTE_SELECTED : InvocationComponent.ATTRIBUTE_NONE
                );
                yield return new InvocationComponent(
                    InvocationComponent.CATEGORY_GLOBAL,
                    COLLAPSE_LEFT,
                    "Collapsable Left",
                    attributes: (byte)((this.CollapseLeft ? InvocationComponent.ATTRIBUTE_SELECTED : InvocationComponent.ATTRIBUTE_NONE) | InvocationComponent.ATTRIBUTE_SEPARATOR)
                );
                yield return new InvocationComponent(
                    InvocationComponent.CATEGORY_GLOBAL,
                    COLLAPSE_RIGHT,
                    "Collapsable Right",
                    attributes: this.CollapseRight ? InvocationComponent.ATTRIBUTE_SELECTED : InvocationComponent.ATTRIBUTE_NONE
                );
            }
        }

        public override Task InvokeAsync(IInvocationComponent component)
        {
            switch (component.Id)
            {
                case FREEZE_LEFT:
                    return Windows.Invoke(() => this.SplitterDirection = Enum.GetName(typeof(Dock), Dock.Left));
                case FREEZE_RIGHT:
                    return Windows.Invoke(() => this.SplitterDirection = Enum.GetName(typeof(Dock), Dock.Right));
                case COLLAPSE_LEFT:
                    return Windows.Invoke(() => this.CollapseLeft = !this.CollapseLeft);
                case COLLAPSE_RIGHT:
                    return Windows.Invoke(() => this.CollapseRight = !this.CollapseRight);
            }
            return base.InvokeAsync(component);
        }

        public static bool IsSplitterDirectionValid(string splitterDirection)
        {
            if (!string.IsNullOrEmpty(splitterDirection))
            {
                if (string.Equals(Enum.GetName(typeof(Dock), Dock.Left), splitterDirection, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
                if (string.Equals(Enum.GetName(typeof(Dock), Dock.Right), splitterDirection, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
