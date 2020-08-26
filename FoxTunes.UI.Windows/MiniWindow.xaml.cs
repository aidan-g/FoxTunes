using FoxTunes.Interfaces;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace FoxTunes
{
    /// <summary>
    /// Interaction logic for MiniWindow.xaml
    /// </summary>
    public partial class MiniWindow : Window
    {
        public static readonly DependencyProperty IsGlassEnabledProperty = DependencyProperty.Register(
            "IsGlassEnabled",
            typeof(bool),
            typeof(MiniWindow),
            new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnIsGlassEnabledChanged))
        );

        public static bool GetIsGlassEnabled(MiniWindow source)
        {
            return (bool)source.GetValue(IsGlassEnabledProperty);
        }

        public static void SetIsGlassEnabled(MiniWindow source, bool value)
        {
            source.SetValue(IsGlassEnabledProperty, value);
        }

        private static void OnIsGlassEnabledChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var miniWindow = sender as MiniWindow;
            if (miniWindow == null)
            {
                return;
            }
            miniWindow.OnIsGlassEnabledChanged();
        }

        public MiniWindow()
        {
            var configuration = ComponentRegistry.Instance.GetComponent<IConfiguration>();
            if (configuration != null)
            {
                var element = configuration.GetElement<BooleanConfigurationElement>(
                    WindowsUserInterfaceConfiguration.SECTION,
                    WindowsUserInterfaceConfiguration.EXTEND_GLASS_ELEMENT
                );
                if (element != null)
                {
                    element.ConnectValue(value => this.IsGlassEnabled = value);
                }
            }
            if (!global::FoxTunes.Properties.Settings.Default.MiniWindowBounds.IsEmpty())
            {
                if (ScreenHelper.WindowBoundsVisible(global::FoxTunes.Properties.Settings.Default.MiniWindowBounds))
                {
                    this.Left = global::FoxTunes.Properties.Settings.Default.MiniWindowBounds.Left;
                    this.Top = global::FoxTunes.Properties.Settings.Default.MiniWindowBounds.Top;
                }
            }
            this.InitializeComponent();
        }

        public bool IsGlassEnabled
        {
            get
            {
                return GetIsGlassEnabled(this);
            }
            set
            {
                SetIsGlassEnabled(this, value);
            }
        }

        protected virtual void OnIsGlassEnabledChanged()
        {
            //Nothing to do.
        }

        protected virtual void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            global::FoxTunes.Properties.Settings.Default.MiniWindowBounds = this.RestoreBounds;
            global::FoxTunes.Properties.Settings.Default.Save();
            base.OnClosing(e);
        }
    }
}
