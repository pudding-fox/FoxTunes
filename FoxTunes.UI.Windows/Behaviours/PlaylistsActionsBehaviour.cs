using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.UserInterface)]
    public class PlaylistsActionsBehaviour : StandardBehaviour, IInvocableComponent
    {
        public const string ADD_PLAYLIST = "QQQQ";

        public const string REMOVE_PLAYLIST = "RRRR";

        public IEnumerable<IInvocationComponent> Invocations
        {
            get
            {
                yield return new InvocationComponent(InvocationComponent.CATEGORY_PLAYLISTS, ADD_PLAYLIST, "Add Playlist");
                yield return new InvocationComponent(InvocationComponent.CATEGORY_PLAYLISTS, REMOVE_PLAYLIST, "Remove Playlist");
            }
        }

        public Task InvokeAsync(IInvocationComponent component)
        {
            switch (component.Id)
            {
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }
    }
}
