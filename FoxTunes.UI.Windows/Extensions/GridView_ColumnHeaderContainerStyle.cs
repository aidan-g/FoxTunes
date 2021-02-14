using FoxTunes.Interfaces;
using System;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace FoxTunes
{
    public static partial class GridViewExtensions
    {
        private static readonly ConditionalWeakTable<GridView, ColumnHeaderContainerStyleBehaviour> ColumnHeaderContainerStyleBehaviours = new ConditionalWeakTable<GridView, ColumnHeaderContainerStyleBehaviour>();

        public static readonly DependencyProperty ColumnHeaderContainerStyleProperty = DependencyProperty.RegisterAttached(
            "ColumnHeaderContainerStyle",
            typeof(Style),
            typeof(GridViewExtensions),
            new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnColumnHeaderContainerStylePropertyChanged))
        );

        public static Style GetColumnHeaderContainerStyle(GridView source)
        {
            return (Style)source.GetValue(ColumnHeaderContainerStyleProperty);
        }

        public static void SetColumnHeaderContainerStyle(GridView source, Style value)
        {
            source.SetValue(ColumnHeaderContainerStyleProperty, value);
        }

        private static void OnColumnHeaderContainerStylePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var GridView = sender as GridView;
            if (GridView == null)
            {
                return;
            }
            if (GetColumnHeaderContainerStyle(GridView) != null)
            {
                var behaviour = default(ColumnHeaderContainerStyleBehaviour);
                if (!ColumnHeaderContainerStyleBehaviours.TryGetValue(GridView, out behaviour))
                {
                    ColumnHeaderContainerStyleBehaviours.Add(GridView, new ColumnHeaderContainerStyleBehaviour(GridView));
                }
            }
            else
            {
                ColumnHeaderContainerStyleBehaviours.Remove(GridView);
            }
        }

        private class ColumnHeaderContainerStyleBehaviour : DynamicStyleBehaviour
        {
            public ColumnHeaderContainerStyleBehaviour(GridView GridView)
            {
                this.GridView = GridView;
                this.Apply();
            }

            public GridView GridView { get; private set; }

            protected override void Apply()
            {
                try
                {
                    this.GridView.ColumnHeaderContainerStyle = this.CreateStyle(
                        GetColumnHeaderContainerStyle(this.GridView),
                        //TODO: Ignoring scope of GridView.
                        (Style)Windows.ActiveWindow.TryFindResource(typeof(GridViewColumnHeader))
                    );
                }
                catch (Exception e)
                {
                    //Seems to happen when changing themes, some unserializable data in the style.
                    Logger.Write(typeof(ColumnHeaderContainerStyleBehaviour), LogLevel.Warn, "Failed to apply column header style: {0}", e.Message);
                }
            }
        }
    }
}
