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

        public static SnapDirection Snap(Rectangle from, Rectangle to, ref Point offset, bool inside)
        {
            var result = SnapDirection.None;
            var proximity = Proximity.Value;
            if (from.Bottom >= to.Top - proximity && from.Top <= to.Bottom + proximity)
            {
                if (inside)
                {
                    if ((Math.Abs(from.Left - to.Left) <= proximity))
                    {
                        offset.X = -(from.Left - to.Left);
                        result |= SnapDirection.Left;
                        Logger.Write(typeof(SnappingHelper), LogLevel.Debug, "Snap inside left.");
                    }
                    if ((Math.Abs(from.Right - to.Right) <= proximity))
                    {
                        offset.X = -(from.Right - to.Right);
                        result |= SnapDirection.Right;
                        Logger.Write(typeof(SnappingHelper), LogLevel.Debug, "Snap inside right.");
                    }
                }
                else
                {
                    if (Math.Abs(from.Right - to.Left) <= proximity)
                    {
                        offset.X = -(from.Right - to.Left);
                        result |= SnapDirection.Left;
                        Logger.Write(typeof(SnappingHelper), LogLevel.Debug, "Snap left.");
                    }
                    else if (Math.Abs(from.Left - to.Left) <= proximity)
                    {
                        offset.X = -(from.Left - to.Left);
                        result |= SnapDirection.Left;
                        Logger.Write(typeof(SnappingHelper), LogLevel.Debug, "Snap left.");
                    }
                    if (Math.Abs(from.Left - to.Right) <= proximity)
                    {
                        offset.X = -(from.Left - to.Right);
                        result |= SnapDirection.Right;
                        Logger.Write(typeof(SnappingHelper), LogLevel.Debug, "Snap right.");
                    }
                    else if (Math.Abs(from.Right - to.Right) <= proximity)
                    {
                        offset.X = -(from.Right - to.Right);
                        result |= SnapDirection.Right;
                        Logger.Write(typeof(SnappingHelper), LogLevel.Debug, "Snap right.");
                    }
                }
            }
            if (from.Right >= to.Left - proximity && from.Left <= to.Right + proximity)
            {
                if (inside)
                {
                    if (Math.Abs(from.Top - to.Top) <= proximity)
                    {
                        offset.Y = -(from.Top - to.Top);
                        result |= SnapDirection.Top;
                        Logger.Write(typeof(SnappingHelper), LogLevel.Debug, "Snap inside top.");
                    }
                    if (Math.Abs(from.Bottom - to.Bottom) <= proximity)
                    {
                        offset.Y = -(from.Bottom - to.Bottom);
                        result |= SnapDirection.Bottom;
                        Logger.Write(typeof(SnappingHelper), LogLevel.Debug, "Snap inside bottom.");
                    }
                }
                else
                {
                    if (Math.Abs(from.Bottom - to.Top) <= proximity)
                    {
                        offset.Y = -(from.Bottom - to.Top);
                        result |= SnapDirection.Top;
                        Logger.Write(typeof(SnappingHelper), LogLevel.Debug, "Snap top.");
                    }
                    else if (Math.Abs(from.Top - to.Top) <= proximity)
                    {
                        offset.Y = -(from.Top - to.Top);
                        result |= SnapDirection.Top;
                        Logger.Write(typeof(SnappingHelper), LogLevel.Debug, "Snap top.");
                    }
                    if (Math.Abs(from.Top - to.Bottom) <= proximity)
                    {
                        offset.Y = -(from.Top - to.Bottom);
                        result |= SnapDirection.Bottom;
                        Logger.Write(typeof(SnappingHelper), LogLevel.Debug, "Snap bottom.");
                    }
                    else if (Math.Abs(from.Bottom - to.Bottom) <= proximity)
                    {
                        offset.Y = -(from.Bottom - to.Bottom);
                        result |= SnapDirection.Bottom;
                        Logger.Write(typeof(SnappingHelper), LogLevel.Debug, "Snap bottom.");
                    }
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
        Top = 1,
        Bottom = 2,
        Left = 4,
        Right = 8
    }
}
