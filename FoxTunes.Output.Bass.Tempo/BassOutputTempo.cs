using FoxTunes.Interfaces;
using System;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.Output)]
    [ComponentDependency(Slot = ComponentSlots.UserInterface)]
    public class BassOutputTempo : BassOutputEffect, IOutputTempo, IStandardComponent, IDisposable
    {
        public override bool Enabled
        {
            get
            {
                return this.EnabledElement.Value;
            }
            set
            {
                this.EnabledElement.Value = value;
                if (this.IsInitialized)
                {
                    this.Configuration.Save();
                }
            }
        }

        public int MinValue
        {
            get
            {
                return Tempo.MIN_TEMPO;
            }
        }

        public int MaxValue
        {
            get
            {
                return Tempo.MAX_TEMPO;
            }
        }

        public int Value
        {
            get
            {
                return this.TempoElement.Value;
            }
            set
            {
                this.TempoElement.Value = value;
                if (this.IsInitialized)
                {
                    this.Configuration.Save();
                }
            }
        }

        protected virtual void OnValueChanged()
        {
            if (this.ValueChanged != null)
            {
                this.ValueChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Value");
        }

        public event EventHandler ValueChanged;

        public int MinPitch
        {
            get
            {
                return Tempo.MIN_PITCH;
            }
        }

        public int MaxPitch
        {
            get
            {
                return Tempo.MAX_PITCH;
            }
        }

        public int Pitch
        {
            get
            {
                return this.PitchElement.Value;
            }
            set
            {
                this.PitchElement.Value = value;
                if (this.IsInitialized)
                {
                    this.Configuration.Save();
                }
            }
        }

        protected virtual void OnPitchChanged()
        {
            if (this.PitchChanged != null)
            {
                this.PitchChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Pitch");
        }

        public event EventHandler PitchChanged;

        public int MinRate
        {
            get
            {
                return Tempo.MIN_RATE;
            }
        }

        public int MaxRate
        {
            get
            {
                return Tempo.MAX_RATE;
            }
        }

        public int Rate
        {
            get
            {
                return this.RateElement.Value;
            }
            set
            {
                this.RateElement.Value = value;
                if (this.IsInitialized)
                {
                    this.Configuration.Save();
                }
            }
        }

        protected virtual void OnRateChanged()
        {
            if (this.RateChanged != null)
            {
                this.RateChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Rate");
        }

        public event EventHandler RateChanged;

        public ICore Core { get; private set; }

        public BooleanConfigurationElement EnabledElement { get; private set; }

        public IntegerConfigurationElement TempoElement { get; private set; }

        public IntegerConfigurationElement PitchElement { get; private set; }

        public IntegerConfigurationElement RateElement { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            base.InitializeComponent(core);
            this.Core = core;
            this.EnabledElement = this.Configuration.GetElement<BooleanConfigurationElement>(
                BassOutputConfiguration.SECTION,
                BassOutputTempoStreamComponentBehaviourConfiguration.ENABLED
            );
            this.TempoElement = this.Configuration.GetElement<IntegerConfigurationElement>(
                BassOutputConfiguration.SECTION,
                BassOutputTempoStreamComponentBehaviourConfiguration.TEMPO
            );
            this.PitchElement = this.Configuration.GetElement<IntegerConfigurationElement>(
                BassOutputConfiguration.SECTION,
                BassOutputTempoStreamComponentBehaviourConfiguration.PITCH
            );
            this.RateElement = this.Configuration.GetElement<IntegerConfigurationElement>(
                BassOutputConfiguration.SECTION,
                BassOutputTempoStreamComponentBehaviourConfiguration.RATE
            );
            this.EnabledElement.ValueChanged += this.OnEnabledChanged;
            this.TempoElement.ValueChanged += this.OnValueChanged;
            this.PitchElement.ValueChanged += this.OnPitchChanged;
            this.RateElement.ValueChanged += this.OnRateChanged;
        }

        protected virtual void OnEnabledChanged(object sender, EventArgs e)
        {
            this.OnEnabledChanged();
        }

        protected virtual void OnValueChanged(object sender, EventArgs e)
        {
            this.OnValueChanged();
        }

        protected virtual void OnPitchChanged(object sender, EventArgs e)
        {
            this.OnPitchChanged();
        }

        protected virtual void OnRateChanged(object sender, EventArgs e)
        {
            this.OnRateChanged();
        }

        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.IsDisposed || !disposing)
            {
                return;
            }
            this.OnDisposing();
            this.IsDisposed = true;
        }

        protected virtual void OnDisposing()
        {
            if (this.EnabledElement != null)
            {
                this.EnabledElement.ValueChanged -= this.OnEnabledChanged;
            }
            if (this.TempoElement != null)
            {
                this.TempoElement.ValueChanged -= this.OnValueChanged;
            }
            if (this.PitchElement != null)
            {
                this.PitchElement.ValueChanged -= this.OnPitchChanged;
            }
            if (this.RateElement != null)
            {
                this.RateElement.ValueChanged -= this.OnRateChanged;
            }
        }

        ~BassOutputTempo()
        {
            Logger.Write(this, LogLevel.Error, "Component was not disposed: {0}", this.GetType().Name);
            try
            {
                this.Dispose(true);
            }
            catch
            {
                //Nothing can be done, never throw on GC thread.
            }
        }
    }
}
