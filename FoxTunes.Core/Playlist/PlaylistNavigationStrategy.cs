using FoxDb;
using FoxDb.Interfaces;
using FoxTunes.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FoxTunes
{
    public abstract class PlaylistNavigationStrategy : BaseComponent
    {
        public PlaylistQueue PlaylistQueue { get; private set; }

        public IPlaylistBrowser PlaylistBrowser { get; private set; }

        public IPlaylistManager PlaylistManager { get; private set; }

        public IDatabaseFactory DatabaseFactory { get; private set; }

        protected virtual Playlist GetPlaylist(PlaylistItem playlistItem)
        {
            if (playlistItem == null)
            {
                return this.PlaylistManager.CurrentPlaylist ?? this.PlaylistManager.SelectedPlaylist;
            }
            return this.PlaylistBrowser.GetPlaylist(playlistItem) ?? this.PlaylistManager.SelectedPlaylist;
        }

        public PlaylistItem GetNext(PlaylistItem playlistItem)
        {
            var playlist = this.GetPlaylist(playlistItem);
            if (playlist == null)
            {
                return null;
            }
            return
                this.PlaylistQueue.GetNext(playlistItem) ??
                this.GetNext(playlist, playlistItem);
        }

        protected abstract PlaylistItem GetNext(Playlist playlist, PlaylistItem playlistItem);

        public PlaylistItem GetPrevious(PlaylistItem playlistItem)
        {
            var playlist = this.GetPlaylist(playlistItem);
            if (playlist == null)
            {
                return null;
            }
            return
                this.GetPrevious(playlist, playlistItem);
        }

        protected abstract PlaylistItem GetPrevious(Playlist playlist, PlaylistItem playlistItem);

        public override void InitializeComponent(ICore core)
        {
            this.PlaylistQueue = ComponentRegistry.Instance.GetComponent<PlaylistQueue>();
            this.PlaylistBrowser = core.Components.PlaylistBrowser;
            this.PlaylistManager = core.Managers.Playlist;
            this.DatabaseFactory = core.Factories.Database;
            base.InitializeComponent(core);
        }
    }

    public class StandardPlaylistNavigationStrategy : PlaylistNavigationStrategy
    {
        public StandardPlaylistNavigationStrategy()
        {
            this.SequenceQueries = new ConcurrentDictionary<Tuple<QueryOperator, OrderByDirection>, IDatabaseQuery>();
        }

        public ConcurrentDictionary<Tuple<QueryOperator, OrderByDirection>, IDatabaseQuery> SequenceQueries { get; private set; }

        protected override PlaylistItem GetNext(Playlist playlist, PlaylistItem playlistItem)
        {
            if (playlistItem == null)
            {
                return this.PlaylistBrowser.GetFirstItem(playlist);
            }
            var sequence = default(int);
            if (this.TryGetNextSequence(playlist, playlistItem.Sequence, out sequence))
            {
                playlistItem = this.PlaylistBrowser.GetItemBySequence(playlist, sequence);
                if (playlistItem == null)
                {
                    playlistItem = this.PlaylistBrowser.GetFirstItem(playlist);
                }
            }
            else
            {
                playlistItem = this.PlaylistBrowser.GetFirstItem(playlist);
            }
            return playlistItem;
        }

        protected override PlaylistItem GetPrevious(Playlist playlist, PlaylistItem playlistItem)
        {
            if (playlistItem == null)
            {
                return this.PlaylistBrowser.GetLastItem(playlist);
            }
            var sequence = default(int);
            if (this.TryGetPreviousSequence(playlist, playlistItem.Sequence, out sequence))
            {
                playlistItem = this.PlaylistBrowser.GetItemBySequence(playlist, sequence);
                if (playlistItem == null)
                {
                    return this.PlaylistBrowser.GetLastItem(playlist);
                }
            }
            else
            {
                playlistItem = this.PlaylistBrowser.GetLastItem(playlist);
            }
            return playlistItem;
        }

        protected virtual bool TryGetNextSequence(Playlist playlist, int currentSequence, out int nextSequence)
        {
            var sequence = this.GetSequence(playlist, currentSequence, QueryOperator.Greater, OrderByDirection.Ascending);
            if (sequence.HasValue)
            {
                nextSequence = sequence.Value;
                return true;
            }
            nextSequence = default(int);
            return false;
        }

        protected virtual bool TryGetPreviousSequence(Playlist playlist, int currentSequence, out int nextSequence)
        {
            var sequence = this.GetSequence(playlist, currentSequence, QueryOperator.Less, OrderByDirection.Descending);
            if (sequence.HasValue)
            {
                nextSequence = sequence.Value;
                return true;
            }
            nextSequence = default(int);
            return false;
        }

        protected virtual int? GetSequence(Playlist playlist, int sequence, QueryOperator @operator, OrderByDirection direction)
        {
            using (var database = this.DatabaseFactory.Create())
            {
                using (var transaction = database.BeginTransaction(database.PreferredIsolationLevel))
                {
                    var query = this.SequenceQueries.GetOrAdd(
                        new Tuple<QueryOperator, OrderByDirection>(@operator, direction),
                        key =>
                        {
                            var table = database.Tables.PlaylistItem;
                            var column = table.GetColumn(ColumnConfig.By("Sequence"));
                            var builder = database.QueryFactory.Build();
                            builder.Output.AddColumn(column);
                            builder.Source.AddTable(table);
                            builder.Filter.AddColumn(table.GetColumn(ColumnConfig.By("Playlist_Id")));
                            builder.Filter.AddColumn(column).Operator = builder.Filter.CreateOperator(@operator);
                            builder.Sort.AddColumn(column).Direction = direction;
                            return builder.Build();
                        }
                    );
                    return database.ExecuteScalar<int?>(query, (parameters, phase) =>
                    {
                        switch (phase)
                        {
                            case DatabaseParameterPhase.Fetch:
                                parameters["playlistId"] = playlist.Id;
                                parameters["sequence"] = sequence;
                                break;
                        }
                    }, transaction);
                }
            }
        }
    }

    public class ShufflePlaylistNavigationStrategy : PlaylistNavigationStrategy
    {
        public ShufflePlaylistNavigationStrategy()
        {
            this.Semaphore = new SemaphoreSlim(1, 1);
            this.Sequences = new List<int>();
        }

        public ShufflePlaylistNavigationStrategy(Func<PlaylistItem, string> selector) : this()
        {
            this.Selector = selector;
        }

        public SemaphoreSlim Semaphore { get; private set; }

        public IList<int> Sequences { get; private set; }

        public Func<PlaylistItem, string> Selector { get; private set; }

        public Playlist Playlist { get; private set; }

        public ISignalEmitter SignalEmitter { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            base.InitializeComponent(core);
            this.SignalEmitter = core.Components.SignalEmitter;
            this.SignalEmitter.Signal += this.OnSignal;
        }

        private Task OnSignal(object sender, ISignal signal)
        {
            switch (signal.Name)
            {
                case CommonSignals.PlaylistUpdated:
                    if (this.Playlist != null)
                    {
                        var playlists = signal.State as IEnumerable<Playlist>;
                        if (playlists == null || !playlists.Any() || playlists.Contains(this.Playlist))
                        {
                            return this.Refresh();
                        }
                    }
                    break;
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        public Task Refresh()
        {
            if (this.Playlist != null)
            {
                this.Refresh(this.Playlist);
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        protected virtual void Refresh(Playlist playlist)
        {
            //TODO: We should use WaitAsync for >NET40.
            this.Semaphore.Wait();
            try
            {
                this.Playlist = playlist;
                this.Sequences.Clear();
                if (this.Selector == null)
                {
                    this.Sequences.AddRange(
                        this.PlaylistBrowser.GetItems(playlist).Select(
                            playlistItem => playlistItem.Sequence
                        )
                    );
                    this.Sequences.Shuffle();
                }
                else
                {
                    var groups = this.PlaylistBrowser.GetItems(playlist).GroupBy(
                        playlistItem => this.Selector(playlistItem)
                    );
                    foreach (var group in groups)
                    {
                        var sequences = group.Select(
                            playlistItem => playlistItem.Sequence
                        ).ToList();
                        sequences.Shuffle();
                        this.Sequences.AddRange(sequences);
                    }
                }
            }
            finally
            {
                this.Semaphore.Release();
            }
        }

        protected override PlaylistItem GetNext(Playlist playlist, PlaylistItem playlistItem)
        {
            if (this.Playlist == null || this.Playlist != playlist)
            {
                this.Refresh(playlist);
            }
            //TODO: We should use WaitAsync for >NET40.
            this.Semaphore.Wait();
            try
            {
                if (this.Sequences.Count == 0)
                {
                    return default(PlaylistItem);
                }
                var position = default(int);
                if (playlistItem != null)
                {
                    position = this.Sequences.IndexOf(playlistItem.Sequence);
                    if (position >= this.Sequences.Count - 1)
                    {
                        position = 0;
                    }
                    else
                    {
                        position++;
                    }
                }
                else
                {
                    position = 0;
                }
                var sequence = this.Sequences[position];
                return this.PlaylistBrowser.GetItemBySequence(playlist, sequence);
            }
            finally
            {
                this.Semaphore.Release();
            }
        }

        protected override PlaylistItem GetPrevious(Playlist playlist, PlaylistItem playlistItem)
        {
            if (this.Playlist == null || this.Playlist != playlist)
            {
                this.Refresh(playlist);
            }
            //TODO: We should use WaitAsync for >NET40.
            this.Semaphore.Wait();
            try
            {
                if (this.Sequences.Count == 0)
                {
                    return default(PlaylistItem);
                }
                var position = default(int);
                if (playlistItem != null)
                {
                    position = this.Sequences.IndexOf(playlistItem.Sequence);
                    if (position == 0)
                    {
                        position = this.Sequences.Count - 1;
                    }
                    else
                    {
                        position--;
                    }
                }
                else
                {
                    position = this.Sequences.Count - 1;
                }
                var sequence = this.Sequences[position];
                return this.PlaylistBrowser.GetItemBySequence(playlist, sequence);
            }
            finally
            {
                this.Semaphore.Release();
            }
        }
    }
}
