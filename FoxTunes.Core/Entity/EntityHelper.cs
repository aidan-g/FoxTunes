using FoxDb;
using FoxDb.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class EntityHelper<T> : BaseComponent
    {
        public EntityHelper(IDatabase database, ITableConfig table, ITransactionSource transaction = null)
        {
            this.Database = database;
            this.Table = table;
            this.Transaction = transaction;
        }

        public IDatabase Database { get; private set; }

        public ITableConfig Table { get; private set; }

        public ITransactionSource Transaction { get; private set; }

        public async Task UpdateAsync(IEnumerable<T> entities, IEnumerable<string> columns)
        {
            var builder = this.Database.QueryFactory.Build();
            builder.Update.SetTable(this.Table);
            if (columns != null && columns.Any())
            {
                builder.Update.AddColumns(this.Table.UpdatableColumns.Where(column => columns.Contains(column.ColumnName, true)));
            }
            else
            {
                builder.Update.AddColumns(this.Table.UpdatableColumns);
            }
            builder.Filter.AddColumns(this.Table.PrimaryKeys);
            var query = builder.Build();
            foreach (var entity in entities)
            {
                var parameters = new ParameterHandlerStrategy(this.Table, entity).Handler;
                await this.Database.ExecuteAsync(query, parameters, this.Transaction).ConfigureAwait(false);
            }
        }

        public static EntityHelper<T> Create(IDatabase database, ITableConfig table, ITransactionSource transaction = null)
        {
            return new EntityHelper<T>(database, table, transaction);
        }
    }
}
