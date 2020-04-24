using System;
using System.Collections.Generic;
using System.Linq;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.UserInterface)]
    public class PlaylistColumnProviderManager : StandardComponent
    {
        public PlaylistColumnProviderManager()
        {
            this._Providers = new Lazy<IDictionary<string, IUIPlaylistColumnProvider>>(
                () => ComponentRegistry.Instance.GetComponents<IUIPlaylistColumnProvider>().ToDictionary(
                    component => component.GetType().AssemblyQualifiedName
                )
            );
        }

        public Lazy<IDictionary<string, IUIPlaylistColumnProvider>> _Providers { get; private set; }

        public IEnumerable<IUIPlaylistColumnProvider> Providers
        {
            get
            {
                return this._Providers.Value.Values;
            }
        }

        public IUIPlaylistColumnProvider GetProvider(string plugin)
        {
            var provider = default(IUIPlaylistColumnProvider);
            if (!this._Providers.Value.TryGetValue(plugin, out provider))
            {
                return null;
            }
            return provider;
        }
    }
}
