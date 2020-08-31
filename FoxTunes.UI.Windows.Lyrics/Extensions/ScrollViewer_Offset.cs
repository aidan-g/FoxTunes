using System;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace FoxTunes
{
    public static partial class ScrollViewerExtensions
    {
        private static readonly ConditionalWeakTable<ScrollViewer, OffsetBehaviour> OffsetBehaviours = new ConditionalWeakTable<ScrollViewer, OffsetBehaviour>();

        public static readonly DependencyProperty AutoScrollProperty = DependencyProperty.RegisterAttached(
            "AutoScroll",
            typeof(bool),
            typeof(ScrollViewerExtensions),
            new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnAutoScrollPropertyChanged))
        );

        public static bool GetAutoScroll(ScrollViewer source)
        {
            return (bool)source.GetValue(AutoScrollProperty);
        }

        public static void SetAutoScroll(ScrollViewer source, bool value)
        {
            source.SetValue(AutoScrollProperty, value);
        }

        private static void OnAutoScrollPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var scrollViewer = sender as ScrollViewer;
            if (scrollViewer == null)
            {
                return;
            }
            var behaviour = default(OffsetBehaviour);
            OffsetBehaviours.TryGetValue(scrollViewer, out behaviour);
            if (GetAutoScroll(scrollViewer))
            {
                if (behaviour == null)
                {
                    OffsetBehaviours.Add(scrollViewer, new OffsetBehaviour(scrollViewer));
                }
            }
            else
            {
                if (behaviour != null)
                {
                    behaviour.Dispose();
                }
                OffsetBehaviours.Remove(scrollViewer);
            }
        }

        public static readonly DependencyProperty VerticalValueProperty = DependencyProperty.RegisterAttached(
            "VerticalValue",
            typeof(double),
            typeof(ScrollViewerExtensions),
            new FrameworkPropertyMetadata(double.NaN, new PropertyChangedCallback(OnVerticalValuePropertyChanged))
        );

        public static double GetVerticalValue(ScrollViewer source)
        {
            return (double)source.GetValue(VerticalValueProperty);
        }

        public static void SetVerticalValue(ScrollViewer source, double value)
        {
            source.SetValue(VerticalValueProperty, value);
        }

        private static void OnVerticalValuePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            //Nothing to do.
        }

        public static readonly DependencyProperty VerticalMaxProperty = DependencyProperty.RegisterAttached(
            "VerticalMax",
            typeof(double),
            typeof(ScrollViewerExtensions),
            new FrameworkPropertyMetadata(double.NaN, new PropertyChangedCallback(OnVerticalMaxPropertyChanged))
        );

        public static double GetVerticalMax(ScrollViewer source)
        {
            return (double)source.GetValue(VerticalMaxProperty);
        }

        public static void SetVerticalMax(ScrollViewer source, double value)
        {
            source.SetValue(VerticalMaxProperty, value);
        }

        private static void OnVerticalMaxPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            //Nothing to do.
        }

        private class OffsetBehaviour : UIBehaviour
        {
            public OffsetBehaviour(ScrollViewer scrollViewer)
            {
                this.ScrollViewer = scrollViewer;
                BindingHelper.AddHandler(this.ScrollViewer, VerticalValueProperty, typeof(ScrollViewer), this.OnVerticalValueChanged);
            }

            public ScrollViewer ScrollViewer { get; private set; }

            protected virtual void OnVerticalValueChanged(object sender, EventArgs e)
            {
                var height = this.ScrollViewer.ExtentHeight - this.ScrollViewer.ViewportHeight;
                if (height <= 0)
                {
                    //Content is not scrollable.
                    return;
                }
                var verticalOffset = Math.Round(height * (GetVerticalValue(this.ScrollViewer) / GetVerticalMax(ScrollViewer)), 1);
                if (verticalOffset == this.ScrollViewer.VerticalOffset)
                {
                    return;
                }
                this.ScrollViewer.ScrollToVerticalOffset(verticalOffset);
            }

            protected override void OnDisposing()
            {
                BindingHelper.RemoveHandler(this.ScrollViewer, VerticalValueProperty, typeof(ScrollViewer), this.OnVerticalValueChanged);
                base.OnDisposing();
            }
        }
    }
}
