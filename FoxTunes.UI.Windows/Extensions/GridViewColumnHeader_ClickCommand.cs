using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FoxTunes
{
    public static class GridViewColumnHeaderExtensions
    {
        public static readonly DependencyProperty ClickCommandProperty = DependencyProperty.RegisterAttached(
            "ClickCommand",
            typeof(ICommand),
            typeof(GridViewColumnExtensions),
            new UIPropertyMetadata(null, new PropertyChangedCallback(OnClickCommandPropertyChanged))
        );

        public static ICommand GetClickCommand(GridViewColumnHeader source)
        {
            return (ICommand)source.GetValue(ClickCommandProperty);
        }

        public static void SetClickCommand(GridViewColumnHeader source, ICommand value)
        {
            source.SetValue(ClickCommandProperty, value);
        }

        private static void OnClickCommandPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var gridViewColumnHeader = sender as GridViewColumnHeader;
            if (gridViewColumnHeader == null)
            {
                return;
            }
            if (GetClickCommand(gridViewColumnHeader) == null)
            {
                gridViewColumnHeader.Click += OnClick;
            }
            else
            {
                gridViewColumnHeader.Click -= OnClick;
            }
        }

        private static void OnClick(object sender, RoutedEventArgs e)
        {
            var gridViewColumnHeader = sender as GridViewColumnHeader;
            if (gridViewColumnHeader == null)
            {
                return;
            }
            var command = GetClickCommand(gridViewColumnHeader);
            if (command == null)
            {
                return;
            }
            command.Execute(gridViewColumnHeader.DataContext);
        }
    }
}
