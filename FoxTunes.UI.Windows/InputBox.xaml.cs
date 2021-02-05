using System.Windows;
using System.Windows.Input;

namespace FoxTunes
{
    /// <summary>
    /// Interaction logic for InputBox.xaml
    /// </summary>
    public partial class InputBox : WindowBase
    {
        public InputBox()
        {
            this.InitializeComponent();
        }

        public override string Id
        {
            get
            {
                return "1EC1A063-DC6A-4264-8882-0A01B8F6B80E";
            }
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.DialogResult = true;
            }
            else if (e.Key == Key.Escape)
            {
                this.DialogResult = false;
            }
            base.OnPreviewKeyDown(e);
        }

        protected virtual void OnOKClick(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        protected virtual void OnCancelClick(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        public static string ShowDialog(string prompt)
        {
            var instance = new InputBox()
            {
                Owner = Windows.ActiveWindow
            };
            instance.Prompt.Text = prompt;

            if (instance.ShowDialog().GetValueOrDefault())
            {
                return instance.Result.Text;
            }

            return null;
        }

    }
}
