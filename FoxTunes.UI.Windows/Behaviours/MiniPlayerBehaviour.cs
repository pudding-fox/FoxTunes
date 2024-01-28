using FoxTunes.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class MiniPlayerBehaviour : StandardBehaviour, IInvocableComponent, IConfigurableComponent
    {
        public const string SHOW_ARTWORK = "AAAA";

        public const string SHOW_PLAYLIST = "BBBB";

        public const string QUIT = "ZZZZ";

        public ICore Core { get; private set; }

        public IUserInterface UserInterface { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public BooleanConfigurationElement Enabled { get; private set; }

        public BooleanConfigurationElement Topmost { get; private set; }

        public BooleanConfigurationElement ShowArtwork { get; private set; }

        public BooleanConfigurationElement ShowPlaylist { get; private set; }

        public TextConfigurationElement ScalingFactor { get; private set; }

        protected virtual Task Enable()
        {
            return Windows.Invoke(() =>
            {
                if (Windows.IsMainWindowCreated)
                {
                    Windows.MainWindow.Hide();
                }
                if (Windows.MiniWindow.DataContext == null)
                {
                    Windows.MiniWindow.DataContext = this.Core;
                }
                Windows.MiniWindow.Topmost = this.Topmost.Value;
                Windows.MiniWindow.Show();
                Windows.MiniWindow.BringToFront();
                Windows.ActiveWindow = Windows.MiniWindow;
            });
        }

        protected virtual Task Disable()
        {
            return Windows.Invoke(() =>
            {
                if (Windows.IsMiniWindowCreated)
                {
                    Windows.MiniWindow.Hide();
                }
                if (Windows.MainWindow.DataContext == null)
                {
                    Windows.MainWindow.DataContext = this.Core;
                }
                Windows.MainWindow.Show();
                Windows.MainWindow.BringToFront();
                Windows.ActiveWindow = Windows.MainWindow;
            });
        }

        public override void InitializeComponent(ICore core)
        {
            this.Core = core;
            this.UserInterface = core.Components.UserInterface;
            this.Configuration = core.Components.Configuration;
            this.Topmost = this.Configuration.GetElement<BooleanConfigurationElement>(
                MiniPlayerBehaviourConfiguration.SECTION,
                MiniPlayerBehaviourConfiguration.TOPMOST_ELEMENT
            );
            this.Topmost.ConnectValue<bool>(value =>
            {
                if (Windows.IsMiniWindowCreated)
                {
                    Windows.MiniWindow.Topmost = value;
                }
            });
            this.Enabled = this.Configuration.GetElement<BooleanConfigurationElement>(
                MiniPlayerBehaviourConfiguration.SECTION,
                MiniPlayerBehaviourConfiguration.ENABLED_ELEMENT
            );
            this.Enabled.ConnectValue<bool>(async value =>
            {
                if (value)
                {
                    await this.Enable();
                }
                else
                {
                    await this.Disable();
                }
                if (this.IsInitialized)
                {
                    await Task.Run(() => this.Configuration.Save());
                }
            });
            this.ShowArtwork = this.Configuration.GetElement<BooleanConfigurationElement>(
                MiniPlayerBehaviourConfiguration.SECTION,
                MiniPlayerBehaviourConfiguration.SHOW_ARTWORK_ELEMENT
            );
            this.ShowPlaylist = this.Configuration.GetElement<BooleanConfigurationElement>(
                MiniPlayerBehaviourConfiguration.SECTION,
                MiniPlayerBehaviourConfiguration.SHOW_PLAYLIST_ELEMENT
            );
            this.ScalingFactor = this.Configuration.GetElement<TextConfigurationElement>(
              WindowsUserInterfaceConfiguration.SECTION,
              WindowsUserInterfaceConfiguration.UI_SCALING_ELEMENT
            );
            this.ScalingFactor.ConnectValue<string>(value =>
            {
                if (this.IsInitialized && Windows.IsMiniWindowCreated)
                {
                    this.UserInterface.Restart();
                }
            });
            base.InitializeComponent(core);
        }

        public IEnumerable<IInvocationComponent> Invocations
        {
            get
            {
                if (this.Enabled.Value)
                {
                    yield return new InvocationComponent(InvocationComponent.CATEGORY_MINI_PLAYER, SHOW_ARTWORK, "Show Artwork", attributes: this.ShowArtwork.Value ? InvocationComponent.ATTRIBUTE_SELECTED : InvocationComponent.ATTRIBUTE_NONE);
                    yield return new InvocationComponent(InvocationComponent.CATEGORY_MINI_PLAYER, SHOW_PLAYLIST, "Show Playlist", attributes: this.ShowPlaylist.Value ? InvocationComponent.ATTRIBUTE_SELECTED : InvocationComponent.ATTRIBUTE_NONE);
                    yield return new InvocationComponent(InvocationComponent.CATEGORY_MINI_PLAYER, QUIT, "Quit", attributes: InvocationComponent.ATTRIBUTE_SEPARATOR);
                }
            }
        }

        public Task InvokeAsync(IInvocationComponent component)
        {
            switch (component.Id)
            {
                case SHOW_ARTWORK:
                    this.ShowArtwork.Toggle();
                    return Task.Run(() => this.Configuration.Save());
                case SHOW_PLAYLIST:
                    this.ShowPlaylist.Toggle();
                    return Task.Run(() => this.Configuration.Save());
                case QUIT:
                    Windows.Shutdown();
                    break;
            }
            return Task.CompletedTask;
        }

        public IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return MiniPlayerBehaviourConfiguration.GetConfigurationSections();
        }
    }
}
