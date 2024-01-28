using FoxTunes.Interfaces;
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

        public ILibraryManager LibraryManager { get; private set; }

        public IPlaylistManager PlaylistManager { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public BooleanConfigurationElement Popularimeter { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.PlaylistManager = core.Managers.Playlist;
            this.LibraryManager = core.Managers.Library;
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
                yield return new InvocationComponent(InvocationComponent.CATEGORY_LIBRARY, APPEND_PLAYLIST, "Add To Playlist");
                yield return new InvocationComponent(InvocationComponent.CATEGORY_LIBRARY, REPLACE_PLAYLIST, "Replace Playlist");
                if (this.Popularimeter.Value)
                {
                    for (var a = 0; a <= 5; a++)
                    {
                        yield return new InvocationComponent(
                            InvocationComponent.CATEGORY_LIBRARY, SET_RATING,
                            string.Format("{0} Stars", a),
                            path: "Set Rating", attributes:
                            InvocationComponent.ATTRIBUTE_SEPARATOR
                        );
                    }
                }
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
            return this.PlaylistManager.Add(this.LibraryManager.SelectedItem, clear);
        }

        private Task SetRating(string name)
        {
            var rating = default(byte);
            if (string.IsNullOrEmpty(name) || !byte.TryParse(name.Split(' ').FirstOrDefault(), out rating))
            {
#if NET40
                return TaskEx.FromResult(false);
#else
                return Task.CompletedTask;
#endif
            }
            return this.LibraryManager.SetRating(this.LibraryManager.SelectedItem, rating);
        }
    }
}
