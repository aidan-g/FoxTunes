using FoxTunes.Interfaces;
using ManagedBass;
using System;
using System.Collections.Generic;

namespace FoxTunes
{
    [Component("04D1138F-395B-4E8F-BE42-5FD7563A01B0", ComponentSlots.None, priority: ComponentAttribute.PRIORITY_LOW)]
    [ComponentDependency(Slot = ComponentSlots.Output)]
    public class BassWasapiStreamOutputBehaviour : StandardBehaviour, IConfigurableComponent, IDisposable
    {
        public ICore Core { get; private set; }

        public IBassOutput Output { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public IBassStreamPipelineFactory BassStreamPipelineFactory { get; private set; }

        new public bool IsInitialized { get; private set; }

        private bool _Enabled { get; set; }

        public bool Enabled
        {
            get
            {
                return this._Enabled;
            }
            set
            {
                this._Enabled = value;
                Logger.Write(this, LogLevel.Debug, "Enabled = {0}", this.Enabled);
                //TODO: Bad .Wait().
                this.Output.Shutdown().Wait();
            }
        }

        private int _WasapiDevice { get; set; }

        public int WasapiDevice
        {
            get
            {
                return this._WasapiDevice;
            }
            set
            {
                this._WasapiDevice = value;
                Logger.Write(this, LogLevel.Debug, "WASAPI Device = {0}", this.WasapiDevice);
                //TODO: Bad .Wait().
                this.Output.Shutdown().Wait();
            }
        }

        private bool _Exclusive { get; set; }

        public bool Exclusive
        {
            get
            {
                return this._Exclusive;
            }
            private set
            {
                this._Exclusive = value;
                Logger.Write(this, LogLevel.Debug, "Exclusive = {0}", this.Exclusive);
                //TODO: Bad .Wait().
                this.Output.Shutdown().Wait();
            }
        }

        public bool AutoFormat
        {
            get
            {
                return !this.Output.EnforceRate;
            }
        }

        private bool _DoubleBuffer { get; set; }

        public bool DoubleBuffer
        {
            get
            {
                return this._DoubleBuffer;
            }
            private set
            {
                this._DoubleBuffer = value;
                Logger.Write(this, LogLevel.Debug, "DoubleBuffer = {0}", this.DoubleBuffer);
                //TODO: Bad .Wait().
                this.Output.Shutdown().Wait();
            }
        }

        private bool _EventDriven { get; set; }

        public bool EventDriven
        {
            get
            {
                return this._EventDriven;
            }
            private set
            {
                this._EventDriven = value;
                Logger.Write(this, LogLevel.Debug, "EventDriven = {0}", this.EventDriven);
                //TODO: Bad .Wait().
                this.Output.Shutdown().Wait();
            }
        }

        private bool _Async { get; set; }

        public bool Async
        {
            get
            {
                return this._Async;
            }
            private set
            {
                this._Async = value;
                Logger.Write(this, LogLevel.Debug, "Async = {0}", this.Async);
                //TODO: Bad .Wait().
                this.Output.Shutdown().Wait();
            }
        }

        private bool _Dither { get; set; }

        public bool Dither
        {
            get
            {
                return this._Dither;
            }
            private set
            {
                this._Dither = value;
                Logger.Write(this, LogLevel.Debug, "Dither = {0}", this.Dither);
                //TODO: Bad .Wait().
                this.Output.Shutdown().Wait();
            }
        }

        private bool _Mixer { get; set; }

        public bool Mixer
        {
            get
            {
                return this._Mixer;
            }
            private set
            {
                this._Mixer = value;
                Logger.Write(this, LogLevel.Debug, "Mixer = {0}", this.Mixer);
                //TODO: Bad .Wait().
                this.Output.Shutdown().Wait();
            }
        }

        private float _BufferLength { get; set; }

        public float BufferLength
        {
            get
            {
                return this._BufferLength;
            }
            private set
            {
                this._BufferLength = value;
                Logger.Write(this, LogLevel.Debug, "BufferLength = {0}", this.BufferLength);
                //TODO: Bad .Wait().
                this.Output.Shutdown().Wait();
            }
        }

        private bool _Raw { get; set; }

        public bool Raw
        {
            get
            {
                return this._Raw;
            }
            private set
            {
                this._Raw = value;
                Logger.Write(this, LogLevel.Debug, "Raw = {0}", this.Raw);
                //TODO: Bad .Wait().
                this.Output.Shutdown().Wait();
            }
        }

        public override void InitializeComponent(ICore core)
        {
            this.Core = core;
            this.Output = core.Components.Output as IBassOutput;
            this.Output.Init += this.OnInit;
            this.Output.Free += this.OnFree;
            this.Configuration = core.Components.Configuration;
            this.Configuration.GetElement<SelectionConfigurationElement>(
                BassOutputConfiguration.SECTION,
                BassOutputConfiguration.OUTPUT_ELEMENT
            ).ConnectValue(value => this.Enabled = string.Equals(value.Id, BassWasapiStreamOutputConfiguration.OUTPUT_WASAPI_OPTION, StringComparison.OrdinalIgnoreCase));
            this.Configuration.GetElement<SelectionConfigurationElement>(
                BassOutputConfiguration.SECTION,
                BassWasapiStreamOutputConfiguration.ELEMENT_WASAPI_DEVICE
            ).ConnectValue(value => this.WasapiDevice = BassWasapiStreamOutputConfiguration.GetWasapiDevice(value));
            this.Configuration.GetElement<BooleanConfigurationElement>(
                BassOutputConfiguration.SECTION,
                BassWasapiStreamOutputConfiguration.ELEMENT_WASAPI_EXCLUSIVE
            ).ConnectValue(value => this.Exclusive = value);
            this.Configuration.GetElement<BooleanConfigurationElement>(
                BassOutputConfiguration.SECTION,
                BassWasapiStreamOutputConfiguration.ELEMENT_WASAPI_EVENT
            ).ConnectValue(value => this.EventDriven = value);
            this.Configuration.GetElement<BooleanConfigurationElement>(
                BassOutputConfiguration.SECTION,
                BassWasapiStreamOutputConfiguration.ELEMENT_WASAPI_ASYNC
            ).ConnectValue(value => this.Async = value);
            this.Configuration.GetElement<BooleanConfigurationElement>(
                BassOutputConfiguration.SECTION,
                BassWasapiStreamOutputConfiguration.ELEMENT_WASAPI_DITHER
            ).ConnectValue(value => this.Dither = value);
            this.Configuration.GetElement<BooleanConfigurationElement>(
                BassOutputConfiguration.SECTION,
                BassWasapiStreamOutputConfiguration.MIXER_ELEMENT
            ).ConnectValue(value => this.Mixer = value);
            this.Configuration.GetElement<BooleanConfigurationElement>(
                BassOutputConfiguration.SECTION,
                BassWasapiStreamOutputConfiguration.DOUBLE_BUFFER_ELEMENT
            ).ConnectValue(value => this.DoubleBuffer = value);
            this.Configuration.GetElement<DoubleConfigurationElement>(
                BassOutputConfiguration.SECTION,
                BassWasapiStreamOutputConfiguration.BUFFER_LENGTH_ELEMENT
            ).ConnectValue(value => this.BufferLength = Convert.ToSingle(value));
            this.Configuration.GetElement<BooleanConfigurationElement>(
                BassOutputConfiguration.SECTION,
                BassWasapiStreamOutputConfiguration.ELEMENT_WASAPI_RAW
            ).ConnectValue(value => this.Raw = value);
            this.BassStreamPipelineFactory = ComponentRegistry.Instance.GetComponent<IBassStreamPipelineFactory>();
            if (this.BassStreamPipelineFactory != null)
            {
                this.BassStreamPipelineFactory.QueryingPipeline += this.OnQueryingPipeline;
                this.BassStreamPipelineFactory.CreatingPipeline += this.OnCreatingPipeline;
            }
            base.InitializeComponent(core);
        }

        protected virtual void OnInit(object sender, EventArgs e)
        {
            if (!this.Enabled)
            {
                return;
            }
            this.IsInitialized = true;
            BassUtils.OK(Bass.Configure(global::ManagedBass.Configuration.UpdateThreads, 0));
            BassUtils.OK(Bass.Configure(global::ManagedBass.Configuration.PlaybackBufferLength, this.Output.BufferLength));
            BassUtils.OK(Bass.Configure(global::ManagedBass.Configuration.SRCQuality, this.Output.ResamplingQuality));
            BassUtils.OK(Bass.Init(Bass.NoSoundDevice));
            //Always detect device for now.
            //if (BassWasapiDevice.Info != null && BassWasapiDevice.Info.Device != this.WasapiDevice)
            {
                BassWasapiDevice.Detect(this.WasapiDevice, this.Exclusive, this.AutoFormat, this.BufferLength, this.DoubleBuffer, this.EventDriven, this.Async, this.Dither, this.Raw);
            }
            Logger.Write(this, LogLevel.Debug, "BASS (No Sound) Initialized.");
        }

        protected virtual void OnFree(object sender, EventArgs e)
        {
            if (!this.IsInitialized)
            {
                return;
            }
            this.OnFreeDevice();
            LogManager.Logger.Write(this, LogLevel.Debug, "Releasing BASS.");
            Bass.Free();
            this.IsInitialized = false;
        }

        protected virtual void OnFreeDevice()
        {
            if (!BassWasapiDevice.IsInitialized)
            {
                return;
            }
            BassWasapiDevice.Free();
        }

        protected virtual void OnQueryingPipeline(object sender, QueryingPipelineEventArgs e)
        {
            if (!this.Enabled)
            {
                return;
            }
            e.OutputRates = BassWasapiDevice.Info.SupportedRates;
            e.OutputChannels = BassWasapiDevice.Info.Outputs;
        }

        protected virtual void OnCreatingPipeline(object sender, CreatingPipelineEventArgs e)
        {
            if (!this.Enabled)
            {
                return;
            }
            var output = new BassWasapiStreamOutput(this, e.Stream);
            output.InitializeComponent(this.Core);
            e.Output = output;
        }

        public IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return BassWasapiStreamOutputConfiguration.GetConfigurationSections();
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
            if (this.Output != null)
            {
                this.Output.Init -= this.OnInit;
                this.Output.Free -= this.OnFree;
            }
            if (this.BassStreamPipelineFactory != null)
            {
                this.BassStreamPipelineFactory.QueryingPipeline -= this.OnQueryingPipeline;
                this.BassStreamPipelineFactory.CreatingPipeline -= this.OnCreatingPipeline;
            }
        }

        ~BassWasapiStreamOutputBehaviour()
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
