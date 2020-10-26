using FoxDb;
using FoxDb.Interfaces;
using FoxTunes.Interfaces;
using FoxTunes.Templates;
using System.Collections.Generic;
using System.Data;

namespace FoxTunes
{
    public class SqlServerDatabaseQueries : DatabaseQueries
    {
        public SqlServerDatabaseQueries(IDatabase database)
            : base(database)
        {
        }

        public override IDatabaseQuery AddLibraryHierarchyNodeToPlaylist(string filter, string sort)
        {
            var filterResult = default(IFilterParserResult);
            var sortResult = default(ISortParserResult);
            if (!string.IsNullOrEmpty(filter) && !this.FilterParser.TryParse(filter, out filterResult))
            {
                //TODO: Warn, failed to parse filter.
                filterResult = null;
            }
            if (!string.IsNullOrEmpty(sort) && !this.SortParser.TryParse(sort, out sortResult))
            {
                //TODO: Warn, failed to parse sort.
                sortResult = null;
            }
            var template = new AddLibraryHierarchyNodeToPlaylist(this.Database, filterResult, sortResult);
            return this.Database.QueryFactory.Create(
                template.TransformText(),
                new DatabaseQueryParameter("playlistId", DbType.Int32, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None),
                new DatabaseQueryParameter("libraryHierarchyId", DbType.Int32, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None),
                new DatabaseQueryParameter("libraryHierarchyItemId", DbType.Int32, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None),
                new DatabaseQueryParameter("sequence", DbType.Int32, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None),
                new DatabaseQueryParameter("status", DbType.Byte, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None)
            );
        }

        public override IDatabaseQuery SequencePlaylistItems(string sort)
        {
            var sortResult = default(ISortParserResult);
            if (!string.IsNullOrEmpty(sort) && !this.SortParser.TryParse(sort, out sortResult))
            {
                //TODO: Warn, failed to parse sort.
                sortResult = null;
            }
            var template = new PlaylistSequenceBuilder(this.Database, sortResult);
            return this.Database.QueryFactory.Create(
                template.TransformText(),
                new DatabaseQueryParameter("playlistId", DbType.Int32, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None),
                new DatabaseQueryParameter("status", DbType.Byte, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None)
            );
        }

        public override IDatabaseQuery GetLibraryHierarchyMetaData(string filter)
        {
            var result = default(IFilterParserResult);
            if (!string.IsNullOrEmpty(filter) && !this.FilterParser.TryParse(filter, out result))
            {
                //TODO: Warn, failed to parse filter.
                result = null;
            }
            var template = new GetLibraryHierarchyMetaData(this.Database, result);
            return this.Database.QueryFactory.Create(
                template.TransformText(),
                new DatabaseQueryParameter("libraryHierarchyItemId", DbType.Int32, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None),
                new DatabaseQueryParameter("name", DbType.String, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None),
                new DatabaseQueryParameter("type", DbType.Byte, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None)
            );
        }

        public override IDatabaseQuery GetPlaylistMetaData(int count)
        {
            var template = new GetPlaylistMetaData(this.Database, count);
            var parameters = new List<DatabaseQueryParameter>();
            for (var position = 0; position < count; position++)
            {
                parameters.Add(new DatabaseQueryParameter("playlistItemId" + position, DbType.Int32, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None));
            }
            parameters.Add(new DatabaseQueryParameter("libraryHierarchyItemId", DbType.Int32, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None));
            parameters.Add(new DatabaseQueryParameter("name", DbType.String, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None));
            parameters.Add(new DatabaseQueryParameter("type", DbType.Byte, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None));
            return this.Database.QueryFactory.Create(
                template.TransformText(),
                parameters.ToArray()
            );
        }
    }
}
