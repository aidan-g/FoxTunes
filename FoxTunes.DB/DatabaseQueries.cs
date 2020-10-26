using FoxDb;
using FoxDb.Interfaces;
using FoxTunes.Interfaces;
using FoxTunes.Templates;
using System.Data;

namespace FoxTunes
{
    public abstract class DatabaseQueries : BaseComponent, IDatabaseQueries
    {
        public DatabaseQueries(IDatabase database)
        {
            this.Database = database;
        }

        public IDatabase Database { get; private set; }

        public IFilterParser FilterParser { get; private set; }

        public ISortParser SortParser { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            if (core != null)
            {
                this.FilterParser = core.Components.FilterParser;
                this.SortParser = core.Components.SortParser;
            }
            base.InitializeComponent(core);
        }

        public IDatabaseQuery AddLibraryHierarchyNode
        {
            get
            {
                return this.Database.QueryFactory.Create(
                    Resources.AddLibraryHierarchyNode,
                    new DatabaseQueryParameter("libraryHierarchyId", DbType.Int32, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None),
                    new DatabaseQueryParameter("libraryItemId", DbType.Int32, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None),
                    new DatabaseQueryParameter("parentId", DbType.Int32, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None),
                    new DatabaseQueryParameter("value", DbType.String, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None),
                    new DatabaseQueryParameter("isLeaf", DbType.Boolean, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None)
                );
            }
        }

        public abstract IDatabaseQuery AddLibraryHierarchyNodeToPlaylist(string filter, string sort);

        public IDatabaseQuery AddLibraryMetaDataItem
        {
            get
            {
                return this.Database.QueryFactory.Create(
                    Resources.AddLibraryMetaDataItem,
                    new DatabaseQueryParameter("itemId", DbType.Int32, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None),
                    new DatabaseQueryParameter("metaDataItemId", DbType.Int32, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None)
                );
            }
        }

        public IDatabaseQuery AddPlaylistMetaDataItem
        {
            get
            {
                return this.Database.QueryFactory.Create(
                    Resources.AddPlaylistMetaDataItem,
                    new DatabaseQueryParameter("itemId", DbType.Int32, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None),
                    new DatabaseQueryParameter("metaDataItemId", DbType.Int32, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None)
                );
            }
        }

        public IDatabaseQuery ClearLibraryMetaDataItems
        {
            get
            {
                return this.Database.QueryFactory.Create(
                    Resources.ClearLibraryMetaDataItems,
                    new DatabaseQueryParameter("itemId", DbType.Int32, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None),
                    new DatabaseQueryParameter("type", DbType.Byte, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None)
                );
            }
        }

        public IDatabaseQuery ClearPlaylistMetaDataItems
        {
            get
            {
                return this.Database.QueryFactory.Create(
                    Resources.ClearPlaylistMetaDataItems,
                    new DatabaseQueryParameter("itemId", DbType.Int32, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None),
                    new DatabaseQueryParameter("type", DbType.Byte, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None)
                );
            }
        }

        public IDatabaseQuery GetLibraryMetaData
        {
            get
            {
                return this.Database.QueryFactory.Create(
                    Resources.GetLibraryMetaData,
                    new DatabaseQueryParameter("libraryItemId", DbType.Int32, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None),
                    new DatabaseQueryParameter("type", DbType.Byte, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None)
                );
            }
        }

        public abstract IDatabaseQuery GetLibraryHierarchyMetaData(string filter);

        public IDatabaseQuery GetLibraryHierarchyNodes(string filter)
        {
            var result = default(IFilterParserResult);
            if (!string.IsNullOrEmpty(filter) && !this.FilterParser.TryParse(filter, out result))
            {
                //TODO: Warn, failed to parse filter.
                result = null;
            }
            var template = new GetLibraryHierarchyNodes(this.Database, result);
            return this.Database.QueryFactory.Create(
                template.TransformText(),
                new DatabaseQueryParameter("libraryHierarchyId", DbType.Int32, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None),
                new DatabaseQueryParameter("libraryHierarchyItemId", DbType.Int32, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None)
            );
        }

