using FoxTunes.Interfaces;
using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace FoxTunes
{
    public abstract class EntityFrameworkDatabase : Database
    {
        protected DbContext DbContext { get; private set; }

        protected virtual DbModelBuilder CreateModelBuilder()
        {
            var builder = new DbModelBuilder();
            return builder;
        }

        protected virtual DbModel CreateDbModel()
        {
            var builder = this.CreateModelBuilder();
            this.MapPlaylistItem(builder);
            this.MapLibraryItem(builder);
            this.MapMetaDataItem(builder);
            this.MapPropertyItem(builder);
            this.MapStatisticItem(builder);
            return builder.Build(this.Connection);
        }

        protected virtual void MapPlaylistItem(DbModelBuilder builder)
        {
            builder.Entity<PlaylistItem>()
                .HasMany(item => item.MetaDatas)
                .WithMany()
                .Map(config =>
                {
                    config.MapLeftKey("PlaylistItem_Id");
                    config.MapRightKey("MetaDataItem_Id");
                    config.ToTable("PlaylistItem_MetaDataItem");
                });
            builder.Entity<PlaylistItem>()
                .HasMany(item => item.Properties)
                .WithMany()
                .Map(config =>
                {
                    config.MapLeftKey("PlaylistItem_Id");
                    config.MapRightKey("PropertyItem_Id");
                    config.ToTable("PlaylistItem_PropertyItem");
                });
        }

        protected virtual void MapLibraryItem(DbModelBuilder builder)
        {
            builder.Entity<LibraryItem>()
                .HasMany(item => item.MetaDatas)
                .WithMany()
                .Map(config =>
                {
                    config.MapLeftKey("LibraryItem_Id");
                    config.MapRightKey("MetaDataItem_Id");
                    config.ToTable("LibraryItem_MetaDataItem");
                });
            builder.Entity<LibraryItem>()
                .HasMany(item => item.Properties)
                .WithMany()
                .Map(config =>
                {
                    config.MapLeftKey("LibraryItem_Id");
                    config.MapRightKey("PropertyItem_Id");
                    config.ToTable("LibraryItem_PropertyItem");
                });
            builder.Entity<LibraryItem>()
                .HasMany(item => item.Statistics)
                .WithMany()
                .Map(config =>
                {
                    config.MapLeftKey("LibraryItem_Id");
                    config.MapRightKey("StatisticItem_Id");
                    config.ToTable("LibraryItem_StatisticItem");
                });
        }

        protected virtual void MapMetaDataItem(DbModelBuilder builder)
        {
            builder.Entity<MetaDataItem>();
        }

        protected virtual void MapPropertyItem(DbModelBuilder builder)
        {
            builder.Entity<PropertyItem>();
        }

        protected virtual void MapStatisticItem(DbModelBuilder builder)
        {
            builder.Entity<StatisticItem>();
        }

        protected virtual DbContext CreateDbContext()
        {
            var model = this.CreateDbModel();
            var compiled = model.Compile();
            return new InternalDbContext(this.Connection, compiled);
        }

        public override IDatabaseSet<T> GetSet<T>()
        {
            if (this.DbContext == null)
            {
                this.DbContext = this.CreateDbContext();
            }
            return new WrappedDbSet<T>(this.DbContext, this.DbContext.Set<T>());
        }

        public override IDatabaseQuery<T> GetQuery<T>()
        {
            if (this.DbContext == null)
            {
                this.DbContext = this.CreateDbContext();
            }
            return new WrappedDbQuery<T>(this.DbContext, this.DbContext.Set<T>());
        }

        public override int SaveChanges()
        {
            if (this.DbContext == null)
            {
                return 0;
            }
            return this.DbContext.SaveChanges();
        }
    }
}
