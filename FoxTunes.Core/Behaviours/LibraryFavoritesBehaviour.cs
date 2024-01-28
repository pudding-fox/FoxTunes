using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.Output)]
    public class LibraryFavoritesBehaviour : StandardBehaviour, IInvocableComponent, IConfigurableComponent
    {
        public const string TOGGLE_FAVORITE = "AAAD";

        public const string TOGGLE_SHOW_FAVORITES = "AAAE";

        public ILibraryManager LibraryManager { get; private set; }

        public ILibraryHierarchyCache LibraryHierarchyCache { get; private set; }

        public ISignalEmitter SignalEmitter { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public BooleanConfigurationElement Enabled { get; private set; }

        public BooleanConfigurationElement ShowFavorites { get; private set; }

        public bool IsFavorite
        {
            get
            {
                var libraryHierarchyNode = this.LibraryManager.SelectedItem;
                if (libraryHierarchyNode == null)
                {
                    return false;
                }
                //TODO: Bad .Result
                return this.LibraryManager.GetIsFavorite(libraryHierarchyNode).Result;
            }
        }

        public override void InitializeComponent(ICore core)
        {
            this.LibraryManager = core.Managers.Library;
            this.LibraryHierarchyCache = core.Components.LibraryHierarchyCache;
            this.SignalEmitter = core.Components.SignalEmitter;
            this.Configuration = core.Components.Configuration;
            this.Enabled = this.Configuration.GetElement<BooleanConfigurationElement>(
                LibraryFavoritesBehaviourConfiguration.SECTION,
                LibraryFavoritesBehaviourConfiguration.ENABLED_ELEMENT
            );
            this.ShowFavorites = this.Configuration.GetElement<BooleanConfigurationElement>(
                LibraryFavoritesBehaviourConfiguration.SECTION,
                LibraryFavoritesBehaviourConfiguration.SHOW_FAVORITES_ELEMENT
            );
            this.ShowFavorites.ConnectValue(
                async value => await this.SignalEmitter.Send(new Signal(this, CommonSignals.HierarchiesUpdated, CommonSignalFlags.SOFT))
.ConfigureAwait(false));
            base.InitializeComponent(core);
        }

        public IEnumerable<IInvocationComponent> Invocations
        {
            get
            {
                if (this.Enabled.Value)
                {
                    yield return new InvocationComponent(
                        InvocationComponent.CATEGORY_LIBRARY,
                        TOGGLE_FAVORITE,
                        "Mark As Favorite",
                        path: "Favorites",
                        attributes: (byte)(InvocationComponent.ATTRIBUTE_SEPARATOR | (this.IsFavorite ? InvocationComponent.ATTRIBUTE_SELECTED : InvocationComponent.ATTRIBUTE_NONE))
                    );
                    yield return new InvocationComponent(
                        InvocationComponent.CATEGORY_LIBRARY,
                        TOGGLE_SHOW_FAVORITES,
                        "Show Only Favorites",
                        path: "Favorites",
                        attributes: this.ShowFavorites.Value ? InvocationComponent.ATTRIBUTE_SELECTED : InvocationComponent.ATTRIBUTE_NONE
                    );
                }
            }
        }

        public async Task InvokeAsync(IInvocationComponent component)
        {
            switch (component.Id)
            {
                case TOGGLE_FAVORITE:
                    var libraryHierarchyNode = this.LibraryManager.SelectedItem;
                    if (libraryHierarchyNode != null)
                    {
                        var isFavorite = await this.LibraryManager.GetIsFavorite(libraryHierarchyNode).ConfigureAwait(false);
                        if (isFavorite)
                        {
                            Logger.Write(this, LogLevel.Debug, "Marking tracks for library hierarchy node as favorite: {0} => {1}", libraryHierarchyNode.Id, libraryHierarchyNode.Value);
                            await this.LibraryManager.SetIsFavorite(libraryHierarchyNode, false).ConfigureAwait(false);
                        }
                        else
                        {
                            Logger.Write(this, LogLevel.Debug, "Marking tracks for library hierarchy node as normal: {0} => {1}", libraryHierarchyNode.Id, libraryHierarchyNode.Value);
                            await this.LibraryManager.SetIsFavorite(libraryHierarchyNode, true).ConfigureAwait(false);
                        }
                        Logger.Write(this, LogLevel.Debug, "Evicting associated library hierarchy cache entries.");
                        foreach (var key in this.LibraryHierarchyCache.Keys)
                        {
                            if (object.Equals(key.State[2], true))
                            {
                                this.LibraryHierarchyCache.Evict(key);
                            }
                        }
                        if (this.ShowFavorites.Value)
                        {
                            await this.SignalEmitter.Send(new Signal(this, CommonSignals.HierarchiesUpdated, CommonSignalFlags.SOFT)).ConfigureAwait(false);
                        }
                    }
                    break;
                case TOGGLE_SHOW_FAVORITES:
                    this.ShowFavorites.Toggle();
                    this.Configuration.Save();
                    break;
            }
        }

        public IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return LibraryFavoritesBehaviourConfiguration.GetConfigurationSections();
        }
    }
}
