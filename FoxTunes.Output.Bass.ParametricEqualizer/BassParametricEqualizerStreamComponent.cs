using FoxTunes.Interfaces;
using ManagedBass;
using System;
using System.Linq;

namespace FoxTunes
{
    public class BassParametricEqualizerStreamComponent : BassStreamComponent
    {
        public BassParametricEqualizerStreamComponent(BassParametricEqualizerStreamComponentBehaviour behaviour, BassOutputStream stream)
        {
            if (BassUtils.GetChannelDsdRaw(stream.ChannelHandle))
            {
                throw new InvalidOperationException("Cannot apply effects to DSD streams.");
            }
            this.Behaviour = behaviour;
            this.Rate = behaviour.Output.Rate;
            this.Channels = stream.Channels;
            this.Flags = BassFlags.Decode;
            if (this.Behaviour.Output.Float)
            {
                this.Flags |= BassFlags.Float;
            }
        }

        public override string Name
        {
            get
            {
                return "ParametricEqualizer";
            }
        }

        public override string Description
        {
            get
            {
                if (!this.IsActive)
                {
                    return string.Format("{0} (none)", this.Name);
                }
                var bands = string.Join(",", this.OutputEffects.Equalizer.Bands.Where(
                    band => band.Value != 0
                ).Select(
                    band => GetDescription(band)
                ));
                return string.Format(
                    "{0} ({1})",
                    this.Name,
                    bands
                );
            }
        }

        public BassParametricEqualizerStreamComponentBehaviour Behaviour { get; private set; }

        public PeakEQ PeakEQ { get; private set; }

        public override int Rate { get; protected set; }

        public override int Channels { get; protected set; }

        public override BassFlags Flags { get; protected set; }

        public override int ChannelHandle { get; protected set; }

        public override bool IsActive
        {
            get
            {
                if (this.OutputEffects == null || this.OutputEffects.Equalizer == null)
                {
                    return false;
                }
                if (!this.OutputEffects.Equalizer.Available || !this.OutputEffects.Equalizer.Enabled || this.OutputEffects.Equalizer.Bands.All(band => band.Value == 0))
                {
                    return false;
                }
                return true;
            }
        }

        public IOutputEffects OutputEffects { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.OutputEffects = core.Components.OutputEffects;
            if (this.OutputEffects.Equalizer != null)
            {
                this.OutputEffects.Equalizer.EnabledChanged += this.OnEnabledChanged;
                foreach (var band in this.OutputEffects.Equalizer.Bands)
                {
                    band.WidthChanged += this.OnWidthChanged;
                    band.ValueChanged += this.OnValueChanged;
                }
            }
            base.InitializeComponent(core);
        }

        protected virtual void OnEnabledChanged(object sender, EventArgs e)
        {
            if (this.IsActive)
            {
                this.Update();
            }
            else
            {
                this.Stop();
            }
        }

        protected virtual void OnWidthChanged(object sender, EventArgs e)
        {
            if (this.IsActive)
            {
                this.Update();
            }
            else
            {
                this.Stop();
            }
        }

        private void OnValueChanged(object sender, EventArgs e)
        {
            if (this.IsActive)
            {
                this.Update();
            }
            else
            {
                this.Stop();
            }
        }

        protected virtual void Update()
        {
            if (this.PeakEQ == null)
            {
                this.PeakEQ = new PeakEQ(this.ChannelHandle);
                this.PeakEQ.InitializeComponent(this.Behaviour.Core);
            }
            foreach (var band in this.OutputEffects.Equalizer.Bands)
            {
                this.Update(band);
            }
        }

        protected virtual void Update(IOutputEqualizerBand band)
        {
            if (this.OutputEffects.Equalizer.Enabled)
            {
                this.PeakEQ.UpdateBand(band.Position, band.Width, band.Center, band.Value);
            }
        }

        protected virtual void Stop()
        {
            if (this.PeakEQ != null)
            {
                this.PeakEQ.Dispose();
                this.PeakEQ = null;
            }
        }

        public override void Connect(IBassStreamComponent previous)
        {
            this.Rate = previous.Rate;
            this.Channels = previous.Channels;
            this.ChannelHandle = previous.ChannelHandle;
            if (this.IsActive)
            {
                this.Update();
            }
        }

        protected virtual float GetEffectiveGain()
        {
            var gain = 0f;
            foreach (var band in this.OutputEffects.Equalizer.Bands)
            {
                if (band.Value > 0)
                {
                    gain += band.Value;
                }
            }
            return gain;
        }

        protected override void OnDisposing()
        {
            if (this.OutputEffects != null && this.OutputEffects.Equalizer != null)
            {
                this.OutputEffects.Equalizer.EnabledChanged -= this.OnEnabledChanged;
                foreach (var band in this.OutputEffects.Equalizer.Bands)
                {
                    band.WidthChanged -= this.OnWidthChanged;
                    band.ValueChanged -= this.OnValueChanged;
                }
            }
            this.Stop();
        }

        public static string GetDescription(IOutputEqualizerBand band)
        {
            return string.Format(
                "{0}/{1}dB",
                band.Value > 0 ? "+" + band.Value.ToString() : band.Value.ToString(),
                band.Center < 1000 ? band.Center.ToString() + "Hz" : band.Center.ToString() + "kHz"
            );
        }
    }
}
