using FoxDb.Interfaces;
using FoxTunes.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.Database)]
    public class MetaDataBrowser : StandardComponent, IMetaDataBrowser
    {
        public IMetaDataCache MetaDataCache { get; private set; }

        public ILibraryHierarchyBrowser LibraryHierarchyBrowser { get; private set; }

        public IDatabaseFactory DatabaseFactory { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.MetaDataCache = core.Components.MetaDataCache;
            this.LibraryHierarchyBrowser = core.Components.LibraryHierarchyBrowser;
            this.DatabaseFactory = core.Factories.Database;
            base.InitializeComponent(core);
        }

        public MetaDataItem[] GetMetaDatas(LibraryHierarchyNode libraryHierarchyNode, MetaDataItemType? metaDataItemType, string metaDataItemName)
        {
            var key = new LibraryMetaDataCacheKey(libraryHierarchyNode, metaDataItemType, metaDataItemName, this.LibraryHierarchyBrowser.Filter);
            return this.MetaDataCache.GetMetaDatas(key, () => this.GetMetaDatasCore(libraryHierarchyNode, metaDataItemType, metaDataItemName));
        }

        public Task<MetaDataItem[]> GetMetaDatasAsync(LibraryHierarchyNode libraryHierarchyNode, MetaDataItemType? metaDataItemType, string metaDataItemName)
        {
            var key = new LibraryMetaDataCacheKey(libraryHierarchyNode, metaDataItemType, metaDataItemName, this.LibraryHierarchyBrowser.Filter);
            return this.MetaDataCache.GetMetaDatas(key, () => this.GetMetaDatasCoreAsync(libraryHierarchyNode, metaDataItemType, metaDataItemName));
        }

        private IEnumerable<MetaDataItem> GetMetaDatasCore(LibraryHierarchyNode libraryHierarchyNode, MetaDataItemType? metaDataItemType, string metaDataItemName)
        {
            using (var database = this.DatabaseFactory.Create())
            {
                using (var transaction = database.BeginTransaction(database.PreferredIsolationLevel))
                {
                    using (var reader = this.GetReader(database, libraryHierarchyNode, metaDataItemType, metaDataItemName, transaction))
                    {
                        foreach (var record in reader)
                        {
                            yield return new MetaDataItem()
                            {
                                Name = metaDataItemName,
                                Type = metaDataItemType.GetValueOrDefault(),
                                Value = record.Get<string>("Value")
                            };
                        }
                    }
                }
            }
        }

        private async Task<IEnumerable<MetaDataItem>> GetMetaDatasCoreAsync(LibraryHierarchyNode libraryHierarchyNode, MetaDataItemType? metaDataItemType, string metaDataItemName)
        {
            using (var database = this.DatabaseFactory.Create())
            {
                using (var transaction = database.BeginTransaction(database.PreferredIsolationLevel))
                {
                    using (var reader = this.GetReader(database, libraryHierarchyNode, metaDataItemType, metaDataItemName, transaction))
                    {
                        using (var sequence = reader.GetAsyncEnumerator())
                        {
                            var result = new List<MetaDataItem>();
                            while (await sequence.MoveNextAsync().ConfigureAwait(false))
                            {
                                result.Add(new MetaDataItem()
                                {
                                    Name = metaDataItemName,
                                    Type = metaDataItemType.GetValueOrDefault(),
                                    Value = sequence.Current.Get<string>("Value")
                                });
                            }
                            return result;
                        }
                    }
                }
            }
        }

        private IDatabaseReader GetReader(IDatabaseComponent database, LibraryHierarchyNode libraryHierarchyNode, MetaDataItemType? metaDataItemType, string metaDataItemName, ITransactionSource transaction)
        {
            return database.ExecuteReader(database.Queries.GetLibraryHierarchyMetaData(this.LibraryHierarchyBrowser.Filter), (parameters, phase) =>
            {
                switch (phase)
                {
                    case DatabaseParameterPhase.Fetch:
                        parameters["libraryHierarchyItemId"] = libraryHierarchyNode.Id;
                        if (metaDataItemType.HasValue)
                        {
                            parameters["type"] = metaDataItemType.Value;
                        }
                        else
                        {
                            parameters["type"] = null;
                        }
                        if (!string.IsNullOrEmpty(metaDataItemName))
                        {
                            parameters["name"] = metaDataItemName;
                        }
                        else
                        {
                            parameters["name"] = null;
                        }
                        break;
                }
            }, transaction);
        }

        public MetaDataItem[] GetMetaDatas(IEnumerable<PlaylistItem> playlistItems, MetaDataItemType? metaDataItemType, string metaDataItemName)
        {
            var key = new PlaylistMetaDataCacheKey(playlistItems.ToArray(), metaDataItemType, metaDataItemName, this.LibraryHierarchyBrowser.Filter);
            return this.MetaDataCache.GetMetaDatas(key, () => this.GetMetaDatasCore(playlistItems, metaDataItemType, metaDataItemName));
        }

        public Task<MetaDataItem[]> GetMetaDatasAsync(IEnumerable<PlaylistItem> playlistItems, MetaDataItemType? metaDataItemType, string metaDataItemName)
        {
            var key = new PlaylistMetaDataCacheKey(playlistItems.ToArray(), metaDataItemType, metaDataItemName, this.LibraryHierarchyBrowser.Filter);
            return this.MetaDataCache.GetMetaDatas(key, () => this.GetMetaDatasCoreAsync(playlistItems, metaDataItemType, metaDataItemName));
        }

        private IEnumerable<MetaDataItem> GetMetaDatasCore(IEnumerable<PlaylistItem> playlistItems, MetaDataItemType? metaDataItemType, string metaDataItemName)
        {
            using (var database = this.DatabaseFactory.Create())
            {
                using (var transaction = database.BeginTransaction(database.PreferredIsolationLevel))
                {
                    using (var reader = this.GetReader(database, playlistItems, metaDataItemType, metaDataItemName, transaction))
                    {
                        foreach (var record in reader)
                        {
                            yield return new MetaDataItem()
                            {
                                Name = metaDataItemName,
                                Type = metaDataItemType.GetValueOrDefault(),
                                Value = record.Get<string>("Value")
                            };
                        }
                    }
                }
            }
        }

        private async Task<IEnumerable<MetaDataItem>> GetMetaDatasCoreAsync(IEnumerable<PlaylistItem> playlistItems, MetaDataItemType? metaDataItemType, string metaDataItemName)
        {
            using (var database = this.DatabaseFactory.Create())
            {
                using (var transaction = database.BeginTransaction(database.PreferredIsolationLevel))
                {
                    using (var reader = this.GetReader(database, playlistItems, metaDataItemType, metaDataItemName, transaction))
                    {
                        using (var sequence = reader.GetAsyncEnumerator())
                        {
                            var result = new List<MetaDataItem>();
                            while (await sequence.MoveNextAsync().ConfigureAwait(false))
                            {
                                result.Add(new MetaDataItem()
                                {
                                    Name = metaDataItemName,
                                    Type = metaDataItemType.GetValueOrDefault(),
                                    Value = sequence.Current.Get<string>("Value")
                                });
                            }
                            return result;
                        }
                    }
                }
            }
        }

        private IDatabaseReader GetReader(IDatabaseComponent database, IEnumerable<PlaylistItem> playlistItems, MetaDataItemType? metaDataItemType, string metaDataItemName, ITransactionSource transaction)
        {
            return database.ExecuteReader(database.Queries.GetPlaylistMetaData(playlistItems.Count()), (parameters, phase) =>
            {
                switch (phase)
                {
                    case DatabaseParameterPhase.Fetch:
                        var position = 0;
                        foreach (var playlistItem in playlistItems)
                        {
                            parameters["playlistItemId" + position] = playlistItem.Id;
                            position++;
                        }
                        if (metaDataItemType.HasValue)
                        {
                            parameters["type"] = metaDataItemType.Value;
                        }
                        else
                        {
                            parameters["type"] = null;
                        }
                        if (!string.IsNullOrEmpty(metaDataItemName))
                        {
                            parameters["name"] = metaDataItemName;
                        }
                        else
                        {
                            parameters["name"] = null;
                        }
                        break;
                }
            }, transaction);
        }
    }
}
