using FoxTunes.Interfaces;
using ManagedBass;
using ManagedBass.Crossfade;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FoxTunes
{
    public class BassCrossfadeStreamInput : BassStreamInput, IBassStreamControllable
    {
        public BassCrossfadeStreamInput(BassCrossfadeStreamInputBehaviour behaviour, BassOutputStream stream)
        {
            this.Behaviour = behaviour;
            this.Channels = stream.Channels;
            this.Flags = BassFlags.Decode;
            if (BassUtils.GetChannelDsdRaw(stream.ChannelHandle))
            {
                throw new InvalidOperationException("Cannot apply effects to DSD streams.");
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
                return "Fading";
            }
        }

        public override string Description
        {
            get
            {
                return string.Format(
                    "{0} ({1:0.00s}/{2:0.00s}) ({3}/{4}/{5})",
                    this.Name,
                    (float)this.Behaviour.InPeriod / 1000,
                    (float)this.Behaviour.OutPeriod / 1000,
                    BassUtils.DepthDescription(this.Flags),
                    MetaDataInfo.SampleRateDescription(this.Rate),
                    MetaDataInfo.ChannelDescription(this.Channels)
                );
            }
        }

        public BassCrossfadeStreamInputBehaviour Behaviour { get; private set; }

        public override int Rate { get; protected set; }

        public override int Channels { get; protected set; }

        public override BassFlags Flags { get; protected set; }

        public override int ChannelHandle { get; protected set; }

        public override IEnumerable<int> Queue
        {
            get
            {
                var count = default(int);
                return BassCrossfade.GetChannels(out count);
            }
        }

        public override bool PreserveBuffer
        {
            get
            {
                //Disable clearing the buffer when the current track is removed as this would prevent fade out.
                return true;
            }
        }

        public override void Connect(IBassStreamComponent previous)
        {
            //BassUtils.OK(BassGapless.SetConfig(BassGaplessAttriubute.KeepAlive, true));
            Logger.Write(this, LogLevel.Debug, "Creating BASS CROSSFADE stream with rate {0} and {1} channels.", this.Rate, this.Channels);
            this.ChannelHandle = BassCrossfade.StreamCreate(this.Rate, this.Channels, this.Flags);
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
            var channelHandles = BassCrossfade.GetChannels(out count);
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
            //If there's nothing in the queue then we're starting.
            if (this.Queue.Count() == 0)
            {
                var flags = default(BassCrossfadeFlags);
                if (this.Behaviour.Start)
                {
                    flags = BassCrossfadeFlags.FadeIn;
                }
                else
                {
                    flags = BassCrossfadeFlags.None;
                }
                BassUtils.OK(BassCrossfade.ChannelEnqueue(stream.ChannelHandle, flags));
                return true;
            }
            BassUtils.OK(BassCrossfade.ChannelEnqueue(stream.ChannelHandle));
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
            //If there's only one stream in the queue then we're stopping. 
            //Block so the fade out behaviour can be applied before Reset is called.
            if (this.Queue.Count() == 1)
            {
                BassUtils.OK(BassCrossfade.StreamReset(this.Behaviour.Stop));
                callBack(stream);
                return true;
            }
            //Fork so fade out doesn't block the next track being enqueued.
            this.Dispatch(() =>
            {
                BassUtils.OK(BassCrossfade.ChannelRemove(stream.ChannelHandle));
                callBack(stream);
            });
            return true;
        }

        public override void Reset()
        {
            Logger.Write(this, LogLevel.Debug, "Resetting the queue.");
            foreach (var channelHandle in this.Queue)
            {
                Logger.Write(this, LogLevel.Debug, "Removing stream from the queue: {0}", channelHandle);
                BassCrossfade.ChannelRemove(channelHandle);
            }
            this.ClearBuffer();
        }

        protected override void OnDisposing()
        {
            if (this.ChannelHandle != 0)
            {
                this.Reset();
                Logger.Write(this, LogLevel.Debug, "Freeing BASS CROSSFADE stream: {0}", this.ChannelHandle);
                Bass.StreamFree(this.ChannelHandle); //Not checking result code as it contains an error if the application is shutting down.
            }
        }

        #region IBassStreamControllable

        public void PreviewPlay()
        {
            //The begin/end seek hooks don't block so this doesn't really work.
            //if (this.Behaviour.PauseSeek)
            //{
            //    BassCrossfade.StreamFadeIn();
            //}
        }

        private bool Fading { get; set; }

        public void PreviewPause()
        {
            if (this.Behaviour.PauseResume)
            {
                this.Fading = true;
                BassCrossfade.StreamFadeOut();
            }
        }

        public void PreviewResume()
        {
            if (this.Fading || this.Behaviour.PauseResume)
            {
                this.Fading = false;
                BassCrossfade.StreamFadeIn();
            }
        }

        public void PreviewStop()
        {
            //The begin/end seek hooks don't block so this doesn't really work.
            //if (this.Behaviour.PauseSeek)
            //{
            //    BassCrossfade.StreamFadeOut();
            //}
        }

        public void Play()
        {
            //Nothing to do.
        }

        public void Pause()
        {
            //Nothing to do.
        }

        public void Resume()
        {
            //Nothing to do.
        }

        public void Stop()
        {
            //Nothing to do.
        }

        #endregion
    }
}
