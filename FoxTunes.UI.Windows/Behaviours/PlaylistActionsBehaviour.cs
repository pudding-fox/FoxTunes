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

#if NET40
        //ListView grouping is too slow under net40 due to lack of virtualization.
#else
        public const string TOGGLE_GROUPING = "MMMM";
#endif

        public IPlaylistManager PlaylistManager { get; private set; }

        public IConfiguration Configuration { get; private set; }

#if NET40
        //ListView grouping is too slow under net40 due to lack of virtualization.
#else
        public BooleanConfigurationElement Grouping { get; private set; }
#endif

        public override void InitializeComponent(ICore core)
        {
            this.PlaylistManager = core.Managers.Playlist;
            this.Configuration = core.Components.Configuration;
#if NET40
            //ListView grouping is too slow under net40 due to lack of virtualization.
#else
            this.Grouping = this.Configuration.GetElement<BooleanConfigurationElement>(
                PlaylistBehaviourConfiguration.SECTION,
                PlaylistGroupingBehaviourConfiguration.GROUP_ENABLED_ELEMENT
            );
#endif
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
#if NET40
                    //ListView grouping is too slow under net40 due to lack of virtualization.
#else
                case TOGGLE_GROUPING:
                    this.Grouping.Toggle();
                    this.Configuration.Save();
                    break;
#endif
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        protected virtual Task RemovePlaylistItems()
        {
            return this.PlaylistManager.Remove(this.PlaylistManager.SelectedPlaylist, this.PlaylistManager.SelectedItems);
        }

        protected virtual Task CropPlaylistItems()
        {
            return this.PlaylistManager.Crop(this.PlaylistManager.SelectedPlaylist, this.PlaylistManager.SelectedItems);
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
    }
}
