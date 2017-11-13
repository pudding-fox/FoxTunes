using FoxTunes.Interfaces;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace FoxTunes
{
    public abstract class EntityFrameworkDatabase : Database
    {
        protected DbCompiledModel DbCompiledModel { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            var connection = this.CreateConnection();
            var model = this.CreateDbModel(connection);
            Logger.Write(this, LogLevel.Debug, "Compiling database model.");
            this.DbCompiledModel = model.Compile();
            base.InitializeComponent(core);
        }

        public override IDatabaseContext CreateContext()
        {
            var dbContext = this.CreateDbContext();
            return new EntityFrameworkDatabaseContext(dbContext);
        }

        protected virtual DbModelBuilder CreateModelBuilder()
        {
            var builder = new DbModelBuilder();
            return builder;
        }

        protected virtual DbModel CreateDbModel(DbConnection connection)
        {
            Logger.Write(this, LogLevel.Debug, "Creating database model.");
            var builder = this.CreateModelBuilder();
            this.MapPlaylistItem(builder);
            this.MapPlaylistColumn(builder);
            this.MapLibraryItem(builder);
            this.MapLibraryHierarchy(builder);
            this.MapLibraryHierarchyLevel(builder);
            this.MapLibraryHierarchyItem(builder);
            this.MapMetaDataItem(builder);
            return builder.Build(connection);
        }

        protected virtual DbContext CreateDbContext()
        {
            var connection = this.CreateConnection();
            switch (connection.State)
            {
                case ConnectionState.Closed:
                    connection.Open();
                    break;
            }
            return new InternalDbContext(connection, this.DbCompiledModel);
        }

        protected virtual void MapPlaylistItem(DbModelBuilder builder)
        {
            Logger.Write(this, LogLevel.Debug, "Creating database mapping: {0}", typeof(PlaylistItem).Name);
            builder.Entity<PlaylistItem>()
                .HasMany(item => item.MetaDatas)
                .WithMany()
                .Map(config =>
                {
                    config.MapLeftKey("PlaylistItem_Id");
                    config.MapRightKey("MetaDataItem_Id");
                    config.ToTable("PlaylistItem_MetaDataItem");
                });
        }

        protected virtual void MapPlaylistColumn(DbModelBuilder builder)
        {
            builder.Entity<PlaylistColumn>();
        }

        protected virtual void MapLibraryItem(DbModelBuilder builder)
        {
            Logger.Write(this, LogLevel.Debug, "Creating database mapping: {0}", typeof(LibraryItem).Name);
            builder.Entity<LibraryItem>()
                .HasMany(item => item.MetaDatas)
                .WithMany()
                .Map(config =>
                {
                    config.MapLeftKey("LibraryItem_Id");
                    config.MapRightKey("MetaDataItem_Id");
                    config.ToTable("LibraryItem_MetaDataItem");
                });
        }

        protected virtual void MapLibraryHierarchy(DbModelBuilder builder)
        {
            Logger.Write(this, LogLevel.Debug, "Creating database mapping: {0}", typeof(LibraryHierarchy).Name);
            builder.Entity<LibraryHierarchy>()
                .HasMany(item => item.Levels)
               .WithMany()
               .Map(config =>
               {
                   config.MapLeftKey("LibraryHierarchy_Id");
                   config.MapRightKey("LibraryHierarchyLevel_Id");
                   config.ToTable("LibraryHierarchy_LibraryHierarchyLevel");
               });
        }

        protected virtual void MapLibraryHierarchyLevel(DbModelBuilder builder)
        {
            Logger.Write(this, LogLevel.Debug, "Creating database mapping: {0}", typeof(LibraryHierarchyLevel).Name);
            builder.Entity<LibraryHierarchyLevel>();
        }

        protected virtual void MapLibraryHierarchyItem(DbModelBuilder builder)
        {
            Logger.Write(this, LogLevel.Debug, "Creating database mapping: {0}", typeof(LibraryHierarchyItem).Name);
            builder.Entity<LibraryHierarchyItem>()
                .HasOptional(item => item.LibraryHierarchy);
            builder.Entity<LibraryHierarchyItem>()
                .HasOptional(item => item.LibraryHierarchyLevel);
        }

        protected virtual void MapMetaDataItem(DbModelBuilder builder)
        {
            Logger.Write(this, LogLevel.Debug, "Creating database mapping: {0}", typeof(MetaDataItem).Name);
            builder.Entity<MetaDataItem>();
        }
    }
}
