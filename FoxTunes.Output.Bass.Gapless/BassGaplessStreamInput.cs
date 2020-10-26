using FoxTunes.Interfaces;
using ManagedBass;
using ManagedBass.Gapless;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FoxTunes
{
    public class BassGaplessStreamInput : BassStreamInput
    {
        public BassGaplessStreamInput(BassGaplessStreamInputBehaviour behaviour, BassOutputStream stream)
        {
            this.Behaviour = behaviour;
            this.Channels = stream.Channels;
            this.Flags = BassFlags.Decode;
            if (BassUtils.GetChannelDsdRaw(stream.ChannelHandle))
            {
                this.Rate = BassUtils.GetChannelDsdRate(stream.ChannelHandle);
                this.Flags |= BassFlags.DSDRaw;
            }
            else
            {
                this.Rate = stream.Rate;
                if (behaviour.Output.Float)
                {
                    this.Flags |= BassFlags.Float;
                }
            }
        }

        public override string Name
        {
            get
            {
                return "Gapless";
            }
        }

        public override string Description
        {
            get
            {
                return string.Format(
                    "{0} ({1}/{2}/{3})",
                    this.Name,
                    BassUtils.DepthDescription(this.Flags),
                    MetaDataInfo.SampleRateDescription(this.Rate),
                    MetaDataInfo.ChannelDescription(this.Channels)
                );
            }
        }

        public BassGaplessStreamInputBehaviour Behaviour { get; private set; }

        public override int Rate { get; protected set; }

        public override int Channels { get; protected set; }

        public override BassFlags Flags { get; protected set; }

        public override int ChannelHandle { get; protected set; }

        public override IEnumerable<int> Queue
        {
            get
            {
                var count = default(int);
                return BassGapless.GetChannels(out count);
            }
        }

        public override void Connect(IBassStreamComponent previous)
        {
            BassUtils.OK(BassGapless.SetConfig(BassGaplessAttriubute.KeepAlive, true));
            Logger.Write(this, LogLevel.Debug, "Creating BASS GAPLESS stream with rate {0} and {1} channels.", this.Rate, this.Channels);
            this.ChannelHandle = BassGapless.StreamCreate(this.Rate, this.Channels, this.Flags);
            if (this.ChannelHandle == 0)
            {
                BassUtils.Throw();
            }
        }

        public override bool CheckFormat(BassOutputStream stream)
        {
            var rate = default(int);
            var channels = default(int);
            if (BassUtils.GetChannelDsdRaw(stream.ChannelHandle))
            {
                rate = BassUtils.GetChannelDsdRate(stream.ChannelHandle);
                channels = BassUtils.GetChannelCount(stream.ChannelHandle);
            }
            else
            {
                rate = BassUtils.GetChannelPcmRate(stream.ChannelHandle);
                channels = BassUtils.GetChannelCount(stream.ChannelHandle);
            }
            return this.Rate == rate && this.Channels == channels;
        }

        public override bool Contains(BassOutputStream stream)
        {
            return this.Queue.Contains(stream.ChannelHandle);
        }

        public override int Position(BassOutputStream stream)
        {
            var count = default(int);
            var channelHandles = BassGapless.GetChannels(out count);
            return channelHandles.IndexOf(stream.ChannelHandle);
        }

        public override bool Add(BassOutputStream stream)
        {
            if (this.Queue.Contains(stream.ChannelHandle))
            {
                Logger.Write(this, LogLevel.Debug, "Stream is already enqueued: {0}", stream.ChannelHandle);
                return false;
            }
            Logger.Write(this, LogLevel.Debug, "Adding stream to the queue: {0}", stream.ChannelHandle);
            BassUtils.OK(BassGapless.ChannelEnqueue(stream.ChannelHandle));
            return true;
        }

        public override bool Remove(BassOutputStream stream, Action<BassOutputStream> callBack)
        {
            if (!this.Queue.Contains(stream.ChannelHandle))
            {
                Logger.Write(this, LogLevel.Debug, "Stream is not enqueued: {0}", stream.ChannelHandle);
                return false;
            }
            Logger.Write(this, LogLevel.Debug, "Removing stream from the queue: {0}", stream.ChannelHandle);
            BassUtils.OK(BassGapless.ChannelRemove(stream.ChannelHandle));
            callBack(stream);
            return true;
        }

        public override void Reset()
        {
            Logger.Write(this, LogLevel.Debug, "Resetting the queue.");
            foreach (var channelHandle in this.Queue)
            {
                Logger.Write(this, LogLevel.Debug, "Removing stream from the queue: {0}", channelHandle);
                BassGapless.ChannelRemove(channelHandle);
            }
            this.ClearBuffer();
        }

        protected override void OnDisposing()
        {
            if (this.ChannelHandle != 0)
            {
                this.Reset();
                Logger.Write(this, LogLevel.Debug, "Freeing BASS GAPLESS stream: {0}", this.ChannelHandle);
                Bass.StreamFree(this.ChannelHandle); //Not checking result code as it contains an error if the application is shutting down.
            }
        }
    }
}
