using FoxTunes.Interfaces;
using ManagedBass;
using ManagedBass.Substream;
using System;
using System.Collections.Generic;

namespace FoxTunes
{
    public class BassCueStreamAdvice : BassStreamAdvice
    {
        static BassCueStreamAdvice()
        {
            BassSubstream.Init();
            BassSubstream.FreeParent = true;
        }

        public BassCueStreamAdvice(string fileName, string offset, string length) : base(fileName)
        {
            this.Offset = CueSheetIndex.ToTimeSpan(offset);
            this.Length = CueSheetIndex.ToTimeSpan(length);
        }

        public TimeSpan Offset { get; private set; }

        public TimeSpan Length { get; private set; }

        public override bool Wrap(IBassStreamProvider provider, int channelHandle, IEnumerable<IBassStreamAdvice> advice, BassFlags flags, out IBassStream stream)
        {
            var offset = default(long);
            var length = default(long);
            if (this.Offset != TimeSpan.Zero)
            {
                offset = Bass.ChannelSeconds2Bytes(channelHandle, this.Offset.TotalSeconds);
            }
            if (this.Length != TimeSpan.Zero)
            {
                length = Bass.ChannelSeconds2Bytes(channelHandle, this.Length.TotalSeconds);
            }
            if (offset != 0 || length != 0)
            {
                if (length == 0)
                {
                    length = Bass.ChannelGetLength(channelHandle, PositionFlags.Bytes) - offset;
                }
                stream = new BassStream(
                    provider,
                    BassSubstream.CreateStream(channelHandle, offset, length, flags),
                    length,
                    advice,
                    flags
                );
                return true;
            }
            stream = null;
            return false;
        }
    }
}
