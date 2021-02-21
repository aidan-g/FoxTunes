#pragma warning disable 612, 618
using FoxDb;
using FoxDb.Interfaces;
using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes
{
    public abstract class PlaylistTaskBase : BackgroundTask
    {
        public const string ID = "4403475F-D67C-4ED8-BF1F-68D22F28066F";

        protected PlaylistTaskBase(Playlist playlist, int sequence = 0)
            : base(ID)
        {
            this.Playlist = playlist;
            this.Sequence = sequence;
            this.Warnings = new Dictionary<PlaylistItem, IList<string>>();
        }

        public Playlist Playlist { get; private set; }

        public int Sequence { get; protected set; }

        public int Offset { get; protected set; }

        public IDictionary<PlaylistItem, IList<string>> Warnings { get; private set; }

        public ICore Core { get; private set; }

        public IDatabaseComponent Database { get; private set; }

        public IPlaybackManager PlaybackManager { get; private set; }

        public IMetaDataSourceFactory MetaDataSourceFactory { get; private set; }

        public ISignalEmitter SignalEmitter { get; private set; }

        public IScriptingRuntime ScriptingRuntime { get; private set; }

        public IMetaDataBrowser MetaDataBrowser { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public TextConfigurationElement Sort { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Core = core;
            this.Database = core.Factories.Database.Create();
            this.PlaybackManager = core.Managers.Playback;
            this.MetaDataSourceFactory = core.Factories.MetaDataSource;
            this.SignalEmitter = core.Components.SignalEmitter;
            this.ScriptingRuntime = core.Components.ScriptingRuntime;
            this.MetaDataBrowser = core.Components.MetaDataBrowser;
            this.Configuration = core.Components.Configuration;
            this.Sort = this.Configuration.GetElement<TextConfigurationElement>(
                PlaylistBehaviourConfiguration.SECTION,
                PlaylistBehaviourConfiguration.PRE_SORT_ORDER_ELEMENT
            );
            base.InitializeComponent(core);
        }

        protected virtual async Task AddPlaylist()
        {
            using (var task = new SingletonReentrantTask(this, ComponentSlots.Database, SingletonReentrantTask.PRIORITY_HIGH, async cancellationToken =>
            {
                using (var transaction = this.Database.BeginTransaction(this.Database.PreferredIsolationLevel))
                {
                    var set = this.Database.Set<Playlist>(transaction);
                    this.Playlist.Sequence = set.Count;
                    await set.AddAsync(this.Playlist).ConfigureAwait(false);
                    if (transaction.HasTransaction)
                    {
                        transaction.Commit();
                    }
                }
            }))
            {
                await task.Run().ConfigureAwait(false);
            }
        }

        protected virtual async Task RemovePlaylist()
        {
            using (var task = new SingletonReentrantTask(this, ComponentSlots.Database, SingletonReentrantTask.PRIORITY_HIGH, async cancellationToken =>
            {
                using (var transaction = this.Database.BeginTransaction(this.Database.PreferredIsolationLevel))
                {
                    var set = this.Database.Set<Playlist>(transaction);
                    await set.RemoveAsync(this.Playlist).ConfigureAwait(false);
                    if (transaction.HasTransaction)
                    {
                        transaction.Commit();
                    }
                }
            }))
            {
                await task.Run().ConfigureAwait(false);
            }
        }

        protected virtual async Task AddPaths(IEnumerable<string> paths)
        {
            using (var task = new SingletonReentrantTask(this, ComponentSlots.Database, SingletonReentrantTask.PRIORITY_HIGH, async cancellationToken =>
             {
                 await this.AddPlaylistItems(paths, cancellationToken).ConfigureAwait(false);
                 await this.ShiftItems(QueryOperator.GreaterOrEqual, this.Sequence, this.Offset).ConfigureAwait(false);
                 await this.SignalEmitter.Send(new Signal(this, CommonSignals.PlaylistUpdated, new[] { this.Playlist })).ConfigureAwait(false);
                 if (this.MetaDataSourceFactory.Enabled)
                 {
                     await this.AddOrUpdateMetaData(cancellationToken).ConfigureAwait(false);
                 }
                 await this.SequenceItems().ConfigureAwait(false);
                 await this.SetPlaylistItemsStatus(PlaylistItemStatus.None).ConfigureAwait(false);
             }))
            {
                await task.Run().ConfigureAwait(false);
            }
        }

        protected virtual async Task AddPlaylistItems(IEnumerable<string> paths, CancellationToken cancellationToken)
        {
            using (var transaction = this.Database.BeginTransaction(this.Database.PreferredIsolationLevel))
            {
                using (var playlistPopulator = new PlaylistPopulator(this.Database, this.PlaybackManager, this.Sequence, this.Offset, this.Visible, transaction))
                {
                    playlistPopulator.InitializeComponent(this.Core);
                    await this.WithSubTask(playlistPopulator,
                        () => playlistPopulator.Populate(this.Playlist, paths, cancellationToken)
                    ).ConfigureAwait(false);
                    this.Offset = playlistPopulator.Offset;
                }
                transaction.Commit();
            }
        }

        protected virtual async Task AddPlaylistItems(LibraryHierarchyNode libraryHierarchyNode, string filter)
        {
            using (var task = new SingletonReentrantTask(this, ComponentSlots.Database, SingletonReentrantTask.PRIORITY_HIGH, async cancellationToken =>
            {
                using (var transaction = this.Database.BeginTransaction(this.Database.PreferredIsolationLevel))
                {
                    this.Offset = await this.Database.ExecuteScalarAsync<int>(this.Database.Queries.AddLibraryHierarchyNodeToPlaylist(filter, this.Sort.Value), (parameters, phase) =>
                    {
                        switch (phase)
                        {
                            case DatabaseParameterPhase.Fetch:
                                parameters["playlistId"] = this.Playlist.Id;
                                parameters["libraryHierarchyId"] = libraryHierarchyNode.LibraryHierarchyId;
                                parameters["libraryHierarchyItemId"] = libraryHierarchyNode.Id;
                                parameters["sequence"] = this.Sequence;
                                parameters["status"] = PlaylistItemStatus.Import;
                                break;
                        }
                    }, transaction).ConfigureAwait(false);
                    transaction.Commit();
                }
            }))
            {
                await task.Run().ConfigureAwait(false);
            }
        }

        protected virtual async Task AddPlaylistItems(IEnumerable<PlaylistItem> playlistItems)
        {
            using (var task = new SingletonReentrantTask(this, ComponentSlots.Database, SingletonReentrantTask.PRIORITY_HIGH, async cancellationToken =>
            {
                using (var transaction = this.Database.BeginTransaction(this.Database.PreferredIsolationLevel))
                {
                    var set = this.Database.Set<PlaylistItem>(transaction);
                    var position = 0;
                    foreach (var playlistItem in PlaylistItem.Clone(playlistItems))
                    {
                        Logger.Write(this, LogLevel.Debug, "Adding file to playlist: {0}", playlistItem.FileName);
                        playlistItem.Playlist_Id = this.Playlist.Id;
                        playlistItem.Sequence = this.Sequence + position;
                        playlistItem.Status = PlaylistItemStatus.Import;
                        await set.AddAsync(playlistItem).ConfigureAwait(false);
                        position++;
                    }
                    this.Offset += position;
                    if (transaction.HasTransaction)
                    {
                        transaction.Commit();
                    }
                }
            }))
            {
                await task.Run().ConfigureAwait(false);
            }
        }

        protected virtual async Task AddOrUpdateMetaData(CancellationToken cancellationToken)
        {
            using (var transaction = this.Database.BeginTransaction(this.Database.PreferredIsolationLevel))
            {
                using (var metaDataPopulator = new PlaylistMetaDataPopulator(this.Database, this.Visible, transaction))
                {
                    metaDataPopulator.InitializeComponent(this.Core);
                    await this.WithSubTask(metaDataPopulator,
                        () => metaDataPopulator.Populate(PlaylistItemStatus.Import, cancellationToken)
                    ).ConfigureAwait(false);
                    foreach (var pair in metaDataPopulator.Warnings)
                    {
                        if (pair.Key is PlaylistItem playlistItem)
                        {
                            this.Warnings.GetOrAdd(playlistItem, _playlistItem => new List<string>()).AddRange(pair.Value);
                        }
                    }
                }
                transaction.Commit();
            }
        }

        protected virtual async Task MoveItems(IEnumerable<PlaylistItem> playlistItems)
        {
            using (var task = new SingletonReentrantTask(this, ComponentSlots.Database, SingletonReentrantTask.PRIORITY_HIGH, async cancellationToken =>
            {
                using (var transaction = this.Database.BeginTransaction(this.Database.PreferredIsolationLevel))
                {
                    foreach (var playlistItem in playlistItems)
                    {
                        playlistItem.Sequence = this.Sequence;
                        await this.Database.ExecuteAsync(this.Database.Queries.MovePlaylistItem, (parameters, phase) =>
                        {
                            switch (phase)
                            {
                                case DatabaseParameterPhase.Fetch:
                                    parameters["id"] = playlistItem.Id;
                                    parameters["sequence"] = playlistItem.Sequence;
                                    break;
                            }
                        }, transaction).ConfigureAwait(false);
                        if (playlistItem.Sequence > this.Sequence)
                        {
                            //TODO: If the current sequence is less than the target sequence we don't have to increment the counter.
                            //TODO: I don't really understand why.
                            this.Sequence++;
                        }
                    }
                    transaction.Commit();
                }
            }))
            {
                await task.Run().ConfigureAwait(false);
            }
        }

        protected virtual async Task RemoveItems(IEnumerable<PlaylistItem> playlistItems)
        {
            using (var task = new SingletonReentrantTask(this, ComponentSlots.Database, SingletonReentrantTask.PRIORITY_HIGH, async cancellationToken =>
            {
                using (var transaction = this.Database.BeginTransaction(this.Database.PreferredIsolationLevel))
                {
                    foreach (var playlistItem in playlistItems)
                    {
                        playlistItem.Status = PlaylistItemStatus.Remove;
                    }
                    var set = this.Database.Set<PlaylistItem>(transaction);
                    await set.AddOrUpdateAsync(playlistItems).ConfigureAwait(false);
                    if (transaction.HasTransaction)
                    {
                        transaction.Commit();
                    }
                }
            }))
            {
                await task.Run().ConfigureAwait(false);
            }
            await this.RemoveItems(PlaylistItemStatus.Remove).ConfigureAwait(false);
        }

        protected virtual async Task RemoveItems(PlaylistItemStatus status)
        {
            Logger.Write(this, LogLevel.Debug, "Removing playlist items.");
            using (var task = new SingletonReentrantTask(this, ComponentSlots.Database, SingletonReentrantTask.PRIORITY_HIGH, async cancellationToken =>
            {
                using (var transaction = this.Database.BeginTransaction(this.Database.PreferredIsolationLevel))
                {
                    await RemovePlaylistItems(this.Database, this.Playlist.Id, status, transaction).ConfigureAwait(false);
                    transaction.Commit();
                }
            }))
            {
                await task.Run().ConfigureAwait(false);
            }
        }

        protected virtual async Task ShiftItems(QueryOperator @operator, int at, int by)
        {
            if (by == 0)
            {
                //Nothing to do.
                return;
            }
            Logger.Write(
                this,
                LogLevel.Debug,
                "Shifting playlist items at position {0} {1} by offset {2}.",
                Enum.GetName(typeof(QueryOperator), @operator),
                at,
                by
            );
            var query = this.Database.QueryFactory.Build();
            var playlistId = this.Database.Tables.PlaylistItem.Column("Playlist_Id");
            var sequence = this.Database.Tables.PlaylistItem.Column("Sequence");
            var status = this.Database.Tables.PlaylistItem.Column("Status");
            query.Update.SetTable(this.Database.Tables.PlaylistItem);
            query.Update.AddColumn(sequence).Right = query.Update.Fragment<IBinaryExpressionBuilder>().With(expression =>
            {
                expression.Left = expression.CreateColumn(sequence);
                expression.Operator = expression.CreateOperator(QueryOperator.Plus);
                expression.Right = expression.CreateParameter("offset", DbType.Int32, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None);
            });
            query.Filter.AddColumn(playlistId);
            query.Filter.AddColumn(status);
            query.Filter.AddColumn(sequence).With(expression =>
            {
                expression.Operator = expression.CreateOperator(@operator);
                expression.Right = expression.CreateParameter("sequence", DbType.Int32, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None);
            });
            using (var transaction = this.Database.BeginTransaction(this.Database.PreferredIsolationLevel))
            {
                await this.Database.ExecuteAsync(query, (parameters, phase) =>
                {
                    switch (phase)
                    {
                        case DatabaseParameterPhase.Fetch:
                            parameters["playlistId"] = this.Playlist.Id;
                            parameters["status"] = PlaylistItemStatus.None;
                            parameters["sequence"] = at;
                            parameters["offset"] = by;
                            break;
                    }
                }, transaction).ConfigureAwait(false);
                transaction.Commit();
            }
        }

        protected virtual async Task SequenceItems()
        {
            Logger.Write(this, LogLevel.Debug, "Sequencing playlist items.");
            using (var transaction = this.Database.BeginTransaction(this.Database.PreferredIsolationLevel))
            {
                var query = this.Database.Queries.SequencePlaylistItems(this.Sort.Value);
                await this.Database.ExecuteAsync(query, (parameters, phase) =>
                {
                    switch (phase)
                    {
                        case DatabaseParameterPhase.Fetch:
                            parameters["playlistId"] = this.Playlist.Id;
                            parameters["status"] = PlaylistItemStatus.Import;
                            break;
                    }
                }, transaction).ConfigureAwait(false);
                transaction.Commit();
            }
        }

        protected virtual Task SortItems(PlaylistColumn playlistColumn, bool descending)
        {
            Logger.Write(this, LogLevel.Debug, "Sorting playlist {0} by column {1}.", this.Playlist.Name, playlistColumn.Name);
            switch (playlistColumn.Type)
            {
                case PlaylistColumnType.Tag:
                    return this.SortItemsByMetaData(playlistColumn.Tag, descending);
                case PlaylistColumnType.Script:
                    return this.SortItemsByScript(playlistColumn.Script, descending);
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        protected virtual Task SortItemsByMetaData(string tag, bool descending)
        {
            var comparer = new PlaylistItemMetaDataComparer(tag);
            comparer.InitializeComponent(this.Core);
            return this.SortItems(comparer, descending);
        }

        protected virtual async Task SortItemsByScript(string script, bool descending)
        {
            using (var comparer = new PlaylistItemScriptComparer(script))
            {
                comparer.InitializeComponent(this.Core);
                await this.SortItems(comparer, descending).ConfigureAwait(false);
            }
        }

        protected virtual async Task SortItems(IComparer<PlaylistItem> comparer, bool descending)
        {
            Logger.Write(this, LogLevel.Debug, "Sorting playlist using comparer: \"{0}\"", comparer.GetType().Name);
            using (var transaction = this.Database.BeginTransaction(this.Database.PreferredIsolationLevel))
            {
                var set = this.Database.Set<PlaylistItem>(transaction);
                var playlistItems = set.ToArray();
                Array.Sort(playlistItems, comparer);
                if (descending)
                {
                    Logger.Write(this, LogLevel.Debug, "Sort is descending, reversing sequence.");
                    Array.Reverse(playlistItems);
                }
                for (var a = 0; a < playlistItems.Length; a++)
                {
                    playlistItems[a].Sequence = a;
                }
                await EntityHelper<PlaylistItem>.Create(
                    this.Database,
                    this.Database.Tables.PlaylistItem,
                    transaction
                ).UpdateAsync(
                    playlistItems,
                    new[] { nameof(PlaylistItem.Sequence) }
                ).ConfigureAwait(false);
                if (transaction.HasTransaction)
                {
                    transaction.Commit();
                }
            }
        }

        protected virtual async Task SetPlaylistItemsStatus(PlaylistItemStatus status)
        {
            Logger.Write(this, LogLevel.Debug, "Setting playlist status: {0}", Enum.GetName(typeof(LibraryItemStatus), LibraryItemStatus.None));
            var query = this.Database.QueryFactory.Build();
            query.Update.SetTable(this.Database.Tables.PlaylistItem);
            query.Update.AddColumn(this.Database.Tables.PlaylistItem.Column("Status"));
            query.Filter.AddColumn(this.Database.Tables.PlaylistItem.Column("Playlist_Id"));
            using (var transaction = this.Database.BeginTransaction(this.Database.PreferredIsolationLevel))
            {
                await this.Database.ExecuteAsync(query, (parameters, phase) =>
                {
                    switch (phase)
                    {
                        case DatabaseParameterPhase.Fetch:
                            parameters["playlistId"] = this.Playlist.Id;
                            parameters["status"] = status;
                            break;
                    }
                }, transaction).ConfigureAwait(false);
                transaction.Commit();
            }
        }

        public static IEnumerable<string> UpdateLibraryCache(ILibraryCache libraryCache, IEnumerable<PlaylistItem> playlistItems, IEnumerable<string> names)
        {
            var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var playlistItem in playlistItems)
            {
                if (!playlistItem.LibraryItem_Id.HasValue)
                {
                    continue;
                }
                var libraryItem = default(LibraryItem);
                if (libraryCache.TryGetItem(playlistItem.LibraryItem_Id.Value, out libraryItem))
                {
                    result.AddRange(MetaDataItem.Update(playlistItem, libraryItem, names));
                }
            }
            return result;
        }

        public static IEnumerable<string> UpdatePlaylistCache(IPlaylistCache playlistCache, IEnumerable<PlaylistItem> playlistItems, IEnumerable<string> names)
        {
            var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var playlistItem in playlistItems)
            {
                var cachedPlaylistItem = default(PlaylistItem);
                if (playlistCache.TryGetItemById(playlistItem.Id, out cachedPlaylistItem))
                {
                    if (!object.ReferenceEquals(playlistItem, cachedPlaylistItem))
                    {
                        result.AddRange(MetaDataItem.Update(playlistItem, cachedPlaylistItem, names));
                    }
                }
            }
            return result;
        }

        public static async Task UpdatePlaylistItem(IDatabaseComponent database, PlaylistItem playlistItem)
        {
            using (var transaction = database.BeginTransaction(database.PreferredIsolationLevel))
            {
                await UpdatePlaylistItem(database, playlistItem, transaction).ConfigureAwait(false);
                if (transaction.HasTransaction)
                {
                    transaction.Commit();
                }
            }
        }

        public static Task UpdatePlaylistItem(IDatabaseComponent database, PlaylistItem playlistItem, ITransactionSource transaction)
        {
            var table = database.Tables.PlaylistItem;
            var builder = database.QueryFactory.Build();
            builder.Update.SetTable(table);
            builder.Update.AddColumns(table.UpdatableColumns);
            builder.Filter.AddColumns(table.PrimaryKeys);
            var query = builder.Build();
            var parameters = new ParameterHandlerStrategy(table, playlistItem).Handler;
            return database.ExecuteAsync(query, parameters, transaction);
        }

        public static async Task UpdatePlaylistItem(IDatabaseComponent database, int playlistItemId, Action<PlaylistItem> action)
        {
            using (var transaction = database.BeginTransaction(database.PreferredIsolationLevel))
            {
                await UpdatePlaylistItem(database, playlistItemId, action, transaction).ConfigureAwait(false);
                if (transaction.HasTransaction)
                {
                    transaction.Commit();
                }
            }
        }

        public static async Task UpdatePlaylistItem(IDatabaseComponent database, int playlistItemId, Action<PlaylistItem> action, ITransactionSource transaction)
        {
            var table = database.Tables.PlaylistItem;
            var builder = database.QueryFactory.Build();
            builder.Output.AddColumns(table.Columns);
            builder.Source.AddTable(table);
            builder.Filter.AddColumns(table.PrimaryKeys);
            var query = builder.Build();
            var playlistItem = default(PlaylistItem);
            using (var sequence = database.ExecuteAsyncEnumerator<PlaylistItem>(query, (parameters, phase) =>
            {
                switch (phase)
                {
                    case DatabaseParameterPhase.Fetch:
                        parameters[table.PrimaryKey] = playlistItemId;
                        break;
                }
            }, transaction))
            {
                if (await sequence.MoveNextAsync().ConfigureAwait(false))
                {
                    playlistItem = sequence.Current;
                }
            }
            action(playlistItem);
            await UpdatePlaylistItem(database, playlistItem, transaction).ConfigureAwait(false);
        }

        public static Task RemovePlaylistItems(IDatabaseComponent database, int playlistId, PlaylistItemStatus status, ITransactionSource transaction)
        {
            return database.ExecuteAsync(database.Queries.RemovePlaylistItems, (parameters, phase) =>
            {
                switch (phase)
                {
                    case DatabaseParameterPhase.Fetch:
                        parameters["playlistId"] = playlistId;
                        parameters["status"] = status;
                        break;
                }
            }, transaction);
        }

        protected override void OnDisposing()
        {
            if (this.Database != null)
            {
                this.Database.Dispose();
            }
            base.OnDisposing();
        }
    }
}
