using FoxTunes.Interfaces;
using System;
using System.Threading.Tasks;

namespace FoxTunes
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
        }

        protected async Task SetTheme(ITheme value)
        {
            await this.OnThemeChanging();
            this._Theme = value;
            await this.OnThemeChanged();
        }

        protected virtual async Task OnThemeChanging()
        {
            if (this.Theme != null)
            {
                this.Theme.Disable();
            }
            if (this.ThemeChanging != null)
            {
                var e = new AsyncEventArgs();
                this.ThemeChanging(this, e);
                await e.Complete();
            }
            this.OnPropertyChanging("Theme");
        }

        public event AsyncEventHandler ThemeChanging;

        protected virtual async Task OnThemeChanged()
        {
            if (this.Theme != null)
            {
                this.Theme.Enable();
            }
            if (this.ThemeChanged != null)
            {
                var e = new AsyncEventArgs();
                this.ThemeChanged(this, e);
                await e.Complete();
            }
            this.OnPropertyChanged("Theme");
        }

        public event AsyncEventHandler ThemeChanged;

        public IConfiguration Configuration { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Configuration = core.Components.Configuration;
            this.Configuration.GetElement<SelectionConfigurationElement>(
                WindowsUserInterfaceConfiguration.SECTION,
                WindowsUserInterfaceConfiguration.THEME_ELEMENT
            ).ConnectValue(async value => await this.SetTheme(WindowsUserInterfaceConfiguration.GetTheme(value)));
            base.InitializeComponent(core);
        }
    }
}
