using FoxTunes.Interfaces;
using System;
using System.Drawing;

namespace FoxTunes
{
    public static class SnappingHelper
    {
        private static ILogger Logger
        {
            get
            {
                return LogManager.Logger;
            }
        }

        static SnappingHelper()
        {
            var configuration = ComponentRegistry.Instance.GetComponent<IConfiguration>();
            if (configuration != null)
            {
                Proximity = configuration.GetElement<IntegerConfigurationElement>(
                    WindowSnappingBehaviourConfiguration.SECTION,
                    WindowSnappingBehaviourConfiguration.PROXIMITY
                );
            }
        }

        private static readonly IntegerConfigurationElement Proximity;

        public static SnapDirection Snap(ref Rectangle from, Rectangle to, bool resize)
        {
            var result = SnapDirection.None;
            var proximity = Proximity.Value;
            if (from.Bottom >= to.Top - proximity && from.Top <= to.Bottom + proximity)
            {
                if (Math.Abs(from.Right - to.Left) <= proximity)
                {
                    if (resize)
                    {
                        from.Width += -(from.Right - to.Left);
                    }
                    else
                    {
                        from.X += -(from.Right - to.Left);
                    }
                    result |= SnapDirection.OutsideLeft;
                    Logger.Write(typeof(SnappingHelper), LogLevel.Debug, "Snap outside left.");
                }
                else if (Math.Abs(from.Left - to.Left) <= proximity)
                {
                    if (resize)
                    {
                        from.Width += from.Left - to.Left;
                    }
                    from.X += -(from.Left - to.Left);
                    result |= SnapDirection.InsideLeft;
                    Logger.Write(typeof(SnappingHelper), LogLevel.Debug, "Snap inside left.");
                }
                if (Math.Abs(from.Left - to.Right) <= proximity)
                {
                    if (resize)
                    {
                        from.Width += from.Left - to.Right;
                    }
                    from.X += -(from.Left - to.Right);
                    result |= SnapDirection.OutsideRight;
                    Logger.Write(typeof(SnappingHelper), LogLevel.Debug, "Snap outside right.");
                }
                else if (Math.Abs(from.Right - to.Right) <= proximity)
                {
                    if (resize)
                    {
                        from.Width += -(from.Right - to.Right);
                    }
                    else
                    {
                        from.X += -(from.Right - to.Right);
                    }
                    result |= SnapDirection.InsideRight;
                    Logger.Write(typeof(SnappingHelper), LogLevel.Debug, "Snap inside right.");
                }
            }
            if (from.Right >= to.Left - proximity && from.Left <= to.Right + proximity)
            {
                if (Math.Abs(from.Bottom - to.Top) <= proximity)
                {
                    if (resize)
                    {
                        from.Height += -(from.Bottom - to.Top);
                    }
                    else
                    {
                        from.Y += -(from.Bottom - to.Top);
                    }
                    result |= SnapDirection.OutsideTop;
                    Logger.Write(typeof(SnappingHelper), LogLevel.Debug, "Snap outside top.");
                }
                else if (Math.Abs(from.Top - to.Top) <= proximity)
                {
                    if (resize)
                    {
                        from.Height += from.Top - to.Top;
                    }
                    from.Y += -(from.Top - to.Top);
                    result |= SnapDirection.InsideTop;
                    Logger.Write(typeof(SnappingHelper), LogLevel.Debug, "Snap inside top.");
                }
                if (Math.Abs(from.Top - to.Bottom) <= proximity)
                {
                    if (resize)
                    {
                        from.Height += from.Top - to.Bottom;
                    }
                    from.Y += -(from.Top - to.Bottom);
                    result |= SnapDirection.OutsideBottom;
                    Logger.Write(typeof(SnappingHelper), LogLevel.Debug, "Snap outside bottom.");
                }
                else if (Math.Abs(from.Bottom - to.Bottom) <= proximity)
                {
                    if (resize)
                    {
                        from.Height += -(from.Bottom - to.Bottom);
                    }
                    else
                    {
                        from.Y += -(from.Bottom - to.Bottom);
                    }
                    result |= SnapDirection.InsideBottom;
                    Logger.Write(typeof(SnappingHelper), LogLevel.Debug, "Snap inside bottom.");
                }
            }
            return result;
        }

        public static SnapDirection IsSnapped(Rectangle from, Rectangle to)
        {
            return IsSnapped(from, to, Proximity.Value);
        }

        public static SnapDirection IsSnapped(Rectangle from, Rectangle to, int proximity)
        {
            var result = SnapDirection.None;
            if (from.Bottom >= to.Top - proximity && from.Top <= to.Bottom + proximity)
            {
                if (Math.Abs(from.Right - to.Left) <= proximity)
                {
                    result |= SnapDirection.OutsideLeft;
                    Logger.Write(typeof(SnappingHelper), LogLevel.Debug, "Snapped outside left.");
                }
                else if (Math.Abs(from.Left - to.Left) <= proximity)
                {
                    result |= SnapDirection.InsideLeft;
                    Logger.Write(typeof(SnappingHelper), LogLevel.Debug, "Snapped inside left.");
                }
                if (Math.Abs(from.Left - to.Right) <= proximity)
                {
                    result |= SnapDirection.OutsideRight;
                    Logger.Write(typeof(SnappingHelper), LogLevel.Debug, "Snapped outside right.");
                }
                else if (Math.Abs(from.Right - to.Right) <= proximity)
                {
                    result |= SnapDirection.InsideRight;
                    Logger.Write(typeof(SnappingHelper), LogLevel.Debug, "Snapped inside right.");
                }
            }
            if (from.Right >= to.Left - proximity && from.Left <= to.Right + proximity)
            {
                if (Math.Abs(from.Bottom - to.Top) <= proximity)
                {
                    result |= SnapDirection.OutsideTop;
                    Logger.Write(typeof(SnappingHelper), LogLevel.Debug, "Snapped outside top.");
                }
                else if (Math.Abs(from.Top - to.Top) <= proximity)
                {
                    result |= SnapDirection.InsideTop;
                    Logger.Write(typeof(SnappingHelper), LogLevel.Debug, "Snapped inside top.");
                }
                if (Math.Abs(from.Top - to.Bottom) <= proximity)
                {
                    result |= SnapDirection.OutsideBottom;
                    Logger.Write(typeof(SnappingHelper), LogLevel.Debug, "Snapped outside bottom.");
                }
                else if (Math.Abs(from.Bottom - to.Bottom) <= proximity)
                {
                    result |= SnapDirection.InsideBottom;
                    Logger.Write(typeof(SnappingHelper), LogLevel.Debug, "Snapped inside bottom.");
                }
            }
            return result;
        }
    }

    [Flags]
    public enum ResizeDirection : byte
    {
        None = 0,
        Top = 1,
        Bottom = 2,
        Left = 4,
        Right = 8
    }

    [Flags]
    public enum SnapDirection : byte
    {
        None = 0,
        InsideTop = 1,
        InsideBottom = 2,
        InsideLeft = 4,
        InsideRight = 8,
        OutsideTop = 16,
        OutsideBottom = 32,
        OutsideLeft = 64,
        OutsideRight = 128,
    }
}
