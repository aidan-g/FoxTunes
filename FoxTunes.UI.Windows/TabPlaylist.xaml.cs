using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace FoxTunes
{
    public partial class TabPlaylist : UserControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty PlaylistProperty = DependencyProperty.Register(
            "Playlist",
            typeof(Playlist),
            typeof(TabPlaylist),
            new PropertyMetadata(new PropertyChangedCallback(OnPlaylistChanged))
        );

        public static Playlist GetPlaylist(TabPlaylist source)
        {
            return (Playlist)source.GetValue(PlaylistProperty);
        }

        public static void SetPlaylist(TabPlaylist source, Playlist value)
        {
            source.SetValue(PlaylistProperty, value);
        }

        public static void OnPlaylistChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var tabPlaylist = sender as TabPlaylist;
            if (tabPlaylist == null)
            {
                return;
            }
            tabPlaylist.OnPlaylistChanged();
        }

        public TabPlaylist()
        {
            this.InitializeComponent();
#if NET40
            //VirtualizingStackPanel.IsVirtualizingWhenGrouping is not supported.
            //The behaviour which requires this feature is disabled.
#else
            VirtualizingStackPanel.SetIsVirtualizingWhenGrouping(this.ListView, true);
#endif
            this.IsVisibleChanged += this.OnIsVisibleChanged;
        }

        public Playlist Playlist
        {
            get
            {
                return this.GetValue(PlaylistProperty) as Playlist;
            }
            set
            {
                this.SetValue(PlaylistProperty, value);
            }
        }

        protected virtual void OnPlaylistChanged()
        {
            if (this.PlaylistChanged != null)
            {
                this.PlaylistChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Playlist");
        }

        public event EventHandler PlaylistChanged;

        protected virtual void DragSourceInitialized(object sender, ListViewExtensions.DragSourceInitializedEventArgs e)
        {
            var items = (e.Data as IEnumerable)
                .OfType<PlaylistItem>()
                .ToArray();
            if (!items.Any())
            {
                return;
            }
            DragDrop.DoDragDrop(
                this.ListView,
                items,
                DragDropEffects.Copy
            );
        }

        protected virtual void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.IsVisible && this.ListView.SelectedItem != null)
            {
                this.ListView.ScrollIntoView(this.ListView.SelectedItem);
            }
        }

        protected virtual void OnHeaderClick(object sender, RoutedEventArgs e)
        {
            var columnHeader = e.OriginalSource as GridViewColumnHeader;
            if (columnHeader == null)
            {
                return;
            }
            var column = columnHeader.Column as PlaylistGridViewColumn;
            if (column == null || column.PlaylistColumn == null)
            {
                return;
            }
            var viewModel = this.FindResource<global::FoxTunes.ViewModel.GridPlaylist>("ViewModel");
            if (viewModel == null)
            {
                return;
            }
            var task = viewModel.Sort(column.PlaylistColumn);
        }

        protected virtual void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.ListView.SelectedItem != null)
            {
                if (this.ListView.SelectedItems != null && this.ListView.SelectedItems.Count > 0)
                {
                    //When multi-selecting don't mess with the scroll position.
                    return;
                }
                this.ListView.ScrollIntoView(this.ListView.SelectedItem);
            }
        }

        protected virtual void OnGroupHeaderMouseDown(object sender, MouseButtonEventArgs e)
        {
            var element = sender as FrameworkElement;
            if (element == null)
            {
                return;
            }
            var group = element.DataContext as CollectionViewGroup;
            if (group == null)
            {
                return;
            }
            this.ListView.SelectedItems.Clear();
            foreach (var item in group.Items)
            {
                this.ListView.SelectedItems.Add(item);
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged == null)
            {
                return;
            }
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
