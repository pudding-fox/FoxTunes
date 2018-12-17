#pragma warning disable 612, 618
using FoxDb;
using FoxDb.Interfaces;
using FoxTunes.Interfaces;

namespace FoxTunes
{
    public abstract class DatabaseFactory : StandardFactory, IDatabaseFactory
    {
        public IConfig Config { get; private set; }

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
                this.Configure();
            }
            return database;
        }

        protected abstract IDatabaseComponent OnCreate();

        protected virtual void Configure()
        {
            this.Config.Table<PlaylistItem>().With(table =>
            {
                table.Relation(item => item.MetaDatas).With(relation =>
                {
                    relation.Expression.Left = relation.Expression.Clone();
                    relation.Expression.Operator = relation.Expression.CreateOperator(QueryOperator.OrElse);
                    relation.Expression.Right = relation.CreateConstraint().With(constraint =>
                    {
                        constraint.Left = relation.CreateConstraint(
                            this.Config.Table<PlaylistItem>().Column("LibraryItem_Id"),
                            this.Config.Table<LibraryItem, MetaDataItem>().Column("LibraryItem_Id")
                        );
                        constraint.Operator = constraint.CreateOperator(QueryOperator.AndAlso);
                        constraint.Right = relation.CreateConstraint(
                            this.Config.Table<LibraryItem, MetaDataItem>().Column("MetaDataItem_Id"),
                            this.Config.Table<MetaDataItem>().Column("Id")
                        );
                    });
                });
            });
        }
    }
}
