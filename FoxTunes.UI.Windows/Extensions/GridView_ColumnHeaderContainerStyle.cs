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

        /// <summary>
        /// TODO: This behaviour sucks. GridView is not a Visual so it doesn't support resource resolution.
        /// TODO: We use Windows.ActiveWindow instead which means scope is ignored.
        /// TODO: If Windows.ActiveWindow is null we need to wait for it (using Windows.ActiveWindowChanged).....
        /// </summary>
        private class ColumnHeaderContainerStyleBehaviour : DynamicStyleBehaviour
        {
            private ColumnHeaderContainerStyleBehaviour()
            {
                Windows.ActiveWindowChanged += this.OnActiveWindowChanged;
            }

            public ColumnHeaderContainerStyleBehaviour(GridView GridView) : this()
            {
                this.GridView = GridView;
                this.Apply();
            }

            public GridView GridView { get; private set; }

            protected virtual void OnActiveWindowChanged(object sender, EventArgs e)
            {
                this.Apply();
            }

            protected override void Apply()
            {
                if (Windows.ActiveWindow == null)
                {
                    return;
                }
                Windows.ActiveWindowChanged -= this.OnActiveWindowChanged;
                this.GridView.ColumnHeaderContainerStyle = this.CreateStyle(
                    GetColumnHeaderContainerStyle(this.GridView),
                    (Style)Windows.ActiveWindow.TryFindResource(typeof(GridViewColumnHeader))
                );
            }

            protected override void OnDisposing()
            {
                Windows.ActiveWindowChanged -= this.OnActiveWindowChanged;
                base.OnDisposing();
            }
        }
    }
}
