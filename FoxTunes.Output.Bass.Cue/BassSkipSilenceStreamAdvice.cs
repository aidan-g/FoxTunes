using FoxTunes.Interfaces;
using ManagedBass;
using ManagedBass.Substream;
using System;
using System.Collections.Generic;

namespace FoxTunes
{
    public class BassSkipSilenceStreamAdvice : BassStreamAdvice
    {
        static BassSkipSilenceStreamAdvice()
        {
            BassSubstream.Init();
            BassSubstream.FreeParent = true;
        }

        public BassSkipSilenceStreamAdvice(string fileName, TimeSpan leadIn, TimeSpan leadOut) : base(fileName)
        {
            this.LeadIn = leadIn;
            this.LeadOut = leadOut;
        }

        public TimeSpan LeadIn { get; private set; }

        public TimeSpan LeadOut { get; private set; }

        public override bool Wrap(IBassStreamProvider provider, int channelHandle, IEnumerable<IBassStreamAdvice> advice, BassFlags flags, out IBassStream stream)
        {
            var offset = default(long);
            var length = default(long);
            if (this.LeadIn != TimeSpan.Zero)
            {
                offset = Bass.ChannelSeconds2Bytes(
                    channelHandle,
                    this.LeadIn.TotalSeconds
                );
            }
            if (this.LeadOut != TimeSpan.Zero)
            {
                length = Bass.ChannelGetLength(
                    channelHandle,
                    PositionFlags.Bytes
                ) - Bass.ChannelSeconds2Bytes(
                    channelHandle,
                    this.LeadOut.TotalSeconds
                ) - offset;
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
