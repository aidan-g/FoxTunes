using FoxTunes.Interfaces;
using ManagedBass;
using ManagedBass.Mix;
using ManagedBass.Wasapi;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FoxTunes
{
    public class BassWasapiStreamOutput : BassStreamOutput
    {
        private BassWasapiStreamOutput()
        {
            this.Flags = BassFlags.Default;
            this.MixerChannelHandles = new HashSet<int>();
        }

        public BassWasapiStreamOutput(BassWasapiStreamOutputBehaviour behaviour, BassOutputStream stream)
            : this()
        {
            this.Behaviour = behaviour;
            this.Rate = behaviour.Output.Rate;
            this.Channels = BassWasapiDevice.Info.Outputs;
            //WASAPI requires BASS_SAMPLE_FLOAT so don't bother respecting the output's Float setting.
            this.Flags = BassFlags.Decode | BassFlags.Float;
        }

        public override string Name
        {
            get
            {
                return "WASAPI";
            }
        }

        public override string Description
        {
            get
            {
                return string.Format(
                    "{0} ({1}/{2} {3})",
                    this.Name,
                    BassUtils.DepthDescription(this.Flags),
                    MetaDataInfo.SampleRateDescription(this.Rate),
                    MetaDataInfo.ChannelDescription(this.Channels)
                );
            }
        }

        public BassWasapiStreamOutputBehaviour Behaviour { get; private set; }

        public override int Rate { get; protected set; }

        public override int Channels { get; protected set; }

        public override BassFlags Flags { get; protected set; }

        public override int ChannelHandle { get; protected set; }

        public HashSet<int> MixerChannelHandles { get; protected set; }

        protected override IEnumerable<int> GetMixerChannelHandles()
        {
            return this.MixerChannelHandles;
        }

        public override bool CheckFormat(int rate, int channels)
        {
            return BassWasapiDevice.Info.SupportedRates.Contains(rate) && channels <= BassWasapiDevice.Info.Outputs;
        }

        public override void Connect(IBassStreamComponent previous)
        {
            this.ConfigureWASAPI(previous);
            if (this.ShouldCreateMixer(previous))
            {
                Logger.Write(this, LogLevel.Debug, "Creating BASS MIX stream with rate {0} and {1} channels.", this.Rate, this.Channels);
                this.ChannelHandle = BassMix.CreateMixerStream(this.Rate, this.Channels, this.Flags);
                if (this.ChannelHandle == 0)
                {
                    BassUtils.Throw();
                }
                Logger.Write(this, LogLevel.Debug, "Adding stream to the mixer: {0}", previous.ChannelHandle);
                BassUtils.OK(BassMix.MixerAddChannel(this.ChannelHandle, previous.ChannelHandle, BassFlags.Default | BassFlags.MixerBuffer));
                BassUtils.OK(BassWasapiHandler.StreamSet(this.ChannelHandle));
                this.MixerChannelHandles.Add(previous.ChannelHandle);
            }
            else
            {
                Logger.Write(this, LogLevel.Debug, "The stream properties match the device, playing directly.");
                BassUtils.OK(BassWasapiHandler.StreamSet(previous.ChannelHandle));
            }
            this.UpdateVolume();
        }

        protected virtual void ConfigureWASAPI(IBassStreamComponent previous)
        {
            if (this.Behaviour.Output.EnforceRate)
            {
                if (!BassWasapiDevice.Info.SupportedRates.Contains(this.Rate))
                {
                    var nearestRate = BassWasapiDevice.Info.GetNearestRate(this.Rate);
                    Logger.Write(this, LogLevel.Warn, "Enforced rate {0} isn't supposed by the device, falling back to {1}.", this.Rate, nearestRate);
                    this.Rate = nearestRate;
                }
                else
                {
                    //Enfoced rate is supported by the device, nothing to do.
                }
            }
            else
            {
                if (!BassWasapiDevice.Info.SupportedRates.Contains(previous.Rate))
                {
                    var nearestRate = BassWasapiDevice.Info.GetNearestRate(previous.Rate);
                    Logger.Write(this, LogLevel.Debug, "Stream rate {0} isn't supposed by the device, falling back to {1}.", this.Rate, nearestRate);
                    this.Rate = nearestRate;
                }
                else
                {
                    //Stream rate is supported by the device, nothing to do.
                    this.Rate = previous.Rate;
                }
            }
            BassWasapiDevice.Init(
                this.Behaviour.WasapiDevice,
                this.Behaviour.Exclusive,
                this.Behaviour.AutoFormat,
                this.Behaviour.BufferLength,
                this.Behaviour.DoubleBuffer,
                this.Behaviour.EventDriven,
                this.Behaviour.Async,
                this.Behaviour.Dither,
                this.Behaviour.Raw,
                this.Rate,
                this.Channels
            );
        }

        protected virtual bool StartWASAPI()
        {
            if (BassWasapi.IsStarted)
            {
                Logger.Write(this, LogLevel.Debug, "WASAPI has already been started.");
                return false;
            }
            Logger.Write(this, LogLevel.Debug, "Starting WASAPI.");
            BassUtils.OK(BassWasapi.Start());
            return true;
        }

        protected virtual bool StopWASAPI(bool reset)
        {
            if (!BassWasapi.IsStarted)
            {
                Logger.Write(this, LogLevel.Debug, "WASAPI has not been started.");
                return false;
            }
            Logger.Write(this, LogLevel.Debug, "Stopping WASAPI.");
            BassUtils.OK(BassWasapi.Stop(reset));
            return true;
        }

        protected virtual bool ShouldCreateMixer(IBassStreamComponent previous)
        {
            if (this.Behaviour.Mixer)
            {
                //Mixer is forced on, probably so visualizations work.
                return true;
            }
            else if (previous.Rate != this.Rate || previous.Channels != this.Channels)
            {
                //Stream rate or channel count differs from device.
                return true;
            }
            else if (!previous.Flags.HasFlag(BassFlags.Float))
            {
                //WASAPI is always 32 bit.
                return true;
            }
            //Looks like no mixer is required.
            return false;
        }

        public override bool IsPlaying
        {
            get
            {
                return BassWasapi.IsStarted;
            }
            protected set
            {
                throw new NotImplementedException();
            }
        }

        public override bool IsPaused { get; protected set; }

        public override bool IsStopped
        {
            get
            {
                return !BassWasapi.IsStarted;
            }
            protected set
            {
                throw new NotImplementedException();
            }
        }

        public override int Latency
        {
            get
            {
                return 0;
            }
        }

        public override void Play()
        {
            if (this.IsPlaying)
            {
                return;
            }
            Logger.Write(this, LogLevel.Debug, "Starting WASAPI.");
            try
            {
                BassUtils.OK(this.StartWASAPI());
                this.IsPaused = false;
            }
            catch (Exception e)
            {
                this.OnError(e);
                throw;
            }
        }

        public override void Pause()
        {
            if (this.IsStopped)
            {
                return;
            }
            Logger.Write(this, LogLevel.Debug, "Pausing WASAPI.");
            try
            {
                BassUtils.OK(this.StopWASAPI(false));
                this.IsPaused = true;
            }
            catch (Exception e)
            {
                this.OnError(e);
                throw;
            }
        }

        public override void Resume()
        {
            this.Play();
        }

        public override void Stop()
        {
            if (this.IsStopped)
            {
                return;
            }
            try
            {
                BassUtils.OK(this.StopWASAPI(true));
                this.IsPaused = false;
            }
            catch (Exception e)
            {
                this.OnError(e);
                throw;
            }
        }

        protected override float GetVolume()
        {
            if (this.Behaviour.Exclusive)
            {
                return 1;
            }
            return BassWasapi.GetVolume(WasapiVolumeTypes.Session);
        }

        protected override void SetVolume(float volume)
        {
            if (this.Behaviour.Exclusive)
            {
                return;
            }
            if (!BassWasapi.SetVolume(WasapiVolumeTypes.Session, volume))
            {
                //TODO: Warn.
            }
        }

        protected override void OnDisposing()
        {
            if (this.ChannelHandle != 0)
            {
                Logger.Write(this, LogLevel.Debug, "Freeing BASS stream: {0}", this.ChannelHandle);
                Bass.StreamFree(this.ChannelHandle); //Not checking result code as it contains an error if the application is shutting down.
            }
            this.Stop();
            BassWasapiDevice.Free();
            base.OnDisposing();
        }
    }
}
