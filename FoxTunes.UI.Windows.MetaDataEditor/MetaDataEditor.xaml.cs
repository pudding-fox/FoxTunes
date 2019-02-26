using System.Windows;
using System.Windows.Input;

namespace FoxTunes
{
    /// <summary>
    /// Interaction logic for MetaDataEditor.xaml
    /// </summary>
    [UIComponent("3D1B2C8E-DEAF-4689-B0C2-33AB3FD8F061", UIComponentSlots.BOTTOM_RIGHT, "Meta Data")]
    public partial class MetaDataEditor : UIComponentBase
    {
        public MetaDataEditor()
        {
            this.InitializeComponent();
        }

        protected virtual void OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var metaDataEntry = (e.OriginalSource as FrameworkElement).DataContext as global::FoxTunes.ViewModel.MetaDataEntry;
            if (metaDataEntry == null || !metaDataEntry.BrowseCommand.CanExecute(null))
            {
                return;
            }
            metaDataEntry.BrowseCommand.Execute(null);
        }
    }
}
