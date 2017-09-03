using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Transactions;

namespace FoxTunes
{
    public abstract class EntityFrameworkDatabase : Database
    {
        protected DbContext DbContext { get; private set; }

        protected ObjectContext ObjectContext
        {
            get
            {
                return (this.DbContext as IObjectContextAdapter).ObjectContext;
            }
        }

        protected virtual DbModelBuilder CreateModelBuilder()
        {
            var builder = new DbModelBuilder();
            return builder;
        }

        protected virtual DbModel CreateDbModel()
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
            return builder.Build(this.Connection);
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
            builder.Entity<ImageItem>()
                .HasMany(item => item.MetaDatas)
                .WithMany()
                .Map(config =>
                {
                    config.MapLeftKey("ImageItem_Id");
                    config.MapRightKey("MetaDataItem_Id");
                    config.ToTable("ImageItem_MetaDataItem");
                });
        }

        protected virtual void MapStatisticItem(DbModelBuilder builder)
        {
            Logger.Write(this, LogLevel.Debug, "Creating database mapping: {0}", typeof(StatisticItem).Name);
            builder.Entity<StatisticItem>();
        }

        protected virtual DbContext CreateDbContext()
        {
            var model = this.CreateDbModel();
            Logger.Write(this, LogLevel.Debug, "Compiling database model.");
            var compiled = model.Compile();
            Logger.Write(this, LogLevel.Debug, "Creating database context.");
            return new InternalDbContext(this.Connection, compiled);
        }

        public override IDatabaseSet<T> GetSet<T>()
        {
            if (this.DbContext == null)
            {
                this.DbContext = this.CreateDbContext();
            }
            Logger.Write(this, LogLevel.Debug, "Creating wrapped database set: {0}", typeof(T).Name);
            return new WrappedDbSet<T>(this.DbContext, this.DbContext.Set<T>());
        }

        public override IDatabaseQuery<T> GetQuery<T>()
        {
            if (this.DbContext == null)
            {
                this.DbContext = this.CreateDbContext();
            }
            Logger.Write(this, LogLevel.Debug, "Creating wrapped database query: {0}", typeof(T).Name);
            return new WrappedDbQuery<T>(this.DbContext, this.DbContext.Set<T>());
        }

        public override bool CanQuery<T>(T item)
        {
            var entry = this.DbContext.Entry(item);
            return entry.State != EntityState.Added && entry.State != EntityState.Detached;
        }

        public override IDatabaseQuery<TMember> GetMemberQuery<T, TMember>(T item, Expression<Func<T, TMember>> member)
        {
            if (!this.CanQuery(item))
            {
                throw new InvalidOperationException("Item is untracked, cannot query.");
            }
            Logger.Write(this, LogLevel.Debug, "Creating wrapped database member query: {0} => {1}", typeof(T).Name, typeof(TMember).Name);
            var entry = this.DbContext.Entry(item);
            return new WrappedDbQuery<TMember>(
                this.DbContext,
                this.DbContext.Set<TMember>(),
                entry.Reference(member).Query()
            );
        }

        public override IDatabaseQuery<TMember> GetMemberQuery<T, TMember>(T item, Expression<Func<T, ICollection<TMember>>> member)
        {
            if (!this.CanQuery(item))
            {
                throw new InvalidOperationException("Item is untracked, cannot query.");
            }
            Logger.Write(this, LogLevel.Debug, "Creating wrapped database member collection query: {0} => {1}", typeof(T).Name, typeof(TMember).Name);
            var entry = this.DbContext.Entry(item);
            return new WrappedDbQuery<TMember>(
                this.DbContext,
                this.DbContext.Set<TMember>(),
                entry.Collection(member).Query()
            );
        }

        public override void WithAutoDetectChanges(Action action)
        {
            Logger.Write(this, LogLevel.Debug, "Begin executing action with auto detect changes enabled.");
            this.DbContext.Configuration.AutoDetectChangesEnabled = true;
            try
            {
                action();
            }
            finally
            {
                Logger.Write(this, LogLevel.Debug, "End executing action with auto detect changes enabled.");
                this.DbContext.Configuration.AutoDetectChangesEnabled = false;
            }
        }

        public override T WithAutoDetectChanges<T>(Func<T> func)
        {
            Logger.Write(this, LogLevel.Debug, "Begin executing action with auto detect changes enabled.");
            this.DbContext.Configuration.AutoDetectChangesEnabled = true;
            try
            {
                return func();
            }
            finally
            {
                Logger.Write(this, LogLevel.Debug, "End executing action with auto detect changes enabled.");
                this.DbContext.Configuration.AutoDetectChangesEnabled = false;
            }
        }

        public override int SaveChanges()
        {
            if (this.DbContext == null)
            {
                return 0;
            }
            using (var transaction = this.BeginTransaction())
            {
                var count = this.DbContext.SaveChanges();
                transaction.Complete();
                Logger.Write(this, LogLevel.Debug, "Saved {0} changes to database.", count);
                return count;
            }
        }

        public override async Task<int> SaveChangesAsync()
        {
            if (this.DbContext == null)
            {
                return 0;
            }
            using (var transaction = this.BeginTransaction())
            {
                var count = await this.DbContext.SaveChangesAsync();
                transaction.Complete();
                Logger.Write(this, LogLevel.Debug, "Saved {0} changes to database.", count);
                return count;
            }
        }

        private TransactionScope BeginTransaction()
        {
            var options = new TransactionOptions();
            return new TransactionScope(TransactionScopeOption.Required, options);
        }
    }
}
