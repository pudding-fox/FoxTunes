using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;

namespace FoxTunes.Managers
{
    public class LibraryManager : StandardManager, ILibraryManager
    {
        public ICore Core { get; private set; }

        public ILibrary Library { get; private set; }

        public IDatabase Database { get; private set; }

        public ILibraryItemFactory LibraryItemFactory { get; private set; }

        public IPlaybackManager PlaybackManager { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Core = core;
            this.Library = core.Components.Library;
            this.Database = core.Components.Database;
            this.LibraryItemFactory = core.Factories.LibraryItem;
            this.PlaybackManager = core.Managers.Playback;
            base.InitializeComponent(core);
        }

        public void Add(IEnumerable<string> paths)
        {
            var task = new AddPathsToLibraryTask(paths);
            task.InitializeComponent(this.Core);
            task.Completed += (sender, e) =>
            {
                this.BuildHierarchies();
            };
            this.OnBackgroundTask(task);
            task.Run();
        }

        public void BuildHierarchies()
        {
            var task = new BuildLibraryHierarchiesTask();
            task.InitializeComponent(this.Core);
            task.Completed += (sender, e) =>
            {
                this.OnUpdated();
            };
            this.OnBackgroundTask(task);
            task.Run();
        }

        protected virtual void OnUpdated()
        {
            if (this.Updated == null)
            {
                return;
            }
            this.Updated(this, EventArgs.Empty);
        }

        public event EventHandler Updated = delegate { };

        protected virtual void OnBackgroundTask(IBackgroundTask backgroundTask)
        {
            if (this.BackgroundTask == null)
            {
                return;
            }
            this.BackgroundTask(this, new BackgroundTaskEventArgs(backgroundTask));
        }

        public event BackgroundTaskEventHandler BackgroundTask = delegate { };
    }
}
