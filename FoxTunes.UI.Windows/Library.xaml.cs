using FoxTunes.Interfaces;
using System.Windows.Controls;

namespace FoxTunes
{
    /// <summary>
    /// Interaction logic for Library.xaml
    /// </summary>
    public partial class Library : UserControl
    {
        public Library()
        {
            this.InitializeComponent();
            var configuration = ComponentRegistry.Instance.GetComponent<IConfiguration>();
            if (configuration != null)
            {
                this.ShowLibrary = configuration.GetElement<BooleanConfigurationElement>(
                    WindowsUserInterfaceConfiguration.SECTION,
                    WindowsUserInterfaceConfiguration.SHOW_LIBRARY_ELEMENT
                );
                this.ShowLibrary.ConnectValue(value => this.Refresh());
                this.LibraryView = configuration.GetElement<SelectionConfigurationElement>(
                    WindowsUserInterfaceConfiguration.SECTION,
                    WindowsUserInterfaceConfiguration.LIBRARY_VIEW_ELEMENT
                );
                this.LibraryView.ConnectValue(value => this.Refresh());
            }
        }

        public BooleanConfigurationElement ShowLibrary { get; private set; }

        public SelectionConfigurationElement LibraryView { get; private set; }

        public void Refresh()
        {
            if (this.ShowLibrary != null && !this.ShowLibrary.Value)
            {
                this.Content = null;
            }
            else if (this.LibraryView != null)
            {
                this.Content = WindowsUserInterfaceConfiguration.GetLibraryView(this.LibraryView.Value);
            }
        }
    }
}
