using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;

namespace FoxTunes
{
    [WindowsUserInterfaceDependency]
    public class LikeBehaviour : StandardBehaviour, IInvocableComponent
    {
        public const string SET_LIBRARY_LIKE = "AAAE";

        public const string SET_PLAYLIST_LIKE = "CCCC";

        public ILibraryManager LibraryManager { get; private set; }

        public IPlaylistManager PlaylistManager { get; private set; }

        public LikeManager LikeManager { get; private set; }

        public ILibraryHierarchyBrowser LibraryHierarchyBrowser { get; private set; }

        public IMetaDataBrowser MetaDataBrowser { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public BooleanConfigurationElement Enabled { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.LibraryManager = core.Managers.Library;
            this.PlaylistManager = core.Managers.Playlist;
            this.LikeManager = ComponentRegistry.Instance.GetComponent<LikeManager>();
            this.LibraryHierarchyBrowser = core.Components.LibraryHierarchyBrowser;
            this.MetaDataBrowser = core.Components.MetaDataBrowser;
            this.Configuration = core.Components.Configuration;
            this.Enabled = this.Configuration.GetElement<BooleanConfigurationElement>(
               MetaDataBehaviourConfiguration.SECTION,
               MetaDataBehaviourConfiguration.READ_POPULARIMETER_TAGS
           );
            base.InitializeComponent(core);
        }
        public IEnumerable<string> InvocationCategories
        {
            get
            {
                yield return InvocationComponent.CATEGORY_LIBRARY;
                yield return InvocationComponent.CATEGORY_PLAYLIST;
            }
        }

        public IEnumerable<IInvocationComponent> Invocations
        {
            get
            {
                if (this.Enabled.Value)
                {
                    if (this.LibraryManager.SelectedItem != null)
                    {
                        yield return new InvocationComponent(
                            InvocationComponent.CATEGORY_LIBRARY,
                            SET_LIBRARY_LIKE,
                            "Like",
                            attributes: this.GetLibraryLike(this.LibraryManager.SelectedItem).Result ? InvocationComponent.ATTRIBUTE_SELECTED : InvocationComponent.ATTRIBUTE_NONE
                        );
                    }
                    if (this.PlaylistManager.SelectedItems != null && this.PlaylistManager.SelectedItems.Any())
                    {
                        yield return new InvocationComponent(
                            InvocationComponent.CATEGORY_PLAYLIST,
                            SET_PLAYLIST_LIKE,
                            "Like",
                            attributes: this.GetPlaylistLike(this.PlaylistManager.SelectedItems).Result ? InvocationComponent.ATTRIBUTE_SELECTED : InvocationComponent.ATTRIBUTE_NONE
                        );
                    }
                }
            }
        }

        protected virtual async Task<bool> GetLibraryLike(LibraryHierarchyNode libraryHierarchyNode)
        {
            var likes = await this.MetaDataBrowser.GetMetaDatas(
                libraryHierarchyNode,
                CommonStatistics.Like,
                MetaDataItemType.Tag,
                2
            ).ConfigureAwait(false);
            switch (likes.Length)
            {
                case 0:
                    Logger.Write(this, LogLevel.Debug, "Library hierarchy node {0} tracks have no likes.", libraryHierarchyNode.Id);
                    return false;
                case 1:
                    var like = default(bool);
                    if (!bool.TryParse(likes[0].Value, out like))
                    {
                        Logger.Write(this, LogLevel.Warn, "Library hierarchy node {0} tracks have like \"{1}\" which is in an unknown format.", libraryHierarchyNode.Id, likes[0].Value);
                        return false;
                    }
                    Logger.Write(this, LogLevel.Debug, "Library hierarchy node {0} tracks have like {1}.", libraryHierarchyNode.Id, like);
                    return like;
                default:
                    Logger.Write(this, LogLevel.Debug, "Library hierarchy node {0} tracks have ambiguous likes.", libraryHierarchyNode.Id);
                    return false;
            }
        }

        protected virtual async Task<bool> GetPlaylistLike(PlaylistItem[] playlistItems)
        {
            var likes = await this.MetaDataBrowser.GetMetaDatas(
                playlistItems,
                CommonStatistics.Like,
                MetaDataItemType.Tag,
                2
            ).ConfigureAwait(false);
            switch (likes.Length)
            {
                case 0:
                    Logger.Write(this, LogLevel.Debug, "{0} playlist items have no likes.", playlistItems.Length);
                    return false;
                case 1:
                    var like = default(bool);
                    if (!bool.TryParse(likes[0].Value, out like))
                    {
                        Logger.Write(this, LogLevel.Warn, "{0} playlist items have like \"{1}\" which is in an unknown format.", playlistItems.Length, likes[0].Value);
                        return false;
                    }
                    Logger.Write(this, LogLevel.Debug, "{0} playlist items have like {1}.", playlistItems.Length, like);
                    return like;
                default:
                    Logger.Write(this, LogLevel.Debug, "{0} playlist items have ambiguous likes.", playlistItems.Length);
                    return false;
            }
        }

        public Task InvokeAsync(IInvocationComponent component)
        {
            switch (component.Id)
            {
                case SET_LIBRARY_LIKE:
                    return this.SetLibraryLike();
                case SET_PLAYLIST_LIKE:
                    return this.SetPlaylistLike();
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        protected virtual async Task SetLibraryLike()
        {
            var like = await this.GetLibraryLike(this.LibraryManager.SelectedItem).ConfigureAwait(false);
            var libraryItems = this.LibraryHierarchyBrowser.GetItems(this.LibraryManager.SelectedItem);
            if (!libraryItems.Any())
            {
                return;
            }
            await this.LikeManager.SetLike(libraryItems, !like).ConfigureAwait(false);
        }

        protected virtual async Task SetPlaylistLike()
        {
            var like = await this.GetPlaylistLike(this.PlaylistManager.SelectedItems).ConfigureAwait(false);
            await this.LikeManager.SetLike(this.PlaylistManager.SelectedItems, !like).ConfigureAwait(false);
        }
    }
}
