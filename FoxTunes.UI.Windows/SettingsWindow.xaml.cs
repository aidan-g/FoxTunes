using System.ComponentModel;
using System.Windows;

namespace FoxTunes
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : WindowBase
    {
        public SettingsWindow()
        {
            if (!global::FoxTunes.Properties.Settings.Default.SettingsWindowBounds.IsEmpty())
            {
                if (ScreenHelper.WindowBoundsVisible(global::FoxTunes.Properties.Settings.Default.SettingsWindowBounds))
                {
                    this.Left = global::FoxTunes.Properties.Settings.Default.SettingsWindowBounds.Left;
                    this.Top = global::FoxTunes.Properties.Settings.Default.SettingsWindowBounds.Top;
                }
                this.Width = global::FoxTunes.Properties.Settings.Default.SettingsWindowBounds.Width;
                this.Height = global::FoxTunes.Properties.Settings.Default.SettingsWindowBounds.Height;
            }
            else
            {
                this.Width = 800;
                this.Height = 600;
            }
            if (double.IsNaN(this.Left) || double.IsNaN(this.Top))
            {
                this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }
            this.InitializeComponent();
        }

        public override string Id
        {
            get
            {
                return "35A7051F-AF3D-48D8-96B3-A63E1D17437E";
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            global::FoxTunes.Properties.Settings.Default.SettingsWindowBounds = this.RestoreBounds;
            global::FoxTunes.Properties.Settings.Default.Save();
            base.OnClosing(e);
        }
    }
}
