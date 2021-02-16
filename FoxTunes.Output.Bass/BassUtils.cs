using ManagedBass;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace FoxTunes
{
    public static class BassUtils
    {
        private static readonly ConcurrentDictionary<string, bool> SUPPORTED_FORMATS = new ConcurrentDictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

        public static readonly HashSet<string> BUILT_IN_FORMATS = new HashSet<string>(new[]
        {
            "mp1", "mp2", "mp3", "ogg", "wav", "aif"
        });

        public static int GetDeviceNumber(int device)
        {
            if (device != Bass.DefaultDevice)
            {
                return device;
            }
            for (var a = 0; a < Bass.DeviceCount; a++)
            {
                var deviceInfo = default(DeviceInfo);
                OK(Bass.GetDeviceInfo(a, out deviceInfo));
                if (deviceInfo.IsDefault)
                {
                    return a;
                }
            }
            throw new BassException(Errors.Device);
        }

        public static IEnumerable<string> GetInputFormats()
        {
            var formats = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var format in BUILT_IN_FORMATS)
            {
                formats.Add(format);
            }
            foreach (var plugin in BassPluginLoader.Instance.Plugins)
            {
                foreach (var format in plugin.Info.Formats)
                {
                    foreach (var extension in format.FileExtensions.Split(';'))
                    {
                        formats.Add(extension.TrimStart('*', '.'));
                    }
                }
            }
            return formats;
        }

        public static bool IsSupported(string extension)
        {
            var supported = default(bool);
            if (!SUPPORTED_FORMATS.TryGetValue(extension, out supported))
            {
                supported = GetInputFormats().Contains(extension, true);
                SUPPORTED_FORMATS.TryAdd(extension, supported);
            }
            return supported;
        }

        public static string DepthDescription(BassFlags flags)
        {
            if (flags.HasFlag(BassFlags.DSDRaw))
            {
                return "DSD";
            }
            if (flags.HasFlag(BassFlags.Float))
            {
                return "32";
            }
            return "16";
        }

        public static bool OK(bool result)
        {
            if (!result)
            {
                if (Bass.LastError != Errors.OK)
                {
                    Throw();
                }
            }
            return result;
        }

        public static int OK(int result)
        {
            if (result == 0)
            {
                if (Bass.LastError != Errors.OK)
                {
                    Throw();
                }
            }
            return result;
        }

        public static void Throw()
        {
            if (Bass.LastError != Errors.OK)
            {
                throw new BassException(Bass.LastError);
            }
        }

        public static bool GetChannelFloat(int channelHandle)
        {
            return GetChannelFlags(channelHandle).HasFlag(BassFlags.Float);
        }

        public static bool GetChannelDsdRaw(int channelHandle)
        {
            return GetChannelFlags(channelHandle).HasFlag(BassFlags.DSDRaw);
        }

        public static int GetChannelCount(int channelHandle)
        {
            var channelInfo = default(ChannelInfo);
            OK(Bass.ChannelGetInfo(channelHandle, out channelInfo));
            return channelInfo.Channels;
        }

        public static int GetChannelPcmRate(int channelHandle)
        {
            return Convert.ToInt32(Bass.ChannelGetAttribute(channelHandle, ChannelAttribute.Frequency));
        }

        public static int GetChannelDsdRate(int channelHandle)
        {
            return Convert.ToInt32(Bass.ChannelGetAttribute(channelHandle, ChannelAttribute.DSDRate));
        }

        public static BassFlags GetChannelFlags(int channelHandle)
        {
            var channelInfo = default(ChannelInfo);
            OK(Bass.ChannelGetInfo(channelHandle, out channelInfo));
            return channelInfo.Flags;
        }
    }

    public class BassException : Exception
    {
        public BassException(Errors error)
            : this(GetMessage(error), error)
        {

        }

        public BassException(string message, Errors error)
            : base(message)
        {
            this.Error = error;
        }

        public Errors Error { get; private set; }

        private static string GetMessage(Errors error)
        {
            //TODO: Create a message based on the error code.
            var message = Enum.GetName(typeof(Errors), error);
            if (!string.IsNullOrEmpty(message))
            {
                return message;
            }
            return string.Format("Error code {0}", Convert.ToInt32(error));
        }
    }
}
