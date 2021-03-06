﻿using FoxTunes;
using FoxTunes.Interfaces;
using ManagedBass;
using ManagedBass.Dts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FoxTunes
{
    public class BassDtsStreamProvider : BassStreamProvider
    {
        public static string Location
        {
            get
            {
                return Path.GetDirectoryName(typeof(BassDtsStreamProvider).Assembly.Location);
            }
        }

        public static readonly string[] EXTENSIONS = new[]
        {
            "dts"
        };

        public BassDtsStreamProvider()
        {
            BassPluginLoader.AddPath(Path.Combine(Location, "Addon"));
            BassPluginLoader.AddExtensions(EXTENSIONS);
        }

        public override bool CanCreateStream(PlaylistItem playlistItem)
        {
            if (!EXTENSIONS.Contains(playlistItem.FileName.GetExtension(), StringComparer.OrdinalIgnoreCase))
            {
                return false;
            }
            return true;
        }

        public override IBassStream CreateBasicStream(PlaylistItem playlistItem, IEnumerable<IBassStreamAdvice> advice, BassFlags flags)
        {
            var fileName = this.GetFileName(playlistItem, advice);
            var channelHandle = BassDts.CreateStream(fileName, 0, 0, flags);
            return this.CreateBasicStream(channelHandle, advice, flags);
        }

        public override IBassStream CreateInteractiveStream(PlaylistItem playlistItem, IEnumerable<IBassStreamAdvice> advice, BassFlags flags)
        {
            var fileName = this.GetFileName(playlistItem, advice);
            var channelHandle = default(int);
            if (this.Output != null && this.Output.PlayFromMemory)
            {
                Logger.Write(this, LogLevel.Warn, "This provider cannot play from memory.");
            }
            channelHandle = BassDts.CreateStream(fileName, 0, 0, flags);
            return this.CreateInteractiveStream(channelHandle, advice, flags);
        }
    }
}
