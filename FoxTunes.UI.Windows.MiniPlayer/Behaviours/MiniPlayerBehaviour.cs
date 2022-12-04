using FoxTunes.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoxTunes
{
    [ComponentPriority(ComponentPriorityAttribute.LOW)]
    [WindowsUserInterfaceDependency]
    public class MiniPlayerBehaviour : StandardBehaviour, IInvocableComponent, IConfigurableComponent
    {
        public const string TOPMOST = "AAAA";

        public const string SHOW_ARTWORK = "BBBB";

        public const string SHOW_PLAYLIST = "CCCC";

        public const string QUIT = "ZZZZ";

        static MiniPlayerBehaviour()
        {
            Windows.Registrations.Add(new Windows.WindowRegistration(MiniWindow.ID, MiniWindow.ROLE, () => new MiniWindow()));
        }

        public ThemeLoader ThemeLoader { get; private set; }

        public IKeyBindingsBehaviour KeyBindingsBehaviour { get; private set; }

        public ICore Core { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public BooleanConfigurationElement Enabled { get; private set; }

        public BooleanConfigurationElement Topmost { get; private set; }

        public BooleanConfigurationElement ShowArtwork { get; private set; }

        public BooleanConfigurationElement ShowPlaylist { get; private set; }

        protected virtual Task Enable()
        {
            return Windows.Invoke(() =>
            {
                Windows.Registrations.Hide(MainWindow.ID);
                Windows.Registrations.Show(MiniWindow.ID);
            });
        }

        protected virtual Task Disable()
        {
            return Windows.Invoke(() =>
            {
                Windows.Registrations.Hide(MiniWindow.ID);
                Windows.Registrations.Show(MainWindow.ID);
            });
        }

        public override void InitializeComponent(ICore core)
        {
            this.ThemeLoader = ComponentRegistry.Instance.GetComponent<ThemeLoader>();
            this.KeyBindingsBehaviour = ComponentRegistry.Instance.GetComponent<IKeyBindingsBehaviour>();
            this.Core = core;
            this.Configuration = core.Components.Configuration;
            this.Enabled = this.Configuration.GetElement<BooleanConfigurationElement>(
                MiniPlayerBehaviourConfiguration.SECTION,
                MiniPlayerBehaviourConfiguration.ENABLED_ELEMENT
            );
            this.Enabled.ConnectValue(async value =>
            {
                //TODO: This code is actually responsible for creating the main application window, 
                //TODO: It should really be WindowsUserInterface.Show().
                //Ensure resources are loaded.
                await ThemeLoader.EnsureTheme().ConfigureAwait(false);
                if (value)
                {
                    await this.Enable().ConfigureAwait(false);
                }
                else
                {
                    await this.Disable().ConfigureAwait(false);
                }
                if (this.IsInitialized)
                {
                    this.Configuration.Save();
                }
            });
            this.Topmost = this.Configuration.GetElement<BooleanConfigurationElement>(
                MiniPlayerBehaviourConfiguration.SECTION,
                MiniPlayerBehaviourConfiguration.TOPMOST_ELEMENT
            );
            this.ShowArtwork = this.Configuration.GetElement<BooleanConfigurationElement>(
                MiniPlayerBehaviourConfiguration.SECTION,
                MiniPlayerBehaviourConfiguration.SHOW_ARTWORK_ELEMENT
            );
            this.ShowPlaylist = this.Configuration.GetElement<BooleanConfigurationElement>(
                MiniPlayerBehaviourConfiguration.SECTION,
                MiniPlayerBehaviourConfiguration.SHOW_PLAYLIST_ELEMENT
            );
            base.InitializeComponent(core);
        }

        public IEnumerable<string> InvocationCategories
        {
            get
            {
                yield return InvocationComponent.CATEGORY_MINI_PLAYER;
            }
        }

        public IEnumerable<IInvocationComponent> Invocations
        {
            get
            {
                if (this.Enabled.Value)
                {
                    yield return new InvocationComponent(InvocationComponent.CATEGORY_MINI_PLAYER, TOPMOST, this.Topmost.Name, attributes: this.Topmost.Value ? InvocationComponent.ATTRIBUTE_SELECTED : InvocationComponent.ATTRIBUTE_NONE);
                    yield return new InvocationComponent(InvocationComponent.CATEGORY_MINI_PLAYER, SHOW_ARTWORK, this.ShowArtwork.Name, attributes: this.ShowArtwork.Value ? InvocationComponent.ATTRIBUTE_SELECTED : InvocationComponent.ATTRIBUTE_NONE);
                    yield return new InvocationComponent(InvocationComponent.CATEGORY_MINI_PLAYER, SHOW_PLAYLIST, this.ShowPlaylist.Name, attributes: this.ShowPlaylist.Value ? InvocationComponent.ATTRIBUTE_SELECTED : InvocationComponent.ATTRIBUTE_NONE);
                    yield return new InvocationComponent(InvocationComponent.CATEGORY_MINI_PLAYER, QUIT, Strings.MiniPlayerBehaviour_Quit, attributes: InvocationComponent.ATTRIBUTE_SEPARATOR);
                }
            }
        }

        public Task InvokeAsync(IInvocationComponent component)
        {
            switch (component.Id)
            {
                case TOPMOST:
                    this.Topmost.Toggle();
                    this.Configuration.Save();
                    break;
                case SHOW_ARTWORK:
                    this.ShowArtwork.Toggle();
                    this.Configuration.Save();
                    break;
                case SHOW_PLAYLIST:
                    this.ShowPlaylist.Toggle();
                    this.Configuration.Save();
                    break;
                case QUIT:
                    return Windows.Shutdown();
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        public IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return MiniPlayerBehaviourConfiguration.GetConfigurationSections();
        }
    }
}
