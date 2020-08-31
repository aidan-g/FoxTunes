using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Interop;
using System.Windows.Media;

namespace FoxTunes
{
    public static partial class Extensions
    {
        public static T FindAncestor<T>(this DependencyObject visual) where T : DependencyObject
        {
            do
            {
                if (visual is T)
                {
                    return visual as T;
                }
                visual = VisualTreeHelper.GetParent(visual);
            } while (visual != null);
            return default(T);
        }

        public static DependencyObject FindAncestor(this DependencyObject visual, Type type)
        {
            do
            {
                if (type.IsAssignableFrom(visual.GetType()))
                {
                    return visual;
                }
                visual = VisualTreeHelper.GetParent(visual);
            } while (visual != null);
            return default(DependencyObject);
        }

        public static T FindResource<T>(this FrameworkElement element, object resourceKey) where T : class
        {
            return element.FindResource(resourceKey) as T;
        }

        public static bool GetDataPresent<T>(this IDataObject dataObject)
        {
            foreach (var format in dataObject.GetFormats())
            {
                try
                {
                    var data = dataObject.GetData(format);
                    if (data is T)
                    {
                        return true;
                    }
                }
                catch
                {
                    //TODO: Warn.
                    continue;
                }
            }
            return false;
        }

        public static T GetData<T>(this IDataObject dataObject)
        {
            foreach (var format in dataObject.GetFormats())
            {
                var data = dataObject.GetData(format);
                if (data is T)
                {
                    return (T)data;
                }
            }
            return default(T);
        }

        public static bool IsEmpty(this Rect rect)
        {
            return
                (rect.Left == 0 || double.IsInfinity(rect.Left)) &&
                (rect.Top == 0 || double.IsInfinity(rect.Top)) &&
                (rect.Width == 0 || double.IsInfinity(rect.Width)) &&
                (rect.Height == 0 || double.IsInfinity(rect.Height));
        }

        public static void BringToFront(this Window window)
        {
            window.Activate();
            if (!window.Topmost)
            {
                window.Topmost = true;
                window.Topmost = false;
            }
            window.Focus();
        }

        public static IntPtr GetHandle(this Window window)
        {
            var source = new WindowInteropHelper(window);
            return source.EnsureHandle();
        }

        public static Size GetElementPixelSize(this FrameworkElement element, double width, double height)
        {
            var presentationSource = PresentationSource.FromVisual(element);
            if (presentationSource != null)
            {
                var matrix = presentationSource.CompositionTarget.TransformToDevice;
                return (Size)matrix.Transform(new Vector(width, height));
            }
            else
            {
                using (var hwndSource = new HwndSource(new HwndSourceParameters()))
                {
                    var matrix = hwndSource.CompositionTarget.TransformToDevice;
                    return (Size)matrix.Transform(new Vector(width, height));
                }
            }
        }

        public static bool IsMouseOver(this FrameworkElement element)
        {
            var x = default(int);
            var y = default(int);
            MouseHelper.GetPosition(out x, out y);
            DpiHelper.TransformPosition(ref x, ref y);
            var window = element.FindAncestor<Window>();
            var result = VisualTreeHelper.HitTest(window, window.PointFromScreen(new Point(x, y)));
            if (result == null || result.VisualHit == null)
            {
                return false;
            }
            if (element == result.VisualHit || element.IsAncestorOf(result.VisualHit))
            {
                return true;
            }
            if (result.VisualHit is Adorner adorner)
            {
                //For some reason adorners return hit tests even when IsHitTestVisible = False.
                //Looks like a bug to me.
                if (element == adorner.AdornedElement)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
