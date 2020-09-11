using System.Security.Cryptography;

namespace FoxTunes
{
    /// <summary>
    /// Interaction logic for ToolWindow.xaml
    /// </summary>
    public partial class ToolWindow : WindowBase
    {
        public ToolWindow()
        {
            this.InitializeComponent();
        }

        public override string Id
        {
            get
            {
                var configuration = this.Configuration;
                if (configuration == null)
                {
                    return string.Empty;
                }
                return configuration.Title;
            }
        }

        public ToolWindowConfiguration Configuration
        {
            get
            {
                var viewModel = this.TryFindResource("ViewModel") as global::FoxTunes.ViewModel.ToolWindow;
                if (viewModel == null)
                {
                    return null;
                }
                return viewModel.Configuration;
            }
            set
            {
                var viewModel = this.TryFindResource("ViewModel") as global::FoxTunes.ViewModel.ToolWindow;
                if (viewModel == null)
                {
                    return;
                }
                viewModel.Configuration = value;
            }
        }
    }
}
