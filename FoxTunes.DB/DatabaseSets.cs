using System;
using FoxTunes.Interfaces;

namespace FoxTunes
{
    public class DatabaseSets : BaseComponent, IDatabaseSets
    {
        public IDatabase Database { get; private set; }

        public ISignalEmitter SignalEmitter { get; private set; }

        public IForegroundTaskRunner ForegroundTaskRunner { get; private set; }

        public IDatabaseSet<PlaylistItem> PlaylistItem
        {
            get
            {
                return this.Database.GetSet<PlaylistItem>();
            }
        }

        protected virtual void OnPlaylistItemChanged()
        {
            this.ForegroundTaskRunner.Run(() => this.OnPropertyChanged("PlaylistItem"));
        }

        public IDatabaseSet<PlaylistColumn> PlaylistColumn
        {
            get
            {
                return this.Database.GetSet<PlaylistColumn>();
            }
        }

        protected virtual void OnPlaylistColumnChanged()
        {
            this.ForegroundTaskRunner.Run(() => this.OnPropertyChanged("PlaylistColumn"));
        }

        public IDatabaseSet<LibraryItem> LibraryItem
        {
            get
            {
                return this.Database.GetSet<LibraryItem>();
            }
        }

        protected virtual void OnLibraryItemChanged()
        {
            this.ForegroundTaskRunner.Run(() => this.OnPropertyChanged("LibraryItem"));
        }

        public IDatabaseSet<LibraryHierarchy> LibraryHierarchy
        {
            get
            {
                return this.Database.GetSet<LibraryHierarchy>();
            }
        }

        protected virtual void OnLibraryHierarchyChanged()
        {
            this.ForegroundTaskRunner.Run(() => this.OnPropertyChanged("LibraryHierarchy"));
        }

        public IDatabaseSet<LibraryHierarchyLevel> LibraryHierarchyLevel
        {
            get
            {
                return this.Database.GetSet<LibraryHierarchyLevel>();
            }
        }

        protected virtual void OnLibraryHierarchyLevelChanged()
        {
            this.ForegroundTaskRunner.Run(() => this.OnPropertyChanged("LibraryHierarchyLevel"));
        }

        public override void InitializeComponent(ICore core)
        {
            this.Database = core.Components.Database;
            this.SignalEmitter = core.Components.SignalEmitter;
            this.SignalEmitter.Signal += this.OnSignal;
            this.ForegroundTaskRunner = core.Components.ForegroundTaskRunner;
            base.InitializeComponent(core);
        }

        protected virtual void OnSignal(object sender, ISignal signal)
        {
            switch (signal.Name)
            {
                case CommonSignals.PlaylistUpdated:
                    this.OnPlaylistItemChanged();
                    break;
                case CommonSignals.HierarchiesUpdated:
                    this.OnLibraryHierarchyChanged();
                    break;
            }
        }
    }
}
