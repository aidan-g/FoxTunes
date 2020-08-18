using System;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace FoxTunes
{
    public static partial class Extensions
    {
        public static bool IsLighter(this Color color)
        {
            return color.A > byte.MaxValue / 2 && color.R > byte.MaxValue / 2 && color.G > byte.MaxValue / 2 && color.B > byte.MaxValue / 2;
        }

        public static Color Shade(this Color color1, Color color2)
        {
            if (color1.IsLighter())
            {
                //Create darner shade.
                return new Color()
                {
                    A = Convert.ToByte(Math.Min(color1.A - color2.A, byte.MaxValue)),
                    R = Convert.ToByte(Math.Min(color1.R - color2.R, byte.MaxValue)),
                    G = Convert.ToByte(Math.Min(color1.G - color2.G, byte.MaxValue)),
                    B = Convert.ToByte(Math.Min(color1.B - color2.B, byte.MaxValue))
                };
            }
            else
            {
                //Create lighter shade.
                return new Color()
                {
                    A = Convert.ToByte(Math.Min(color1.A + color2.A, byte.MaxValue)),
                    R = Convert.ToByte(Math.Min(color1.R + color2.R, byte.MaxValue)),
                    G = Convert.ToByte(Math.Min(color1.G + color2.G, byte.MaxValue)),
                    B = Convert.ToByte(Math.Min(color1.B + color2.B, byte.MaxValue))
                };
            }
        }

        public static Color[] ToPair(this Color color, byte shade)
        {
            var contrast = new Color()
            {
                R = shade,
                G = shade,
                B = shade
            };
            if (color.IsLighter())
            {
                return new[]
                {
                    color.Shade(contrast),
                    color
                };
            }
            else
            {
                return new[]
                {
                    color,
                    color.Shade(contrast)
                };
            }
        }
    }
}
