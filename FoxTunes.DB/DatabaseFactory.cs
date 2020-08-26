#pragma warning disable 612, 618
using FoxDb;
using FoxDb.Interfaces;
using FoxTunes.Interfaces;

namespace FoxTunes
{
    public abstract class DatabaseFactory : StandardFactory, IDatabaseFactory
    {
        public ICore Core { get; private set; }

        public IConfig Config { get; private set; }

        public abstract DatabaseFactoryFlags Flags { get; }

        public override void InitializeComponent(ICore core)
        {
            this.Core = core;
            base.InitializeComponent(core);
        }

        public DatabaseTestResult Test()
        {
            using (var database = this.OnCreate())
            {
                return this.OnTest(database);
            }
        }

        protected abstract DatabaseTestResult OnTest(IDatabase database);

        public void Initialize()
        {
            using (var database = this.OnCreate())
            {
                this.OnInitialize(database);
            }
        }

        protected abstract void OnInitialize(IDatabase database);

        public IDatabaseComponent Create()
        {
            var database = this.OnCreate();
            if (this.Config != null)
            {
                this.Config.CopyTo(database.Config);
            }
            else
            {
                this.Config = database.Config;
                this.Configure(database);
            }
            database.InitializeComponent(this.Core);
            return database;
        }

        protected abstract IDatabaseComponent OnCreate();

        protected virtual void Configure(IDatabase database)
        {
            database.Config.Table<PlaylistItem>().With(table =>
            {
                table.Relation(item => item.MetaDatas).With(relation =>
                {
                    relation.Expression.Left = relation.Expression.Clone();
                    relation.Expression.Operator = relation.Expression.CreateOperator(QueryOperator.OrElse);
                    relation.Expression.Right = relation.CreateConstraint().With(constraint =>
                    {
                        constraint.Left = relation.CreateConstraint(
                            database.Config.Table<PlaylistItem>().Column("LibraryItem_Id"),
                            database.Config.Table<LibraryItem, MetaDataItem>().Column("LibraryItem_Id")
                        );
                        constraint.Operator = constraint.CreateOperator(QueryOperator.AndAlso);
                        constraint.Right = relation.CreateConstraint(
                            database.Config.Table<LibraryItem, MetaDataItem>().Column("MetaDataItem_Id"),
                            database.Config.Table<MetaDataItem>().Column("Id")
                        );
                    });
                });
            });
        }
    }
}
