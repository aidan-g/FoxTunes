using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace FoxTunes.ViewModel
{
    public class Playlists : ViewModelBase
    {
        public IPlaylistManager PlaylistManager { get; private set; }

        public IPlaylistBrowser PlaylistBrowser { get; private set; }

        public ISignalEmitter SignalEmitter { get; private set; }

        private PlaylistCollection _Items { get; set; }

        public PlaylistCollection Items
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

        public Playlist SelectedItem
        {
            get
            {
                if (this.PlaylistManager == null)
                {
                    return Playlist.Empty;
                }
                return this.PlaylistManager.SelectedPlaylist;
            }
            set
            {
                if (this.PlaylistManager == null || value == null)
                {
                    return;
                }
                this.PlaylistManager.SelectedPlaylist = value;
            }
        }

        protected virtual void OnSelectedItemChanged()
        {
            if (this.SelectedItemChanged != null)
            {
                this.SelectedItemChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("SelectedItem");
        }

        public event EventHandler SelectedItemChanged;

        protected virtual Task RefreshItems()
        {
            var playlists = this.PlaylistBrowser.GetPlaylists();
            if (this.Items == null)
            {
                return Windows.Invoke(() => this.Items = new PlaylistCollection(playlists));
            }
            else
            {
                return Windows.Invoke(this.Items.Reset(playlists));
            }
        }

        protected virtual Task RefreshItems(IEnumerable<Playlist> playlists)
        {
            var cached = this.PlaylistBrowser.GetPlaylists();
            if (this.Items == null)
            {
                return Windows.Invoke(() => this.Items = new PlaylistCollection(cached));
            }
            else
            {
                var updated = new List<Playlist>();
                var removed = new List<Playlist>();
                foreach (var playlist in playlists)
                {
                    if (cached.Contains(playlist))
                    {
                        updated.Add(playlist);
                    }
                    else
                    {
                        removed.Add(playlist);
                    }
                }
                return Windows.Invoke(() =>
                {
                    this.Items.AddOrUpdate(updated);
                    this.Items.Remove(removed);
                });
            }
        }

        public virtual async Task Reload(IEnumerable<Playlist> playlists)
        {
            await this.RefreshItems(playlists).ConfigureAwait(false);
            await Windows.Invoke(() =>
            {
                this.OnSelectedItemChanged();
            }).ConfigureAwait(false);
        }

        public virtual async Task Reload()
        {
            await this.RefreshItems().ConfigureAwait(false);
            await Windows.Invoke(() =>
            {
                this.OnSelectedItemChanged();
            }).ConfigureAwait(false);
        }

        public override void InitializeComponent(ICore core)
        {
            this.PlaylistManager = core.Managers.Playlist;
            this.PlaylistManager.SelectedPlaylistChanged += this.OnSelectedPlaylistChanged;
            this.PlaylistBrowser = core.Components.PlaylistBrowser;
            this.SignalEmitter = core.Components.SignalEmitter;
            this.SignalEmitter.Signal += this.OnSignal;
            //TODO: Bad .Wait().
            this.Reload().Wait();
            base.InitializeComponent(core);
        }

        protected virtual Task OnSignal(object sender, ISignal signal)
        {
            switch (signal.Name)
            {
                case CommonSignals.PlaylistUpdated:
                    var playlists = signal.State as IEnumerable<Playlist>;
                    if (playlists != null && playlists.Any())
                    {
                        return this.Reload(playlists);
                    }
                    else
                    {
                        return this.Reload();
                    }
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        protected virtual void OnSelectedPlaylistChanged(object sender, EventArgs e)
        {
            var task = Windows.Invoke(new Action(this.OnSelectedItemChanged));
        }

        public ICommand AddPlaylistCommand
        {
            get
            {
                return CommandFactory.Instance.CreateCommand(this.AddPlaylist);
            }
        }

        public Task AddPlaylist()
        {
            return PlaylistsActionsBehaviour.Instance.AddPlaylist();
        }

        public Task AddPlaylist(LibraryHierarchyNode libraryHierarchyNode)
        {
            return PlaylistsActionsBehaviour.Instance.AddPlaylist(libraryHierarchyNode);
        }

        public Task AddPlaylist(IEnumerable<PlaylistItem> playlistItems)
        {
            return PlaylistsActionsBehaviour.Instance.AddPlaylist(playlistItems);
        }

        public Task AddPlaylist(IEnumerable<string> paths)
        {
            return PlaylistsActionsBehaviour.Instance.AddPlaylist(paths);
        }

        public ICommand RemovePlaylistCommand
        {
            get
            {
                return CommandFactory.Instance.CreateCommand(this.RemovePlaylist);
            }
        }

        public Task RemovePlaylist()
        {
            return PlaylistsActionsBehaviour.Instance.RemovePlaylist();
        }

        public ICommand DragEnterCommand
        {
            get
            {
                return new Command<DragEventArgs>(this.OnDragEnter);
            }
        }

        protected virtual void OnDragEnter(DragEventArgs e)
        {
            this.UpdateDragDropEffects(e);
        }

        public ICommand DragOverCommand
        {
            get
            {
                return new Command<DragEventArgs>(this.OnDragOver);
            }
        }

        protected virtual void OnDragOver(DragEventArgs e)
        {
            this.UpdateDragDropEffects(e);
        }

        protected virtual void UpdateDragDropEffects(DragEventArgs e)
        {
            var effects = DragDropEffects.None;
            try
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    effects = DragDropEffects.Copy;
                }
                if (e.Data.GetDataPresent(typeof(LibraryHierarchyNode)))
                {
                    effects = DragDropEffects.Copy;
                }
                if (e.Data.GetDataPresent<IEnumerable<PlaylistItem>>())
                {
                    effects = DragDropEffects.Copy;
                }
                if (ShellIDListHelper.GetDataPresent(e.Data))
                {
                    effects = DragDropEffects.Copy;
                }
            }
            catch (Exception exception)
            {
                Logger.Write(this, LogLevel.Warn, "Failed to query clipboard contents: {0}", exception.Message);
            }
            e.Effects = effects;
        }

        public ICommand DropCommand
        {
            get
            {
                return CommandFactory.Instance.CreateCommand<DragEventArgs>(
                    new Func<DragEventArgs, Task>(this.OnDrop)
                );
            }
        }

        protected virtual Task OnDrop(DragEventArgs e)
        {
            try
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    var paths = e.Data.GetData(DataFormats.FileDrop) as IEnumerable<string>;
                    return this.AddPlaylist(paths);
                }
                if (e.Data.GetDataPresent(typeof(LibraryHierarchyNode)))
                {
                    var libraryHierarchyNode = e.Data.GetData(typeof(LibraryHierarchyNode)) as LibraryHierarchyNode;
                    return this.AddPlaylist(libraryHierarchyNode);
                }
                if (e.Data.GetDataPresent<IEnumerable<PlaylistItem>>())
                {
                    var playlistItems = e.Data
                        .GetData<IEnumerable<PlaylistItem>>()
                        .OrderBy(playlistItem => playlistItem.Sequence);
                    return this.AddPlaylist(playlistItems);
                }
                if (ShellIDListHelper.GetDataPresent(e.Data))
                {
                    var paths = ShellIDListHelper.GetData(e.Data);
                    return this.AddPlaylist(paths);
                }
            }
            catch (Exception exception)
            {
                Logger.Write(this, LogLevel.Warn, "Failed to process clipboard contents: {0}", exception.Message);
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        protected override Freezable CreateInstanceCore()
        {
            return new Playlists();
        }

        protected override void OnDisposing()
        {
            if (this.PlaylistManager != null)
            {
                this.PlaylistManager.SelectedPlaylistChanged -= this.OnSelectedPlaylistChanged;
            }
            if (this.SignalEmitter != null)
            {
                this.SignalEmitter.Signal -= this.OnSignal;
            }
            base.OnDisposing();
        }
    }
}
