using System;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FoxTunes
{
    public static class BitmapHelper
    {
        public static void Clear(RenderInfo info)
        {
            memset(info.Buffer, 0, new UIntPtr((uint)(info.Height * info.Stride)));
        }

        public static void DrawRectangle(RenderInfo info, int x, int y, int width, int height)
        {
#if DEBUG
            //Check arguments are valid.

            if (x < 0 || y < 0 || width < 0 || height < 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (x + width > info.Width || y + height > info.Height)
            {
                throw new ArgumentOutOfRangeException();
            }
#endif

            var topLeft = IntPtr.Add(info.Buffer, (x * info.BytesPerPixel) + (y * info.Stride));

            //Set initial pixel.
            memset(IntPtr.Add(topLeft, 0), info.Blue, new UIntPtr(1));
            memset(IntPtr.Add(topLeft, 1), info.Green, new UIntPtr(1));
            memset(IntPtr.Add(topLeft, 2), info.Red, new UIntPtr(1));
            memset(IntPtr.Add(topLeft, 3), info.Alpha, new UIntPtr(1));

            //Fill first line by copying initial pixel.
            var position = 1;
            var linePosition = IntPtr.Add(topLeft, info.BytesPerPixel);
            for (position = 1; position <= width;)
            {
                //Double the number of pixels we copy until there isn't enough room.
                if (position * 2 <= width)
                {
                    memcpy(linePosition, topLeft, new UIntPtr((uint)(position * info.BytesPerPixel)));
                    linePosition = IntPtr.Add(linePosition, position * info.BytesPerPixel);
                    position *= 2;
                }
                //Fill the remainder.
                else
                {
                    var remaining = width - position;
                    memcpy(linePosition, topLeft, new UIntPtr((uint)(remaining * info.BytesPerPixel)));
                    break;
                }
            }
            if (height > 0)
            {
                //Fill each other line by copying the first line.
                var lineStart = IntPtr.Add(topLeft, info.Stride);
                for (position = 1; position < height; position++)
                {
                    memcpy(lineStart, topLeft, new UIntPtr((uint)(width * info.BytesPerPixel)));
                    lineStart = IntPtr.Add(lineStart, info.Stride);
                }
            }
        }

        [DllImport("msvcrt.dll", EntryPoint = "memset", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        public static extern IntPtr memset(IntPtr destination, int value, UIntPtr count);

        [DllImport("msvcrt.dll", EntryPoint = "memcpy", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        private static extern IntPtr memcpy(IntPtr destination, IntPtr source, UIntPtr count);

        public static RenderInfo CreateRenderInfo(WriteableBitmap bitmap, Color color)
        {
            if (bitmap.Format != PixelFormats.Pbgra32)
            {
                throw new NotImplementedException();
            }

            return new RenderInfo()
            {
                BytesPerPixel = bitmap.Format.BitsPerPixel / 8,
                Width = bitmap.PixelWidth,
                Height = bitmap.PixelHeight,
                Stride = bitmap.PixelWidth * (bitmap.Format.BitsPerPixel / 8),
                Buffer = bitmap.BackBuffer,
                Blue = color.B,
                Green = color.G,
                Red = color.R,
                Alpha = color.A
            };
        }

        public struct RenderInfo
        {
            public int BytesPerPixel;

            public int Width;

            public int Height;

            public int Stride;

            public IntPtr Buffer;

            public int Blue;

            public int Green;

            public int Red;

            public int Alpha;
        }
    }
}
