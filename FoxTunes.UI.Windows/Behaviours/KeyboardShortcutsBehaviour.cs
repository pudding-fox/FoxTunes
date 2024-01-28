using FoxTunes.Interfaces;

namespace FoxTunes
{
    public class KeyboardShortcutsBehaviour : StandardBehaviour
    {
        public IInputManager InputManager { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.InputManager = core.Managers.Input;
            this.Configuration = core.Components.Configuration;
            this.Configuration.GetElement<BooleanConfigurationElement>(
                WindowsUserInterfaceConfiguration.KEYBOARD_SHORTCUTS_SECTION,
                WindowsUserInterfaceConfiguration.KEYBOARD_SHORTCUTS_ENABLED_ELEMENT
            ).ConnectValue<bool>(value => this.InputManager.Enabled = value);
            base.InitializeComponent(core);
        }
    }
}
