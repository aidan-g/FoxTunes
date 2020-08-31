using System.ComponentModel;
using System.Windows;

namespace FoxTunes
{
    /// <summary>
    /// Interaction logic for PlaylistManagerWindow.xaml
    /// </summary>
    public partial class PlaylistManagerWindow : WindowBase
    {
        public PlaylistManagerWindow()
        {
            if (!global::FoxTunes.Properties.Settings.Default.PlaylistManagerWindowBounds.IsEmpty())
            {
                if (ScreenHelper.WindowBoundsVisible(global::FoxTunes.Properties.Settings.Default.PlaylistManagerWindowBounds))
                {
                    this.Left = global::FoxTunes.Properties.Settings.Default.PlaylistManagerWindowBounds.Left;
                    this.Top = global::FoxTunes.Properties.Settings.Default.PlaylistManagerWindowBounds.Top;
                }
                this.Width = global::FoxTunes.Properties.Settings.Default.PlaylistManagerWindowBounds.Width;
                this.Height = global::FoxTunes.Properties.Settings.Default.PlaylistManagerWindowBounds.Height;
            }
            else
            {
                this.Width = 600;
                this.Height = 400;
            }
            if (double.IsNaN(this.Left) || double.IsNaN(this.Top))
            {
                this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }
            this.InitializeComponent();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            global::FoxTunes.Properties.Settings.Default.PlaylistManagerWindowBounds = this.RestoreBounds;
            global::FoxTunes.Properties.Settings.Default.Save();
            base.OnClosing(e);
        }
    }
}
