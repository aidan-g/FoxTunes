using System.ComponentModel;
using System.Windows;

namespace FoxTunes
{
    /// <summary>
    /// Interaction logic for EqualizerWindow.xaml
    /// </summary>
    public partial class EqualizerWindow : WindowBase
    {
        public EqualizerWindow()
        {
            if (!global::FoxTunes.Properties.Settings.Default.EqualizerWindowBounds.IsEmpty())
            {
                if (ScreenHelper.WindowBoundsVisible(global::FoxTunes.Properties.Settings.Default.EqualizerWindowBounds))
                {
                    this.Left = global::FoxTunes.Properties.Settings.Default.EqualizerWindowBounds.Left;
                    this.Top = global::FoxTunes.Properties.Settings.Default.EqualizerWindowBounds.Top;
                }
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
                return "57C395DF-35A2-4EEB-B20A-5D6B11375BE1";
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            global::FoxTunes.Properties.Settings.Default.EqualizerWindowBounds = this.RestoreBounds;
            global::FoxTunes.Properties.Settings.Default.Save();
            base.OnClosing(e);
        }
    }
}
