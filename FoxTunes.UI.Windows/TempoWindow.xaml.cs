using System.ComponentModel;
using System.Windows;

namespace FoxTunes
{
    /// <summary>
    /// Interaction logic for TempoWindow.xaml
    /// </summary>
    public partial class TempoWindow : WindowBase
    {
        public TempoWindow()
        {
            if (!global::FoxTunes.Properties.Settings.Default.TempoWindowBounds.IsEmpty())
            {
                if (ScreenHelper.WindowBoundsVisible(global::FoxTunes.Properties.Settings.Default.TempoWindowBounds))
                {
                    this.Left = global::FoxTunes.Properties.Settings.Default.TempoWindowBounds.Left;
                    this.Top = global::FoxTunes.Properties.Settings.Default.TempoWindowBounds.Top;
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
                return "82AD2BC9-AFE3-4A79-A7D4-D2CE2A88F256";
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            global::FoxTunes.Properties.Settings.Default.TempoWindowBounds = this.RestoreBounds;
            global::FoxTunes.Properties.Settings.Default.Save();
            base.OnClosing(e);
        }
    }
}
