using FoxTunes.Interfaces;
using System.Windows;

namespace FoxTunes.ViewModel
{
    public class FirstRun : ViewModelBase
    {
        public IConfiguration Configuration { get; private set; }

        public SelectionConfigurationElement Theme { get; private set; }

        public SelectionConfigurationElement Layout { get; private set; }

        protected override void InitializeComponent(ICore core)
        {
            this.Configuration = core.Components.Configuration;
            this.Theme = this.Configuration.GetElement<SelectionConfigurationElement>(
                WindowsUserInterfaceConfiguration.SECTION,
                WindowsUserInterfaceConfiguration.THEME_ELEMENT
            );
            this.Layout = this.Configuration.GetElement<SelectionConfigurationElement>(
                WindowsUserInterfaceConfiguration.SECTION,
                WindowsUserInterfaceConfiguration.LAYOUT_ELEMENT
            );
            base.InitializeComponent(core);
        }

        protected override Freezable CreateInstanceCore()
        {
            return new FirstRun();
        }
    }
}
