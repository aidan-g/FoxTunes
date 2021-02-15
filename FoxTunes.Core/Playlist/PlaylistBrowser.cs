using FoxDb;
using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.Database)]
    public class PlaylistBrowser : StandardComponent, IPlaylistBrowser
    {
        private PlaylistBrowserState _State { get; set; }

        public PlaylistBrowserState State
        {
            get
            {
                return this._State;
            }
            set
            {
                this._State = value;
                this.OnStateChanged();
            }
        }

        protected virtual void OnStateChanged()
        {
            if (this.StateChanged != null)
            {
                this.StateChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("State");
        }

        public event EventHandler StateChanged;

        public ICore Core { get; private set; }

        public IPlaylistManager PlaylistManager { get; private set; }

        public IPlaylistCache PlaylistCache { get; private set; }

        public IDatabaseFactory DatabaseFactory { get; private set; }

        public PlaylistNavigationStrategyFactory PlaylistNavigationStrategyFactory { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public PlaylistNavigationStrategy NavigationStrategy { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Core = core;
            this.PlaylistManager = core.Managers.Playlist;
            this.PlaylistCache = core.Components.PlaylistCache;
            this.DatabaseFactory = core.Factories.Database;
            this.PlaylistNavigationStrategyFactory = ComponentRegistry.Instance.GetComponent<PlaylistNavigationStrategyFactory>();
            this.Configuration = core.Components.Configuration;
            this.Configuration.GetElement<SelectionConfigurationElement>(
                PlaylistBehaviourConfiguration.SECTION,
                PlaylistBehaviourConfiguration.ORDER_ELEMENT
            ).ConnectValue(option => this.NavigationStrategy = this.PlaylistNavigationStrategyFactory.Create(option.Id));
            base.InitializeComponent(core);
        }

        public PlaylistColumn[] GetColumns()
        {
            return this.PlaylistCache.GetColumns(this.GetColumnsCore);
        }

        private IEnumerable<PlaylistColumn> GetColumnsCore()
        {
            using (var database = this.DatabaseFactory.Create())
            {
                using (var transaction = database.BeginTransaction(database.PreferredIsolationLevel))
                {
                    var set = database.Set<PlaylistColumn>(transaction);
                    //It's easier to just filter enabled/disabled in memory, there isn't much data.
                    //set.Fetch.Filter.AddColumn(
                    //    set.Table.GetColumn(ColumnConfig.By("Enabled", ColumnFlags.None))
                    //).With(filter => filter.Right = filter.CreateConstant(1));
                    set.Fetch.Sort.Expressions.Clear();
                    set.Fetch.Sort.AddColumn(set.Table.GetColumn(ColumnConfig.By("Sequence", ColumnFlags.None)));
                    foreach (var element in set)
                    {
                        yield return element;
                    }
                }
            }
        }

        public Playlist[] GetPlaylists()
        {
            return this.PlaylistCache.GetPlaylists(this.GetPlaylistsCore);
        }

        private IEnumerable<Playlist> GetPlaylistsCore()
        {
            using (var database = this.DatabaseFactory.Create())
            {
                using (var transaction = database.BeginTransaction(database.PreferredIsolationLevel))
                {
                    var set = database.Set<Playlist>(transaction);
                    set.Fetch.Filter.AddColumn(
                        set.Table.GetColumn(ColumnConfig.By("Enabled", ColumnFlags.None))
                    ).With(filter => filter.Right = filter.CreateConstant(1));
                    set.Fetch.Sort.Expressions.Clear();
                    set.Fetch.Sort.AddColumn(set.Table.GetColumn(ColumnConfig.By("Sequence", ColumnFlags.None)));
                    foreach (var element in set)
                    {
                        yield return element;
                    }
                }
            }
        }

        public Playlist GetPlaylist(PlaylistItem playlistItem)
        {
            return this.GetPlaylists().FirstOrDefault(playlist => playlist.Id == playlistItem.Playlist_Id);
        }

        public PlaylistItem[] GetItems(Playlist playlist)
        {
            return this.PlaylistCache.GetItems(playlist, () => this.GetItemsCore(playlist));
        }

        private IEnumerable<PlaylistItem> GetItemsCore(Playlist playlist)
        {
            this.State |= PlaylistBrowserState.Loading;
            try
            {
                using (var database = this.DatabaseFactory.Create())
                {
                    using (var transaction = database.BeginTransaction(database.PreferredIsolationLevel))
                    {
                        var sequence = database.AsQueryable<PlaylistItem>(transaction)
                            .Where(playlistItem => playlistItem.Playlist_Id == playlist.Id)
                            .OrderBy(playlistItem => playlistItem.Sequence);
                        foreach (var element in sequence)
                        {
                            yield return element;
                        }
                    }
                }
            }
            finally
            {
                this.State &= ~PlaylistBrowserState.Loading;
            }
        }

        public PlaylistItem GetItemById(Playlist playlist, int id)
        {
            var playlistItem = default(PlaylistItem);
            if (!this.PlaylistCache.TryGetItemById(id, out playlistItem))
            {
                //TODO: This could potentially be very expensive if somebody keeps calling this routine trying to retrieve items that aren't in the specified playlist.
                this.GetItems(playlist);
                if (!this.PlaylistCache.TryGetItemById(id, out playlistItem))
                {
                    return null;
                }
            }
            return playlistItem;
        }

        public PlaylistItem GetItemBySequence(Playlist playlist, int sequence)
        {
            var playlistItem = default(PlaylistItem);
            if (!this.PlaylistCache.TryGetItemBySequence(playlist, sequence, out playlistItem))
            {
                return null;
            }
            return playlistItem;
        }

        public PlaylistItem GetFirstItem(Playlist playlist)
        {
            return this.GetItems(playlist).FirstOrDefault();
        }

        public PlaylistItem GetLastItem(Playlist playlist)
        {
            return this.GetItems(playlist).LastOrDefault();
        }

        public PlaylistItem GetNextItem(Playlist playlist)
        {
            return this.NavigationStrategy.GetNext(this.PlaylistManager.CurrentItem);
        }

        public PlaylistItem GetNextItem(PlaylistItem playlistItem)
        {
            return this.NavigationStrategy.GetNext(playlistItem);
        }

        public PlaylistItem GetPreviousItem(Playlist playlist)
        {
            return this.NavigationStrategy.GetPrevious(this.PlaylistManager.CurrentItem);
        }

        public PlaylistItem GetPreviousItem(PlaylistItem playlistItem)
        {
            return this.NavigationStrategy.GetPrevious(playlistItem);
        }

        public int GetInsertIndex(Playlist playlist)
        {
            var playlistItem = this.GetLastItem(playlist);
            if (playlistItem == null)
            {
                return 0;
            }
            else
            {
                return playlistItem.Sequence + 1;
            }
        }
    }
}
