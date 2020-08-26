using System.Windows;
using System.Windows.Controls;

namespace FoxTunes
{
    /// <summary>
    /// Interaction logic for MiniPlaylist.xaml
    /// </summary>
    [UIComponent("19AB38D6-AFBB-48D8-9E76-EF6B2BFD75DA", "Simple Playlist")]
    public partial class MiniPlaylist : UIComponentBase
    {
        public MiniPlaylist()
        {
            this.InitializeComponent();
            this.IsVisibleChanged += this.OnIsVisibleChanged;
        }

        protected virtual async void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!this.IsVisible)
            {
                return;
            }
            await Windows.Invoke(() =>
            {
                if (this.ListBox.SelectedItem != null)
                {
                    this.ListBox.ScrollIntoView(this.ListBox.SelectedItem);
                }
            }).ConfigureAwait(false);
        }

        protected virtual void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!this.IsVisible)
            {
                return;
            }
            var listBox = sender as ListBox;
            if (listBox == null || listBox.SelectedItem == null)
            {
                return;
            }
            listBox.ScrollIntoView(listBox.SelectedItem);
        }
    }
}
