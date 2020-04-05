using FoxTunes.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.UserInterface)]
    public class MetaDataEditorBehaviour : StandardBehaviour, IInvocableComponent
    {
        public const string EDIT_METADATA = "LLLL";

        public ILibraryManager LibraryManager { get; private set; }

        public IPlaylistManager PlaylistManager { get; private set; }

        public ISignalEmitter SignalEmitter { get; private set; }

        public bool Enabled
        {
            get
            {
                return LayoutManager.Instance.IsComponentActive(typeof(global::FoxTunes.MetaDataEditor)) && LayoutManager.Instance.IsComponentValid(typeof(global::FoxTunes.MetaDataEditor));
            }
        }

        public override void InitializeComponent(ICore core)
        {
            this.LibraryManager = core.Managers.Library;
            this.PlaylistManager = core.Managers.Playlist;
            this.SignalEmitter = core.Components.SignalEmitter;
            base.InitializeComponent(core);
        }

        public IEnumerable<IInvocationComponent> Invocations
        {
            get
            {
                if (this.Enabled)
                {
                    if (this.LibraryManager.SelectedItem != null)
                    {
                        yield return new InvocationComponent(InvocationComponent.CATEGORY_LIBRARY, EDIT_METADATA, "Tag");
                    }
                    if (this.PlaylistManager.SelectedItems != null && this.PlaylistManager.SelectedItems.Any())
                    {
                        yield return new InvocationComponent(InvocationComponent.CATEGORY_PLAYLIST, EDIT_METADATA, "Tag");
                    }
                }
            }
        }

        public Task InvokeAsync(IInvocationComponent component)
        {
            switch (component.Id)
            {
                case EDIT_METADATA:
                    return this.SignalEmitter.Send(new Signal(this, CommonSignals.PluginInvocation, component));
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }
    }
}
