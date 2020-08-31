using System;
using System.Windows.Media;
using DrawingPoint = global::System.Drawing.Point;
using WindowsPoint = global::System.Windows.Point;

namespace FoxTunes
{
    public static class PointConverter
    {
        public static DrawingPoint ToDrawingPoint(WindowsPoint point)
        {
            return new DrawingPoint(
                Convert.ToInt32(point.X),
                Convert.ToInt32(point.Y)
            );
        }

        public static WindowsPoint ToWindowsPoint(DrawingPoint point)
        {
            return new WindowsPoint(
                point.X,
                point.Y
            );
        }

        public static WindowsPoint PointToScreen(Visual visual, WindowsPoint point)
        {
            return visual.PointToScreen(point);
        }
    }
}
