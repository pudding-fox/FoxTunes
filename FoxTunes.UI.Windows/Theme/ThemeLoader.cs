using FoxTunes.Interfaces;
using System;
using System.Threading.Tasks;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.UserInterface)]
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
            if (this.Theme != null)
            {
                this.Theme.Disable();
            }
            if (this.ThemeChanging != null)
            {
                this.ThemeChanging(this, EventArgs.Empty);
            }
            this.OnPropertyChanging("Theme");
        }

        public event EventHandler ThemeChanging;

        protected virtual void OnThemeChanged()
        {
            if (this.Theme != null)
            {
                this.Theme.Enable();
            }
            if (this.ThemeChanged != null)
            {
                this.ThemeChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Theme");
        }

        public event EventHandler ThemeChanged;

        public IConfiguration Configuration { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Configuration = core.Components.Configuration;
            this.Configuration.GetElement<SelectionConfigurationElement>(
                WindowsUserInterfaceConfiguration.SECTION,
                WindowsUserInterfaceConfiguration.THEME_ELEMENT
            ).ConnectValue(value => this.Theme = WindowsUserInterfaceConfiguration.GetTheme(value));
            base.InitializeComponent(core);
        }
    }
}
