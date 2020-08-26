using FoxTunes.Interfaces;
using ManagedBass.ReplayGain;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Linq;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.Output)]
    public class BassReplayGainBehaviour : StandardBehaviour, IConfigurableComponent, IDisposable
    {
        public BassReplayGainBehaviour()
        {
            BassReplayGain.Init();
            this.Effects = new ConditionalWeakTable<BassOutputStream, ReplayGainEffect>();
        }

        public ConditionalWeakTable<BassOutputStream, ReplayGainEffect> Effects { get; private set; }

        public ICore Core { get; private set; }

        public IBassOutput Output { get; private set; }

        public IBassStreamPipelineFactory BassStreamPipelineFactory { get; private set; }

        public IMetaDataManager MetaDataManager { get; private set; }

        public ISignalEmitter SignalEmitter { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public ReplayGainMode Mode { get; private set; }

        public bool OnDemand { get; private set; }

        public bool WriteTags { get; private set; }

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
            }
        }

        public override void InitializeComponent(ICore core)
        {
            this.Core = core;
            this.Output = ComponentRegistry.Instance.GetComponent<IBassOutput>();
            this.BassStreamPipelineFactory = ComponentRegistry.Instance.GetComponent<IBassStreamPipelineFactory>();
            this.MetaDataManager = core.Managers.MetaData;
            this.SignalEmitter = core.Components.SignalEmitter;
            this.Configuration = core.Components.Configuration;
            this.Configuration.GetElement<BooleanConfigurationElement>(
                BassOutputConfiguration.SECTION,
                BassReplayGainBehaviourConfiguration.ENABLED
            ).ConnectValue(value => this.Enabled = value);
            this.Configuration.GetElement<SelectionConfigurationElement>(
                BassOutputConfiguration.SECTION,
                BassReplayGainBehaviourConfiguration.MODE
            ).ConnectValue(option => this.Mode = BassReplayGainBehaviourConfiguration.GetMode(option));
            this.Configuration.GetElement<BooleanConfigurationElement>(
                BassOutputConfiguration.SECTION,
                BassReplayGainBehaviourConfiguration.ON_DEMAND
            ).ConnectValue(value => this.OnDemand = value);
            this.Configuration.GetElement<BooleanConfigurationElement>(
                BassOutputConfiguration.SECTION,
                BassReplayGainScannerBehaviourConfiguration.WRITE_TAGS
            ).ConnectValue(value => this.WriteTags = value);
            if (this.Output != null)
            {
                this.Output.Loaded += this.OnLoaded;
                this.Output.Unloaded += this.OnUnloaded;
            }
            if (BassStreamPipelineFactory != null)
            {
                this.BassStreamPipelineFactory.CreatingPipeline += this.OnCreatingPipeline;
            }
            base.InitializeComponent(core);
        }

        protected virtual void OnLoaded(object sender, OutputStreamEventArgs e)
        {
            if (!this.Enabled)
            {
                return;
            }
            if (e.Stream is BassOutputStream stream)
            {
                this.Add(stream);
            }
        }

        protected virtual void OnUnloaded(object sender, OutputStreamEventArgs e)
        {
            if (e.Stream is BassOutputStream stream)
            {
                this.Remove(stream);
            }
        }

        protected virtual void OnCreatingPipeline(object sender, CreatingPipelineEventArgs e)
        {
            if (BassUtils.GetChannelDsdRaw(e.Stream.ChannelHandle))
            {
                //Cannot apply effects to DSD.
                return;
            }
            var component = new BassReplayGainStreamComponent(this, e.Stream);
            component.InitializeComponent(this.Core);
            e.Components.Add(component);
        }

        protected virtual void Add(BassOutputStream stream)
        {
            if (!FileSystemHelper.IsLocalPath(stream.FileName))
            {
                return;
            }
            if (BassUtils.GetChannelDsdRaw(stream.ChannelHandle))
            {
                return;
            }
            var gain = default(float);
            var peak = default(float);
            var mode = default(ReplayGainMode);
            if (!this.TryGetReplayGain(stream, out gain, out mode))
            {
                if (this.OnDemand)
                {
                    //TODO: Bad .Result
                    using (var duplicated = this.Output.Duplicate(stream).Result as BassOutputStream)
                    {
                        if (duplicated == null)
                        {
                            Logger.Write(this, LogLevel.Warn, "Failed to duplicate stream for file \"{0}\", cannot calculate.", stream.FileName);
                            return;
                        }
                        if (!this.TryCalculateReplayGain(duplicated, out gain, out peak, out mode))
                        {
                            return;
                        }
                    }
                    this.Dispatch(() => this.UpdateMetaData(stream, gain, peak, mode));
                }
                else
                {
                    return;
                }
            }
            var effect = new ReplayGainEffect(stream.ChannelHandle, gain, mode);
            effect.Activate();
            this.Effects.Add(stream, effect);
        }

        protected virtual void Remove(BassOutputStream stream)
        {
            var effect = default(ReplayGainEffect);
            if (this.Effects.TryGetValue(stream, out effect))
            {
                this.Effects.Remove(stream);
                effect.Dispose();
            }
        }

        protected virtual bool TryGetReplayGain(BassOutputStream stream, out float replayGain, out ReplayGainMode mode)
        {
            var albumGain = default(float);
            var trackGain = default(float);
            lock (stream.PlaylistItem.MetaDatas)
            {
                foreach (var metaDataItem in stream.PlaylistItem.MetaDatas)
                {
                    if (string.Equals(metaDataItem.Name, CommonMetaData.ReplayGainAlbumGain, StringComparison.OrdinalIgnoreCase))
                    {
                        if (float.TryParse(metaDataItem.Value, out albumGain))
                        {
                            if (!this.IsValidReplayGain(albumGain))
                            {
                                albumGain = default(float);
                                continue;
                            }
                            if (this.Mode == ReplayGainMode.Album)
                            {
                                Logger.Write(this, LogLevel.Debug, "Found preferred replay gain data for album:  \"{0}\" => {1}", stream.FileName, albumGain);
                                mode = ReplayGainMode.Album;
                                replayGain = albumGain;
                                return true;
                            }
                        }
                    }
                    else if (string.Equals(metaDataItem.Name, CommonMetaData.ReplayGainTrackGain, StringComparison.OrdinalIgnoreCase))
                    {
                        if (float.TryParse(metaDataItem.Value, out trackGain))
                        {
                            if (!this.IsValidReplayGain(trackGain))
                            {
                                trackGain = default(float);
                                continue;
                            }
                            if (this.Mode == ReplayGainMode.Track)
                            {
                                Logger.Write(this, LogLevel.Debug, "Found preferred replay gain data for track:  \"{0}\" => {1}", stream.FileName, trackGain);
                                mode = ReplayGainMode.Track;
                                replayGain = trackGain;
                                return true;
                            }
                        }
                    }
                }
            }
            if (this.IsValidReplayGain(albumGain))
            {
                Logger.Write(this, LogLevel.Debug, "Using album replay gain data: \"{0}\" => {1}", stream.FileName, albumGain);
                mode = ReplayGainMode.Album;
                replayGain = albumGain;
                return true;
            }
            if (this.IsValidReplayGain(trackGain))
            {
                Logger.Write(this, LogLevel.Debug, "Using track replay gain data: \"{0}\" => {1}", stream.FileName, trackGain);
                mode = ReplayGainMode.Track;
                replayGain = trackGain;
                return true;
            }
            Logger.Write(this, LogLevel.Debug, "No replay gain data: \"{0}\".", stream.FileName);
            mode = ReplayGainMode.None;
            replayGain = 0;
            return false;
        }

        protected virtual bool IsValidReplayGain(float replayGain)
        {
            //TODO: I'm sure there is a valid range of values.
            return replayGain != 0 && !float.IsNaN(replayGain);
        }

        protected virtual bool TryCalculateReplayGain(BassOutputStream stream, out float gain, out float peak, out ReplayGainMode mode)
        {
            Logger.Write(this, LogLevel.Debug, "Attempting to calculate track replay gain for file \"{0}\".", stream.FileName);
            try
            {
                var info = default(ReplayGainInfo);
                if (BassReplayGain.Process(stream.ChannelHandle, out info))
                {
                    Logger.Write(this, LogLevel.Debug, "Calculated track replay gain for file \"{0}\": {1}dB", stream.FileName, ReplayGainEffect.GetVolume(info.gain));
                    gain = info.gain;
                    peak = info.peak;
                    mode = ReplayGainMode.Track;
                    return true;
                }
                else
                {
                    Logger.Write(this, LogLevel.Warn, "Failed to calculate track replay gain for file \"{0}\".", stream.FileName);
                }
            }
            catch (Exception e)
            {
                Logger.Write(this, LogLevel.Warn, "Failed to calculate track replay gain for file \"{0}\": {1}", stream.FileName, e.Message);
            }
            gain = 0;
            peak = 0;
            mode = ReplayGainMode.None;
            return false;
        }

        protected virtual async Task UpdateMetaData(BassOutputStream stream, float gain, float peak, ReplayGainMode mode)
        {
            var names = new HashSet<string>();
            lock (stream.PlaylistItem.MetaDatas)
            {
                var metaDatas = stream.PlaylistItem.MetaDatas.ToDictionary(
                    element => element.Name,
                    StringComparer.OrdinalIgnoreCase
                );
                var metaDataItem = default(MetaDataItem);
                if (gain != 0)
                {
                    var name = default(string);
                    switch (mode)
                    {
                        case ReplayGainMode.Album:
                            name = CommonMetaData.ReplayGainAlbumGain;
                            break;
                        case ReplayGainMode.Track:
                            name = CommonMetaData.ReplayGainTrackGain;
                            break;
                    }
                    if (!metaDatas.TryGetValue(name, out metaDataItem))
                    {
                        metaDataItem = new MetaDataItem(name, MetaDataItemType.Tag);
                        stream.PlaylistItem.MetaDatas.Add(metaDataItem);
                    }
                    metaDataItem.Value = Convert.ToString(gain);
                    names.Add(name);
                }
                if (peak != 0)
                {
                    var name = default(string);
                    switch (mode)
                    {
                        case ReplayGainMode.Album:
                            name = CommonMetaData.ReplayGainAlbumPeak;
                            break;
                        case ReplayGainMode.Track:
                            name = CommonMetaData.ReplayGainTrackPeak;
                            break;
                    }
                    if (!metaDatas.TryGetValue(name, out metaDataItem))
                    {
                        metaDataItem = new MetaDataItem(name, MetaDataItemType.Tag);
                        stream.PlaylistItem.MetaDatas.Add(metaDataItem);
                    }
                    metaDataItem.Value = Convert.ToString(peak);
                    names.Add(name);
                }
            }
            await this.MetaDataManager.Save(
                new[] { stream.PlaylistItem },
                this.WriteTags,
                false,
                names.ToArray()
            ).ConfigureAwait(false);
        }

        public IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return BassReplayGainBehaviourConfiguration.GetConfigurationSections();
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
                this.Output.Loaded -= this.OnLoaded;
                this.Output.Unloaded -= this.OnUnloaded;
            }
            if (BassStreamPipelineFactory != null)
            {
                this.BassStreamPipelineFactory.CreatingPipeline -= this.OnCreatingPipeline;
            }
            BassReplayGain.Free();
        }

        ~BassReplayGainBehaviour()
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
