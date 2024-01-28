using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.UserInterface)]
    public class PlaylistColumnManager : StandardComponent
    {
        public IEnumerable<UIPlaylistColumnProvider> GetProviders()
        {
            var components = new List<UIPlaylistColumnProvider>();
            foreach (var type in ComponentScanner.Instance.GetComponents(typeof(IUIPlaylistColumnProvider)))
            {
                var attribute = default(UIPlaylistColumnProviderAttribute);
                if (!type.HasCustomAttribute<UIPlaylistColumnProviderAttribute>(false, out attribute))
                {
                    attribute = new UIPlaylistColumnProviderAttribute(type.AssemblyQualifiedName, type.Name);
                }
                components.Add(new UIPlaylistColumnProvider(attribute, type));
            }
            return components;
        }

        public IUIPlaylistColumnProvider GetProvider(string typeName)
        {
            var type = default(Type);
            try
            {
                type = Type.GetType(typeName);
                if (type == null)
                {
                    Logger.Write(typeof(PlaylistGridViewColumnFactory), LogLevel.Warn, "Failed to locate playlist column proider \"{0}\": No such type.", typeName);
                    return null;
                }
            }
            catch (Exception e)
            {
                Logger.Write(typeof(PlaylistGridViewColumnFactory), LogLevel.Warn, "Failed to locate playlist column proider \"{0}\": {1}", typeName, e.Message);
                return null;
            }
            return this.GetProvider(type);
        }

        public IUIPlaylistColumnProvider GetProvider(Type type)
        {
            var provider = ComponentRegistry.Instance.GetComponent(type) as IUIPlaylistColumnProvider;
            if (provider == null)
            {
                Logger.Write(typeof(PlaylistGridViewColumnFactory), LogLevel.Warn, "Failed to locate playlist column proider \"{0}\": No such registration.", type.FullName);
            }
            return provider;
        }
    }
}
