using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes
{
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
                    var appliesTo = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
                    foreach (var playlist in this.Playlists)
                    {
                        if (string.IsNullOrEmpty(playlist.Filter))
                        {
                            continue;
                        }
                        if (names != null && names.Any())
                        {
                            if (!appliesTo.GetOrAdd(playlist.Filter, filter => this.FilterParser.AppliesTo(filter, names)))
                            {
                                continue;
                            }
                        }
                        await this.Refresh(playlist).ConfigureAwait(false);
                    }
                    break;
                case CommonSignals.HierarchiesUpdated:
                    if (!object.Equals(signal.State, CommonSignalFlags.SOFT))
                    {
                        foreach (var playlist in this.Playlists)
                        {
                            await this.Refresh(playlist).ConfigureAwait(false);
                        }
                    }
                    break;
            }
            await base.OnSignal(sender, signal).ConfigureAwait(false);
        }

        protected override async Task Refresh(Playlist playlist)
        {
            var libraryHierarchy = this.LibraryManager.SelectedHierarchy;
            if (libraryHierarchy == null || LibraryHierarchy.Empty.Equals(libraryHierarchy))
            {
                return;
            }
            var libraryHierarchyNodes = this.LibraryHierarchyBrowser.GetNodes(libraryHierarchy);
            using (var task = new AddLibraryHierarchyNodesToPlaylistTask(playlist, 0, libraryHierarchyNodes, playlist.Filter, true))
            {
                task.InitializeComponent(this.Core);
                await task.Run().ConfigureAwait(false);
            }
        }
    }
}
