using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.UserInterface)]
    public class LibraryActionsBehaviour : StandardBehaviour, IInvocableComponent
    {
        public const string APPEND_PLAYLIST = "AAAB";

        public const string REPLACE_PLAYLIST = "AAAC";

        public const string SET_RATING = "AAAD";

        public const string RESCAN = "ZZAA";

        public ILibraryManager LibraryManager { get; private set; }

        public IPlaylistManager PlaylistManager { get; private set; }

        public IMetaDataBrowser MetaDataBrowser { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public BooleanConfigurationElement Popularimeter { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.PlaylistManager = core.Managers.Playlist;
            this.LibraryManager = core.Managers.Library;
            this.MetaDataBrowser = core.Components.MetaDataBrowser;
            this.Configuration = core.Components.Configuration;
            this.Popularimeter = this.Configuration.GetElement<BooleanConfigurationElement>(
                MetaDataBehaviourConfiguration.SECTION,
                MetaDataBehaviourConfiguration.READ_POPULARIMETER_TAGS
            );
            base.InitializeComponent(core);
        }

        public IEnumerable<IInvocationComponent> Invocations
        {
            get
            {
                if (this.LibraryManager.SelectedItem != null)
                {
                    yield return new InvocationComponent(InvocationComponent.CATEGORY_LIBRARY, APPEND_PLAYLIST, "Add To Playlist");
                    yield return new InvocationComponent(InvocationComponent.CATEGORY_LIBRARY, REPLACE_PLAYLIST, "Replace Playlist");
                    if (this.Popularimeter.Value)
                    {
                        var invocationComponents = new Dictionary<byte, InvocationComponent>();
                        for (var a = 0; a <= 5; a++)
                        {
                            var invocationComponent = new InvocationComponent(
                                InvocationComponent.CATEGORY_LIBRARY,
                                SET_RATING,
                                a == 0 ? "None" : string.Format("{0} Stars", a),
                                path: "Set Rating",
                                attributes: InvocationComponent.ATTRIBUTE_SEPARATOR
                            );
                            invocationComponents.Add((byte)a, invocationComponent);
                            yield return invocationComponent;
                        }
                        //Don't block the menu from opening while we fetch ratings.
                        this.Dispatch(() => this.GetRating(this.LibraryManager.SelectedItem, invocationComponents));
                    }
                }
                yield return new InvocationComponent(InvocationComponent.CATEGORY_LIBRARY, RESCAN, "Rescan Files", path: "Library");
            }
        }

        public Task InvokeAsync(IInvocationComponent component)
        {
            switch (component.Id)
            {
                case APPEND_PLAYLIST:
                    return this.AddToPlaylist(false);
                case REPLACE_PLAYLIST:
                    return this.AddToPlaylist(true);
                case SET_RATING:
                    return this.SetRating(component.Name);
                case RESCAN:
                    return this.Rescan();
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        private Task AddToPlaylist(bool clear)
        {
            if (this.LibraryManager.SelectedItem == null)
            {
#if NET40
                return TaskEx.FromResult(false);
#else
                return Task.CompletedTask;
#endif
            }
            return this.PlaylistManager.Add(
                this.PlaylistManager.SelectedPlaylist,
                this.LibraryManager.SelectedItem,
                clear
            );
        }

        protected virtual async Task GetRating(LibraryHierarchyNode libraryHierarchyNode, Dictionary<byte, InvocationComponent> invocationComponents)
        {
            Logger.Write(this, LogLevel.Debug, "Determining rating for library hierarchy node: {0}", libraryHierarchyNode.Id);
            var rating = default(byte);
            var ratings = await this.MetaDataBrowser.GetMetaDatasAsync(libraryHierarchyNode, MetaDataItemType.Tag, CommonMetaData.Rating).ConfigureAwait(false);
            switch (ratings.Length)
            {
                case 0:
                    Logger.Write(this, LogLevel.Debug, "Library hierarchy node {0} tracks have no rating.", libraryHierarchyNode.Id);
                    rating = 0;
                    break;
                case 1:
                    if (!byte.TryParse(ratings[0].Value, out rating))
                    {
                        Logger.Write(this, LogLevel.Warn, "Library hierarchy node {0} tracks have rating \"{1}\" which is in an unknown format.", libraryHierarchyNode.Id, rating);
                        return;
                    }
                    Logger.Write(this, LogLevel.Debug, "Library hierarchy node {0} tracks have rating {1}.", libraryHierarchyNode.Id, rating);
                    break;
                default:
                    Logger.Write(this, LogLevel.Debug, "Library hierarchy node {0} tracks have multiple ratings.", libraryHierarchyNode.Id);
                    return;
            }
            foreach (var key in invocationComponents.Keys)
            {
                var invocationComponent = invocationComponents[key];
                if (key == rating)
                {
                    invocationComponent.Attributes = (byte)(invocationComponent.Attributes | InvocationComponent.ATTRIBUTE_SELECTED);
                }
            }
        }

        protected virtual Task SetRating(string name)
        {
            var rating = default(byte);
            if (string.Equals(name, "None", StringComparison.OrdinalIgnoreCase))
            {
                rating = 0;
            }
            else if (string.IsNullOrEmpty(name) || !byte.TryParse(name.Split(' ').FirstOrDefault(), out rating))
            {
#if NET40
                return TaskEx.FromResult(false);
#else
                return Task.CompletedTask;
#endif
            }
            return this.LibraryManager.SetRating(this.LibraryManager.SelectedItem, rating);
        }

        protected virtual Task Rescan()
        {
            return this.LibraryManager.Rescan();
        }
    }
}
