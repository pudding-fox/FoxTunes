using FoxTunes.Interfaces;
using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Threading;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Threading.Tasks;

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
            var builder = this.CreateModelBuilder();
            this.MapPlaylistItem(builder);
            this.MapLibraryItem(builder);
            this.MapLibraryHierarchy(builder);
            this.MapLibraryHierarchyLevel(builder);
            this.MapLibraryHierarchyItem(builder);
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

        protected virtual void MapLibraryHierarchy(DbModelBuilder builder)
        {
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
            builder.Entity<LibraryHierarchyLevel>();
        }

        protected virtual void MapLibraryHierarchyItem(DbModelBuilder builder)
        {
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

        public override bool CanQuery<T>(T item)
        {
            var entry = this.DbContext.Entry(item);
            return entry.State != EntityState.Detached;
        }

        public override IDatabaseQuery<TMember> GetMemberQuery<T, TMember>(T item, Expression<Func<T, TMember>> member)
        {
            var entry = this.DbContext.Entry(item);
            if (entry.State == EntityState.Detached)
            {
                throw new InvalidOperationException("Item is untracked, cannot query.");
            }
            return new WrappedDbQuery<TMember>(
                this.DbContext,
                this.DbContext.Set<TMember>(),
                entry.Reference(member).Query()
            );
        }

        public override IDatabaseQuery<TMember> GetMemberQuery<T, TMember>(T item, Expression<Func<T, ICollection<TMember>>> member)
        {
            var entry = this.DbContext.Entry(item);
            if (entry.State == EntityState.Detached)
            {
                throw new InvalidOperationException("Item is untracked, cannot query.");
            }
            return new WrappedDbQuery<TMember>(
                this.DbContext,
                this.DbContext.Set<TMember>(),
                entry.Collection(member).Query()
            );
        }
        
        public override void WithAutoDetectChanges(Action action)
        {
            this.DbContext.Configuration.AutoDetectChangesEnabled = true;
            try
            {
                action();
            }
            finally
            {
                this.DbContext.Configuration.AutoDetectChangesEnabled = false;
            }
        }

        public override T WithAutoDetectChanges<T>(Func<T> func)
        {
            this.DbContext.Configuration.AutoDetectChangesEnabled = true;
            try
            {
                return func();
            }
            finally
            {
                this.DbContext.Configuration.AutoDetectChangesEnabled = false;
            }
        }

        public override int SaveChanges()
        {
            if (this.DbContext == null)
            {
                return 0;
            }
            return this.DbContext.SaveChanges();
        }

        public override Task<int> SaveChangesAsync()
        {
            //if (this.DbContext == null)
            //{
            //    return Task.FromResult(0);
            //}
            //return this.DbContext.SaveChangesAsync();
            return Task.FromResult(this.SaveChanges());
        }
    }
}
