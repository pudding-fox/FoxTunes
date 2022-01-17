using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.Database)]
    public class DynamicPlaylistBehaviour : PlaylistBehaviourBase
    {
        public override Func<Playlist, bool> Predicate
        {
            get
            {
                return playlist => playlist.Type == PlaylistType.Dynamic && playlist.Enabled;
            }
        }

        public ICore Core { get; private set; }

        public ILibraryManager LibraryManager { get; private set; }

        public ILibraryHierarchyBrowser LibraryHierarchyBrowser { get; private set; }

        public IFilterParser FilterParser { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Core = core;
            this.LibraryManager = core.Managers.Library;
            this.LibraryHierarchyBrowser = core.Components.LibraryHierarchyBrowser;
            this.FilterParser = core.Components.FilterParser;
            base.InitializeComponent(core);
        }

        protected override async Task OnSignal(object sender, ISignal signal)
        {
            switch (signal.Name)
            {
                case CommonSignals.MetaDataUpdated:
                    var names = signal.State as IEnumerable<string>;
                    await this.Refresh(names).ConfigureAwait(false);
                    break;
                case CommonSignals.HierarchiesUpdated:
                    await this.Refresh(false).ConfigureAwait(false);
                    break;
            }
            await base.OnSignal(sender, signal).ConfigureAwait(false);
        }

        protected virtual async Task Refresh(IEnumerable<string> names)
        {
            foreach (var playlist in this.GetPlaylists())
            {
                if (string.IsNullOrEmpty(playlist.Filter))
                {
                    continue;
                }
                if (names != null && names.Any())
                {
                    if (!this.FilterParser.AppliesTo(playlist.Filter, names))
                    {
                        continue;
                    }
                }
                await this.Refresh(playlist, false).ConfigureAwait(false);
            }
        }

        public override Task Refresh(Playlist playlist, bool force)
        {
            return this.Refresh(playlist, playlist.Filter, force);
        }

        protected virtual async Task Refresh(Playlist playlist, string filter, bool force)
        {
            var libraryHierarchy = this.LibraryManager.SelectedHierarchy;
            if (libraryHierarchy == null || LibraryHierarchy.Empty.Equals(libraryHierarchy))
            {
                return;
            }
            var libraryHierarchyNodes = this.LibraryHierarchyBrowser.GetNodes(libraryHierarchy, filter);
            if (!libraryHierarchyNodes.Any())
            {
                Logger.Write(this, LogLevel.Debug, "Library search returned no results: {0}", filter);
                using (var task = new ClearPlaylistTask(playlist))
                {
                    task.InitializeComponent(this.Core);
                    await task.Run().ConfigureAwait(false);
                }
            }
            else
            {
                using (var task = new AddLibraryHierarchyNodesToPlaylistTask(playlist, 0, libraryHierarchyNodes, filter, true, false))
                {
                    task.InitializeComponent(this.Core);
                    await task.Run().ConfigureAwait(false);
                }
            }
        }
    }
}
