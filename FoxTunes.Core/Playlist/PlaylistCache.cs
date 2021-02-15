using FoxTunes.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.Database)]
    //Setting PRIORITY_HIGH so the the cache is cleared before being re-queried.
    [Component("AF71D00F-5D47-4740-BC14-1B3E4513A1A3", ComponentSlots.None, priority: ComponentAttribute.PRIORITY_HIGH)]
    public class PlaylistCache : StandardComponent, IPlaylistCache, IDisposable
    {
        public Lazy<PlaylistColumn[]> Columns { get; private set; }

        public Lazy<Playlist[]> Playlists { get; private set; }

        public ConcurrentDictionary<Playlist, Lazy<IndexedArray<PlaylistItem>>> Items { get; private set; }

        public ConcurrentDictionary<Playlist, Lazy<IndexedArray<PlaylistItem>.Index<int>>> ItemsById { get; private set; }

        public ConcurrentDictionary<Playlist, Lazy<IndexedArray<PlaylistItem>.Index<int>>> ItemsBySequence { get; private set; }

        public ConcurrentDictionary<Playlist, Lazy<IndexedArray<PlaylistItem>.Index<int>>> ItemsByLibraryId { get; private set; }

        public ISignalEmitter SignalEmitter { get; private set; }

        public PlaylistCache()
        {
            this.Reset();
        }

        public override void InitializeComponent(ICore core)
        {
            this.SignalEmitter = core.Components.SignalEmitter;
            this.SignalEmitter.Signal += this.OnSignal;
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
                        foreach (var playlist in playlists)
                        {
                            Logger.Write(this, LogLevel.Debug, "Playlist \"{0}\" was updated, resetting cache.", playlist.Name);
                            this.Reset(playlist);
                        }
                    }
                    else
                    {
                        Logger.Write(this, LogLevel.Debug, "Playlists were updated, resetting cache.");
                        this.Reset();
                    }
                    break;
                case CommonSignals.PlaylistColumnsUpdated:
                    var columns = signal.State as IEnumerable<PlaylistColumn>;
                    if (columns != null && columns.Any())
                    {
                        //Nothing to do for indivudual column change.
                    }
                    else
                    {
                        Logger.Write(this, LogLevel.Debug, "Columns were updated, resetting cache.");
                        this.Columns = null;
                    }
                    break;
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        public PlaylistColumn[] GetColumns(Func<IEnumerable<PlaylistColumn>> factory)
        {
            if (this.Columns == null)
            {
                this.Columns = new Lazy<PlaylistColumn[]>(() => factory().ToArray());
            }
            return this.Columns.Value;
        }

        public Playlist[] GetPlaylists(Func<IEnumerable<Playlist>> factory)
        {
            if (this.Playlists == null)
            {
                this.Playlists = new Lazy<Playlist[]>(() => factory().ToArray());
            }
            return this.Playlists.Value;
        }

        public PlaylistItem[] GetItems(Playlist playlist, Func<IEnumerable<PlaylistItem>> factory)
        {
            return this.Items.GetOrAdd(
                playlist,
                key => new Lazy<IndexedArray<PlaylistItem>>(() => new IndexedArray<PlaylistItem>(factory().ToArray()))
            ).Value.InnerArray;
        }

        public bool TryGetItemById(int id, out PlaylistItem result)
        {
            foreach (var pair in this.Items)
            {
                if (this.ItemsById.GetOrAdd(
                    pair.Key,
                    key => new Lazy<IndexedCollection<PlaylistItem>.Index<int>>(() => pair.Value.Value.By(playlistItem => playlistItem.Id, IndexedCollection.IndexType.Single))
                ).Value.TryFind(id, out result))
                {
                    return true;
                }
            }
            result = default(PlaylistItem);
            return false;
        }

        public bool TryGetItemBySequence(Playlist playlist, int sequence, out PlaylistItem result)
        {
            var playlistItems = default(Lazy<IndexedArray<PlaylistItem>>);
            if (this.Items.TryGetValue(playlist, out playlistItems))
            {
                if (this.ItemsBySequence.GetOrAdd(
                    playlist,
                    key => new Lazy<IndexedCollection<PlaylistItem>.Index<int>>(() => playlistItems.Value.By(playlistItem => playlistItem.Sequence, IndexedCollection.IndexType.Single))
                ).Value.TryFind(sequence, out result))
                {
                    return true;
                }
            }
            result = default(PlaylistItem);
            return false;
        }

        public bool TryGetItemsByLibraryId(int id, out PlaylistItem[] result)
        {
            var list = new List<PlaylistItem>();
            foreach (var pair in this.Items)
            {
                list.AddRange(this.ItemsById.GetOrAdd(
                    pair.Key,
                    key => new Lazy<IndexedCollection<PlaylistItem>.Index<int>>(() => pair.Value.Value.By(playlistItem => playlistItem.LibraryItem_Id.GetValueOrDefault(), IndexedCollection.IndexType.Multiple))
                ).Value.FindAll(id));
            }
            if (!list.Any())
            {
                result = default(PlaylistItem[]);
                return false;
            }
            result = list.ToArray();
            return true;
        }

        public void Reset()
        {
            this.Playlists = null;
            this.Items = new ConcurrentDictionary<Playlist, Lazy<IndexedArray<PlaylistItem>>>();
            this.ItemsById = new ConcurrentDictionary<Playlist, Lazy<IndexedArray<PlaylistItem>.Index<int>>>();
            this.ItemsBySequence = new ConcurrentDictionary<Playlist, Lazy<IndexedArray<PlaylistItem>.Index<int>>>();
            this.ItemsByLibraryId = new ConcurrentDictionary<Playlist, Lazy<IndexedArray<PlaylistItem>.Index<int>>>();
        }

        public void Reset(Playlist playlist)
        {
            this.Playlists = null;
            if (this.Items != null)
            {
                this.Items.TryRemove(playlist);
            }
            if (this.ItemsById != null)
            {
                this.ItemsById.TryRemove(playlist);
            }
            if (this.ItemsBySequence != null)
            {
                this.ItemsBySequence.TryRemove(playlist);
            }
            if (this.ItemsByLibraryId != null)
            {
                this.ItemsByLibraryId.TryRemove(playlist);
            }
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
            if (this.SignalEmitter != null)
            {
                this.SignalEmitter.Signal -= this.OnSignal;
            }
        }

        ~PlaylistCache()
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
