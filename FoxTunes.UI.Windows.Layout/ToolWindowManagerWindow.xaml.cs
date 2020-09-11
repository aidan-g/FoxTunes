using System.ComponentModel;
using System.Windows;

namespace FoxTunes
{
    /// <summary>
    /// Interaction logic for ToolWindowManagerWindow.xaml
    /// </summary>
    public partial class ToolWindowManagerWindow : WindowBase
    {
        public ToolWindowManagerWindow()
        {
            if (!global::FoxTunes.Properties.Settings.Default.ToolWindowBounds.IsEmpty())
            {
                if (ScreenHelper.WindowBoundsVisible(global::FoxTunes.Properties.Settings.Default.ToolWindowBounds))
                {
                    this.Left = global::FoxTunes.Properties.Settings.Default.ToolWindowBounds.Left;
                    this.Top = global::FoxTunes.Properties.Settings.Default.ToolWindowBounds.Top;
                }
                this.Width = global::FoxTunes.Properties.Settings.Default.ToolWindowBounds.Width;
                this.Height = global::FoxTunes.Properties.Settings.Default.ToolWindowBounds.Height;
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
                return "A4342D78-BE0A-4707-9180-B0D71093B074";
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            global::FoxTunes.Properties.Settings.Default.ToolWindowBounds = this.RestoreBounds;
            global::FoxTunes.Properties.Settings.Default.Save();
            base.OnClosing(e);
        }
    }
}
