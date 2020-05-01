using FoxTunes.Integration;
using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.UserInterface)]
    public class PlaylistActionsBehaviour : StandardBehaviour, IInvocableComponent
    {
        public const string REMOVE_PLAYLIST_ITEMS = "AAAA";

        public const string CROP_PLAYLIST_ITEMS = "AAAB";

        public const string LOCATE_PLAYLIST_ITEMS = "AAAC";

        public const string SET_RATING = "AAAD";

#if NET40
        //ListView grouping is too slow under net40 due to lack of virtualization.
#else
        public const string TOGGLE_GROUPING = "MMMM";
#endif

        public const string TOGGLE_FOLLOW_LIBRARY = "NNNN";

        public IPlaylistManager PlaylistManager { get; private set; }

        public IMetaDataBrowser MetaDataBrowser { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public BooleanConfigurationElement Popularimeter { get; private set; }

#if NET40
        //ListView grouping is too slow under net40 due to lack of virtualization.
#else
        public BooleanConfigurationElement Grouping { get; private set; }
#endif

        public BooleanConfigurationElement FollowLibrary { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.PlaylistManager = core.Managers.Playlist;
            this.MetaDataBrowser = core.Components.MetaDataBrowser;
            this.Configuration = core.Components.Configuration;
            this.Popularimeter = this.Configuration.GetElement<BooleanConfigurationElement>(
                MetaDataBehaviourConfiguration.SECTION,
                MetaDataBehaviourConfiguration.READ_POPULARIMETER_TAGS
            );
#if NET40
            //ListView grouping is too slow under net40 due to lack of virtualization.
#else
            this.Grouping = this.Configuration.GetElement<BooleanConfigurationElement>(
                PlaylistBehaviourConfiguration.SECTION,
                PlaylistGroupingBehaviourConfiguration.GROUP_ENABLED_ELEMENT
            );
#endif
            this.FollowLibrary = this.Configuration.GetElement<BooleanConfigurationElement>(
                PlaylistBehaviourConfiguration.SECTION,
                PlaylistTracksLibraryBehaviourConfiguration.ENABLED_ELEMENT
            );
            base.InitializeComponent(core);
        }

        public IEnumerable<IInvocationComponent> Invocations
        {
            get
            {
                if (this.PlaylistManager.SelectedItems != null && this.PlaylistManager.SelectedItems.Any())
                {
                    yield return new InvocationComponent(InvocationComponent.CATEGORY_PLAYLIST, REMOVE_PLAYLIST_ITEMS, "Remove");
                    yield return new InvocationComponent(InvocationComponent.CATEGORY_PLAYLIST, CROP_PLAYLIST_ITEMS, "Crop");
                    if (this.Popularimeter.Value)
                    {
                        var invocationComponents = new Dictionary<byte, InvocationComponent>();
                        for (var a = 0; a <= 5; a++)
                        {
                            var invocationComponent = new InvocationComponent(
                                InvocationComponent.CATEGORY_PLAYLIST,
                                SET_RATING,
                                a == 0 ? "None" : string.Format("{0} Stars", a),
                                path: "Set Rating",
                                attributes: InvocationComponent.ATTRIBUTE_SEPARATOR
                            );
                            invocationComponents.Add((byte)a, invocationComponent);
                            yield return invocationComponent;
                        }
                        //Don't block the menu from opening while we fetch ratings.
                        this.Dispatch(() => this.GetRating(this.PlaylistManager.SelectedItems, invocationComponents));
                    }
                    yield return new InvocationComponent(InvocationComponent.CATEGORY_PLAYLIST, LOCATE_PLAYLIST_ITEMS, "Locate");
                }
#if NET40
                //ListView grouping is too slow under net40 due to lack of virtualization.
#else
                yield return new InvocationComponent(
                    InvocationComponent.CATEGORY_PLAYLIST, 
                    TOGGLE_GROUPING, 
                    "Grouping", 
                    path: "Playlist",
                    attributes: this.Grouping.Value ? InvocationComponent.ATTRIBUTE_SELECTED : InvocationComponent.ATTRIBUTE_NONE
                );
#endif
                yield return new InvocationComponent(
                    InvocationComponent.CATEGORY_PLAYLIST, 
                        TOGGLE_FOLLOW_LIBRARY, 
                        "Follow Library", 
                        path: "Playlist", 
                        attributes: this.FollowLibrary.Value ? InvocationComponent.ATTRIBUTE_SELECTED : InvocationComponent.ATTRIBUTE_NONE
                );
            }
        }

        public Task InvokeAsync(IInvocationComponent component)
        {
            switch (component.Id)
            {
                case REMOVE_PLAYLIST_ITEMS:
                    return this.RemovePlaylistItems();
                case CROP_PLAYLIST_ITEMS:
                    return this.CropPlaylistItems();
                case LOCATE_PLAYLIST_ITEMS:
                    return this.LocatePlaylistItems();
                case SET_RATING:
                    return this.SetRating(component.Name);
#if NET40
                //ListView grouping is too slow under net40 due to lack of virtualization.
#else
                case TOGGLE_GROUPING:
                    this.Grouping.Toggle();
                    this.Configuration.Save();
                    break;
#endif
                case TOGGLE_FOLLOW_LIBRARY:
                    this.FollowLibrary.Toggle();
                    this.Configuration.Save();
                    break;
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        protected virtual Task RemovePlaylistItems()
        {
            return this.PlaylistManager.Remove(this.PlaylistManager.SelectedItems);
        }

        protected virtual Task CropPlaylistItems()
        {
            return this.PlaylistManager.Crop(this.PlaylistManager.SelectedItems);
        }

        protected virtual Task LocatePlaylistItems()
        {
            foreach (var item in this.PlaylistManager.SelectedItems)
            {
                Explorer.Select(item.FileName);
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        protected virtual async Task GetRating(IEnumerable<PlaylistItem> playlistItems, Dictionary<byte, InvocationComponent> invocationComponents)
        {
            if (playlistItems.Count() > ListViewExtensions.MAX_SELECTED_ITEMS)
            {
                //This would result in too many parameters.
                return;
            }
            var rating = default(byte);
            var ratings = await this.MetaDataBrowser.GetMetaDatasAsync(playlistItems, MetaDataItemType.Tag, CommonMetaData.Rating).ConfigureAwait(false);
            switch (ratings.Length)
            {
                case 0:
                    rating = 0;
                    break;
                case 1:
                    if (!byte.TryParse(ratings[0].Value, out rating))
                    {
                        return;
                    }
                    break;
                default:
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
            return this.PlaylistManager.SetRating(this.PlaylistManager.SelectedItems, rating);
        }
    }
}
