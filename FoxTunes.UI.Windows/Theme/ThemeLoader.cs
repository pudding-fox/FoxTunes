using FoxTunes.Interfaces;
using System;
using System.Linq;
using System.Windows;

namespace FoxTunes.Theme
{
    public class ThemeLoader : StandardComponent, IThemeLoader
    {
        public IConfiguration Configuration { get; private set; }

        public IThemeRegistry ThemeRegistry { get; private set; }

        public SelectionConfigurationElement Element { get; private set; }

        private Application _Application { get; set; }

        public Application Application
        {
            get
            {
                return this._Application;
            }
            set
            {
                this._Application = value;
                this.OnApplicationChanged();
            }
        }

        protected virtual void OnApplicationChanged()
        {
            this.Apply();
            if (this.ApplicationChanged != null)
            {
                this.ApplicationChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Application");
        }

        public event EventHandler ApplicationChanged = delegate { };

        public override void InitializeComponent(ICore core)
        {
            this.Configuration = core.Components.Configuration;
            this.ThemeRegistry = ComponentRegistry.Instance.GetComponent<IThemeRegistry>();
            this.Element = this.Configuration.GetElement<SelectionConfigurationElement>(
                WindowsUserInterfaceConfiguration.APPEARANCE_SECTION,
                WindowsUserInterfaceConfiguration.THEME_ELEMENT
            );
            this.Element.SelectedOptionChanged += this.Element_SelectedOptionChanged;
            base.InitializeComponent(core);
        }

        protected virtual void Element_SelectedOptionChanged(object sender, EventArgs e)
        {
            this.Apply();
        }


        public ITheme Theme
        {
            get
            {
                if (this.Element.SelectedOption == null)
                {
                    return this.ThemeRegistry.Themes.FirstOrDefault();
                }
                return this.ThemeRegistry.GetTheme(this.Element.SelectedOption.Id);
            }
        }

        private void Apply()
        {
            if (this.Theme == null)
            {
                return;
            }
            this.Theme.Apply(this.Application);
        }
    }
}
