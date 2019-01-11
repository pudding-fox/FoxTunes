using FoxTunes.Interfaces;

namespace FoxTunes.Theme
{
    public class ThemeLoader : StandardComponent
    {
        private ITheme _Theme { get; set; }

        public ITheme Theme
        {
            get
            {
                return this._Theme;
            }
            set
            {
                this.OnThemeChanging();
                this._Theme = value;
                this.OnThemeChanged();
            }
        }

        protected virtual void OnThemeChanging()
        {
            if (this.Theme == null)
            {
                return;
            }
            this.Theme.Disable();
        }

        protected virtual void OnThemeChanged()
        {
            if (this.Theme == null)
            {
                return;
            }
            this.Theme.Enable();
        }

        public IConfiguration Configuration { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Configuration = core.Components.Configuration;
            this.Configuration.GetElement<SelectionConfigurationElement>(
                WindowsUserInterfaceConfiguration.SECTION,
                WindowsUserInterfaceConfiguration.THEME_ELEMENT
            ).ConnectValue<string>(value => this.Theme = WindowsUserInterfaceConfiguration.GetTheme(value));
            base.InitializeComponent(core);
        }
    }
}
