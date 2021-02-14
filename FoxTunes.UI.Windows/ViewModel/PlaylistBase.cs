using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes.ViewModel
{
    public abstract class PlaylistBase : ViewModelBase
    {
        protected virtual string LOADING
        {
            get
            {
                return "Loading...";
            }
        }

        protected virtual string UPDATING
        {
            get
            {
                return "Updating...";
            }
        }

        protected virtual string EMPTY
        {
            get
            {
                return "Add to playlist by dropping files here.";
            }
        }

        public IPlaylistBrowser PlaylistBrowser { get; private set; }

        public IScriptingRuntime ScriptingRuntime { get; private set; }

        public IDatabaseFactory DatabaseFactory { get; private set; }

        public ISignalEmitter SignalEmitter { get; private set; }

        public IPlaybackManager PlaybackManager { get; private set; }

        public IPlaylistManager PlaylistManager { get; private set; }

        private PlaylistItemCollection _Items { get; set; }

        public PlaylistItemCollection Items
        {
            get
            {
                return this._Items;
            }
            set
            {
                this._Items = value;
                this.OnItemsChanged();
            }
        }

        protected virtual void OnItemsChanged()
        {
            if (this.ItemsChanged != null)
            {
                this.ItemsChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Items");
        }

        public event EventHandler ItemsChanged;

        protected virtual async Task<string> GetStatusMessage()
        {
            if (this.PlaylistBrowser == null || this.PlaylistManager == null || this.Items == null)
            {
                return LOADING;
            }
            if (this.Items.Count > 0)
            {
                return null;
            }
            switch (this.PlaylistBrowser.State)
            {
                case PlaylistBrowserState.Loading:
                    return LOADING;
            }
            var playlist = await this.GetPlaylist().ConfigureAwait(false);
            var isUpdating = global::FoxTunes.BackgroundTask.Active
                    .OfType<PlaylistTaskBase>()
                    .Any(task => task.Playlist == playlist);
            if (isUpdating)
            {
                return UPDATING;
            }
            else
            {
                return EMPTY;
            }
        }

        private string _StatusMessage { get; set; }

        public virtual string StatusMessage
        {
            get
            {
                return this._StatusMessage;
            }
            set
            {
                this._StatusMessage = value;
                this.OnStatusMessageChanged();
            }
        }

        protected virtual void OnStatusMessageChanged()
        {
            if (this.StatusMessageChanged != null)
            {
                this.StatusMessageChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("StatusMessage");
        }

        public event EventHandler StatusMessageChanged;

        public virtual bool HasStatusMessage
        {
            get
            {
                if (this.Items == null || this.Items.Count == 0)
                {
                    return true;
                }
                return false;
            }
        }

        protected virtual void OnHasStatusMessageChanged()
        {
            if (this.HasStatusMessageChanged != null)
            {
                this.HasStatusMessageChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("HasStatusMessage");
        }

        public event EventHandler HasStatusMessageChanged;

        protected virtual async Task RefreshStatus()
        {
            var statusMessage = await this.GetStatusMessage().ConfigureAwait(false);
            await Windows.Invoke(() =>
            {
                this.StatusMessage = statusMessage;
                this.OnHasStatusMessageChanged();
            }).ConfigureAwait(false);
        }

        protected abstract Task<Playlist> GetPlaylist();

        public override void InitializeComponent(ICore core)
        {
            global::FoxTunes.BackgroundTask.ActiveChanged += this.OnActiveChanged;
            this.DatabaseFactory = this.Core.Factories.Database;
            this.SignalEmitter = this.Core.Components.SignalEmitter;
            this.SignalEmitter.Signal += this.OnSignal;
            this.PlaylistBrowser = this.Core.Components.PlaylistBrowser;
            this.PlaylistBrowser.StateChanged += this.OnStateChanged;
            this.ScriptingRuntime = this.Core.Components.ScriptingRuntime;
            this.PlaylistManager = this.Core.Managers.Playlist;
            this.PlaybackManager = this.Core.Managers.Playback;
            //TODO: Bad .Wait().
            this.RefreshStatus().Wait();
            base.InitializeComponent(core);
        }

        protected virtual void OnActiveChanged(object sender, EventArgs e)
        {
            this.Dispatch(this.RefreshStatus);
        }

        protected virtual async Task OnSignal(object sender, ISignal signal)
        {
            switch (signal.Name)
            {
                case CommonSignals.PlaylistUpdated:
                    var playlists = signal.State as IEnumerable<Playlist>;
                    if (playlists != null && playlists.Any())
                    {
                        var playlist = await this.GetPlaylist().ConfigureAwait(false);
                        if (playlist == null || playlists.Contains(playlist))
                        {
                            await this.RefreshItems().ConfigureAwait(false);
                        }
                    }
                    else
                    {
                        await this.RefreshItems().ConfigureAwait(false);
                    }
                    break;
            }
        }

        protected virtual void OnStateChanged(object sender, EventArgs e)
        {
            this.Dispatch(this.RefreshStatus);
        }

        protected virtual async Task RefreshItems()
        {
            var playlist = await this.GetPlaylist().ConfigureAwait(false);
            await this.RefreshItems(playlist).ConfigureAwait(false);
        }

        protected virtual async Task RefreshItems(Playlist playlist)
        {
            var items = default(PlaylistItem[]);
            if (playlist != null)
            {
                items = this.PlaylistBrowser.GetItems(playlist);
            }
            else
            {
                items = new PlaylistItem[] { };
            }
            if (this.Items == null)
            {
                await Windows.Invoke(() => this.Items = new PlaylistItemCollection(items)).ConfigureAwait(false);
            }
            else
            {
                await Windows.Invoke(this.Items.Reset(items)).ConfigureAwait(false);
            }
            await this.RefreshStatus().ConfigureAwait(false);
        }

        protected override void OnDisposing()
        {
            global::FoxTunes.BackgroundTask.ActiveChanged -= this.OnActiveChanged;
            if (this.PlaylistBrowser != null)
            {
                this.PlaylistBrowser.StateChanged += this.OnStateChanged;
            }
            if (this.SignalEmitter != null)
            {
                this.SignalEmitter.Signal -= this.OnSignal;
            }
            base.OnDisposing();
        }
    }
}
