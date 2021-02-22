using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.UserInterface)]
    public class LyricsBehaviour : StandardBehaviour, IInvocableComponent, IConfigurableComponent
    {
        public const string CATEGORY = "BB46B834-5372-440F-B75B-57FF0E473BB4";

        public const string EDIT = "0000";

        public const string LOOKUP = "0001";

        public LyricsBehaviour()
        {
            this.Providers = new List<LyricsProvider>();
        }

        public List<LyricsProvider> Providers { get; private set; }

        public IPlaybackManager PlaybackManager { get; private set; }

        public IMetaDataManager MetaDataManager { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public BooleanConfigurationElement Enabled { get; private set; }

        public BooleanConfigurationElement AutoScroll { get; private set; }

        public BooleanConfigurationElement AutoLookup { get; private set; }

        public SelectionConfigurationElement AutoLookupProvider { get; private set; }

        public TextConfigurationElement Editor { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.PlaybackManager = core.Managers.Playback;
            this.MetaDataManager = core.Managers.MetaData;
            this.Configuration = core.Components.Configuration;
            this.Enabled = this.Configuration.GetElement<BooleanConfigurationElement>(
                MetaDataBehaviourConfiguration.SECTION,
                MetaDataBehaviourConfiguration.READ_LYRICS_TAGS
            );
            this.AutoScroll = this.Configuration.GetElement<BooleanConfigurationElement>(
                LyricsBehaviourConfiguration.SECTION,
                LyricsBehaviourConfiguration.AUTO_SCROLL
            );
            this.AutoLookup = this.Configuration.GetElement<BooleanConfigurationElement>(
                LyricsBehaviourConfiguration.SECTION,
                LyricsBehaviourConfiguration.AUTO_LOOKUP
            );
            this.AutoLookup.ConnectValue(value =>
            {
                if (value)
                {
                    Logger.Write(this, LogLevel.Debug, "Enabling auto lookup.");
                    this.PlaybackManager.CurrentStreamChanged += this.OnCurrentStreamChanged;
                }
                else
                {
                    Logger.Write(this, LogLevel.Debug, "Disabling auto lookup.");
                    this.PlaybackManager.CurrentStreamChanged -= this.OnCurrentStreamChanged;
                }
            });
            this.AutoLookupProvider = this.Configuration.GetElement<SelectionConfigurationElement>(
                LyricsBehaviourConfiguration.SECTION,
                LyricsBehaviourConfiguration.AUTO_LOOKUP_PROVIDER
            );
            this.Editor = this.Configuration.GetElement<TextConfigurationElement>(
                LyricsBehaviourConfiguration.SECTION,
                LyricsBehaviourConfiguration.EDITOR
            );
            base.InitializeComponent(core);
        }

        protected virtual void OnCurrentStreamChanged(object sender, EventArgs e)
        {
            //Critical: Don't block in this event handler, it causes a deadlock.
            this.Dispatch(this.Lookup);
        }

        public IEnumerable<IInvocationComponent> Invocations
        {
            get
            {
                if (this.Enabled.Value)
                {
                    if (this.PlaybackManager.CurrentStream != null)
                    {
                        yield return new InvocationComponent(
                            CATEGORY,
                            EDIT,
                            "Edit"
                        );
                        foreach (var provider in this.Providers)
                        {
                            yield return new InvocationComponent(
                                CATEGORY,
                                LOOKUP,
                                provider.Name,
                                path: "Lookup"
                            );
                        }
                    }
                    yield return new InvocationComponent(
                        CATEGORY,
                        this.AutoScroll.Id,
                        this.AutoScroll.Name,
                        attributes: (byte)(InvocationComponent.ATTRIBUTE_SEPARATOR | (this.AutoScroll.Value ? InvocationComponent.ATTRIBUTE_SELECTED : InvocationComponent.ATTRIBUTE_NONE))
                    );
                    yield return new InvocationComponent(
                        CATEGORY,
                        this.AutoLookup.Id,
                        this.AutoLookup.Name,
                        attributes: this.AutoLookup.Value ? InvocationComponent.ATTRIBUTE_SELECTED : InvocationComponent.ATTRIBUTE_NONE
                    );
                }
            }
        }

        public Task InvokeAsync(IInvocationComponent component)
        {
            switch (component.Id)
            {
                case EDIT:
                    return this.Edit();
                case LOOKUP:
                    var provider = this.Providers.FirstOrDefault(
                        _provider => string.Equals(_provider.Name, component.Name, StringComparison.OrdinalIgnoreCase)
                    );
                    if (provider != null)
                    {
                        return this.Lookup(provider);
                    }
#if NET40
                    return TaskEx.FromResult(false);
#else
                    return Task.CompletedTask;
#endif
            }
            if (string.Equals(component.Id, this.AutoScroll.Id, StringComparison.OrdinalIgnoreCase))
            {
                this.AutoScroll.Toggle();
                this.Configuration.Save();
            }
            else if (string.Equals(component.Id, this.AutoLookup.Id, StringComparison.OrdinalIgnoreCase))
            {
                this.AutoLookup.Toggle();
                this.Configuration.Save();
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        public async Task Edit()
        {
            var outputStream = this.PlaybackManager.CurrentStream;
            if (outputStream == null)
            {
                return;
            }
            var playlistItem = outputStream.PlaylistItem;
            var metaDataItem = default(MetaDataItem);
            lock (playlistItem.MetaDatas)
            {
                metaDataItem = playlistItem.MetaDatas.FirstOrDefault(
                    element => string.Equals(element.Name, CommonMetaData.Lyrics, StringComparison.OrdinalIgnoreCase)
                );
            }
            var fileName = Path.GetTempFileName();
            Logger.Write(this, LogLevel.Debug, "Editing lyrics for file \"{0}\": \"{1}\"", playlistItem.FileName, fileName);
            if (metaDataItem != null)
            {
                File.WriteAllText(fileName, metaDataItem.Value);
            }
            else
            {
                File.WriteAllText(fileName, string.Empty);
            }
            var args = string.Format("\"{0}\"", fileName);
            Logger.Write(this, LogLevel.Debug, "Using editor: \"{0}\"", this.Editor.Value);
            var process = Process.Start(this.Editor.Value, args);
            await process.WaitForExitAsync().ConfigureAwait(false);
            if (process.ExitCode != 0)
            {
                Logger.Write(this, LogLevel.Warn, "Process does not indicate success: Code = {0}", process.ExitCode);
                return;
            }
            if (metaDataItem == null)
            {
                metaDataItem = new MetaDataItem(CommonMetaData.Lyrics, MetaDataItemType.Tag);
                lock (playlistItem.MetaDatas)
                {
                    playlistItem.MetaDatas.Add(metaDataItem);
                }
            }
            metaDataItem.Value = File.ReadAllText(fileName);
            await this.MetaDataManager.Save(
                new[] { playlistItem },
                true,
                false,
                CommonMetaData.Lyrics
            ).ConfigureAwait(false);
        }

        public Task Lookup()
        {
            var outputStream = this.PlaybackManager.CurrentStream;
            if (outputStream == null)
            {
#if NET40
                return TaskEx.FromResult(false);
#else
                return Task.CompletedTask;
#endif
            }
            var playlistItem = outputStream.PlaylistItem;
            lock (playlistItem.MetaDatas)
            {
                var metaDataItem = playlistItem.MetaDatas.FirstOrDefault(
                    element => string.Equals(element.Name, CommonMetaData.Lyrics, StringComparison.OrdinalIgnoreCase)
                );
                if (metaDataItem != null && !string.IsNullOrEmpty(metaDataItem.Value))
                {
                    Logger.Write(this, LogLevel.Debug, "Lyrics already defined for file \"{0}\", nothing to do.", playlistItem.FileName);
#if NET40
                    return TaskEx.FromResult(false);
#else
                    return Task.CompletedTask;
#endif
                }
            }
            var provider = default(LyricsProvider);
            if (this.AutoLookupProvider.Value != null)
            {
                provider = this.Providers.FirstOrDefault(
                   _provider => string.Equals(_provider.Id, this.AutoLookupProvider.Value.Id, StringComparison.OrdinalIgnoreCase)
               );
            }
            if (provider == null)
            {
                Logger.Write(this, LogLevel.Warn, "Failed to determine the preferred provider.");
                provider = this.Providers.FirstOrDefault();
                if (provider == null)
                {
                    Logger.Write(this, LogLevel.Warn, "No providers.");
#if NET40
                    return TaskEx.FromResult(false);
#else
                    return Task.CompletedTask;
#endif
                }
            }
            return this.Lookup(provider);
        }

        public Task Lookup(LyricsProvider provider)
        {
            var outputStream = this.PlaybackManager.CurrentStream;
            if (outputStream == null)
            {
#if NET40
                return TaskEx.FromResult(false);
#else
                return Task.CompletedTask;
#endif
            }
            var playlistItem = outputStream.PlaylistItem;
            return this.Lookup(provider, playlistItem);
        }

        public async Task Lookup(LyricsProvider provider, PlaylistItem playlistItem)
        {
            Logger.Write(this, LogLevel.Debug, "Looking up lyrics for file \"{0}\"..", playlistItem.FileName);
            var result = await provider.Lookup(playlistItem).ConfigureAwait(false);
            if (!result.Success)
            {
                Logger.Write(this, LogLevel.Warn, "Failed to look up lyrics for file \"{0}\".", playlistItem.FileName);
                return;
            }
            Logger.Write(this, LogLevel.Debug, "Looking up lyrics for file \"{0}\": OK, updating meta data.", playlistItem.FileName);
            lock (playlistItem.MetaDatas)
            {
                var metaDataItem = playlistItem.MetaDatas.FirstOrDefault(
                    element => string.Equals(element.Name, CommonMetaData.Lyrics, StringComparison.OrdinalIgnoreCase)
                );
                if (metaDataItem == null)
                {
                    metaDataItem = new MetaDataItem(CommonMetaData.Lyrics, MetaDataItemType.Tag);
                    lock (playlistItem.MetaDatas)
                    {
                        playlistItem.MetaDatas.Add(metaDataItem);
                    }
                }
                metaDataItem.Value = result.Lyrics;
            }
            await this.MetaDataManager.Save(
                new[] { playlistItem },
                true,
                false,
                CommonMetaData.Lyrics
            ).ConfigureAwait(false);
        }

        public void Register(LyricsProvider provider)
        {
            this.Providers.Add(provider);
        }

        public IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return LyricsBehaviourConfiguration.GetConfigurationSections();
        }
    }
}
