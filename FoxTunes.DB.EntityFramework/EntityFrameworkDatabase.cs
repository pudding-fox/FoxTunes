using FoxTunes.Interfaces;
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
            this.MapPropertyItem(builder);
            this.MapImageItem(builder);
            this.MapStatisticItem(builder);
            return builder.Build(connection);
        }

        protected virtual DbContext CreateDbContext()
        {
            return new InternalDbContext(this.CreateConnection(), this.DbCompiledModel);
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
            builder.Entity<PlaylistItem>()
                .HasMany(item => item.Properties)
                .WithMany()
                .Map(config =>
                {
                    config.MapLeftKey("PlaylistItem_Id");
                    config.MapRightKey("PropertyItem_Id");
                    config.ToTable("PlaylistItem_PropertyItem");
                });
            builder.Entity<PlaylistItem>()
                .HasMany(item => item.Images)
                .WithMany()
                .Map(config =>
                {
                    config.MapLeftKey("PlaylistItem_Id");
                    config.MapRightKey("ImageItem_Id");
                    config.ToTable("PlaylistItem_ImageItem");
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
                .HasMany(item => item.Images)
                .WithMany()
                .Map(config =>
                {
                    config.MapLeftKey("LibraryItem_Id");
                    config.MapRightKey("ImageItem_Id");
                    config.ToTable("LibraryItem_ImageItem");
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
            builder.Entity<LibraryHierarchy>()
                .HasMany(item => item.Items)
                .WithMany()
                .Map(config =>
                {
                    config.MapLeftKey("LibraryHierarchy_Id");
                    config.MapRightKey("LibraryHierarchyItem_Id");
                    config.ToTable("LibraryHierarchy_LibraryHierarchyItem");
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
            //builder.Entity<LibraryHierarchyItem>()
            //    .HasOptional(item => item.Parent)
            //    .WithMany(item => item.Children);
            builder.Entity<LibraryHierarchyItem>()
                .HasMany(item => item.Items)
                .WithMany()
                .Map(config =>
                {
                    config.MapLeftKey("LibraryItem_Id");
                    config.MapRightKey("LibraryHierarchyItem_Id");
                    config.ToTable("LibraryItem_LibraryHierarchyItem");
                });
        }

        protected virtual void MapMetaDataItem(DbModelBuilder builder)
        {
            Logger.Write(this, LogLevel.Debug, "Creating database mapping: {0}", typeof(MetaDataItem).Name);
            builder.Entity<MetaDataItem>();
        }

        protected virtual void MapPropertyItem(DbModelBuilder builder)
        {
            Logger.Write(this, LogLevel.Debug, "Creating database mapping: {0}", typeof(PropertyItem).Name);
            builder.Entity<PropertyItem>();
        }

        public virtual void MapImageItem(DbModelBuilder builder)
        {
            Logger.Write(this, LogLevel.Debug, "Creating database mapping: {0}", typeof(ImageItem).Name);
            builder.Entity<ImageItem>();
        }

        protected virtual void MapStatisticItem(DbModelBuilder builder)
        {
            Logger.Write(this, LogLevel.Debug, "Creating database mapping: {0}", typeof(StatisticItem).Name);
            builder.Entity<StatisticItem>();
        }
    }
}