        public IDatabaseQuery GetLibraryItems(string filter)
        {
            var result = default(IFilterParserResult);
            if (!string.IsNullOrEmpty(filter) && !this.FilterParser.TryParse(filter, out result))
            {
                //TODO: Warn, failed to parse filter.
                result = null;
            }
            var template = new GetLibraryItems(this.Database, result);
            return this.Database.QueryFactory.Create(
                template.TransformText(),
                new DatabaseQueryParameter("libraryHierarchyId", DbType.Int32, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None),
                new DatabaseQueryParameter("libraryHierarchyItemId", DbType.Int32, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None),
                new DatabaseQueryParameter("loadMetaData", DbType.Boolean, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None),
                new DatabaseQueryParameter("metaDataType", DbType.Byte, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None)
            );
        }

        public IDatabaseQuery GetOrAddMetaDataItem
        {
            get
            {
                return this.Database.QueryFactory.Create(
                    Resources.GetOrAddMetaDataItem,
                    new DatabaseQueryParameter("name", DbType.String, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None),
                    new DatabaseQueryParameter("type", DbType.Byte, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None),
                    new DatabaseQueryParameter("value", DbType.String, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None)
                );
            }
        }

        public abstract IDatabaseQuery GetPlaylistMetaData(int count);

        public IDatabaseQuery MovePlaylistItem
        {
            get
            {
                return this.Database.QueryFactory.Create(
                    Resources.MovePlaylistItem,
                    new DatabaseQueryParameter("id", DbType.Int32, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None),
                    new DatabaseQueryParameter("sequence", DbType.Int32, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None)
                );
            }
        }

        public IDatabaseQuery RemoveCancelledLibraryItems
        {
            get
            {
                return this.Database.QueryFactory.Create(
                    Resources.RemoveCancelledLibraryItems
                );
            }
        }

        public IDatabaseQuery RemoveLibraryHierarchyItems
        {
            get
            {
                return this.Database.QueryFactory.Create(
                    Resources.RemoveLibraryHierarchyItems,
                    new DatabaseQueryParameter("libraryHierarchyId", DbType.Int32, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None),
                    new DatabaseQueryParameter("status", DbType.Byte, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None)
                );
            }
        }

        public IDatabaseQuery RemoveLibraryItems
        {
            get
            {
                return this.Database.QueryFactory.Create(
                    Resources.RemoveLibraryItems,
                    new DatabaseQueryParameter("status", DbType.Byte, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None)
                );
            }
        }

        public IDatabaseQuery RemovePlaylistItems
        {
            get
            {
                return this.Database.QueryFactory.Create(
                    Resources.RemovePlaylistItems,
                    new DatabaseQueryParameter("playlistId", DbType.Int32, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None),
                    new DatabaseQueryParameter("status", DbType.Byte, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None)
                );
            }
        }

        public abstract IDatabaseQuery SequencePlaylistItems(string sort);

        public IDatabaseQuery UpdateLibraryHierarchyNode
        {
            get
            {
                return this.Database.QueryFactory.Create(
                    Resources.UpdateLibraryHierarchyNode,
                    new DatabaseQueryParameter("libraryHierarchyId", DbType.Int32, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None),
                    new DatabaseQueryParameter("libraryHierarchyItemId", DbType.Int32, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None),
                    new DatabaseQueryParameter("libraryItemId", DbType.Int32, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None)
                );
            }
        }
        public IDatabaseQuery UpdateLibraryVariousArtists
        {
            get
            {
                return this.Database.QueryFactory.Create(
                    Resources.UpdateLibraryVariousArtists,
                    new DatabaseQueryParameter("name", DbType.String, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None),
                    new DatabaseQueryParameter("type", DbType.Byte, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None),
                    new DatabaseQueryParameter("value", DbType.String, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None),
                    new DatabaseQueryParameter("status", DbType.Byte, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None)
                );
            }
        }

        public IDatabaseQuery UpdatePlaylistVariousArtists
        {
            get
            {
                return this.Database.QueryFactory.Create(
                    Resources.UpdatePlaylistVariousArtists,
                    new DatabaseQueryParameter("name", DbType.String, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None),
                    new DatabaseQueryParameter("type", DbType.Byte, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None),
                    new DatabaseQueryParameter("value", DbType.String, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None),
                    new DatabaseQueryParameter("status", DbType.Byte, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None)
                );
            }
        }
    }
}
