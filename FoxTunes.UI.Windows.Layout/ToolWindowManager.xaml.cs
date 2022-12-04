using System.Windows;
using System.Windows.Controls;

namespace FoxTunes
{
    /// <summary>
    /// Interaction logic for ToolWindowManager.xaml
    /// </summary>
    public partial class ToolWindowManager : UserControl
    {
        public ToolWindowManager()
        {
            this.InitializeComponent();
        }

        protected virtual async void OnEditLayoutXMLClick(object sender, RoutedEventArgs e)
        {
            var viewModel = this.TryFindResource("ViewModel") as global::FoxTunes.ViewModel.ToolWindowManager;
            if (viewModel == null)
            {
                return;
            }
            var toolWindow = viewModel.SelectedWindow;
            if (toolWindow == null)
            {
                return;
            }
            var layoutEditor = new LayoutEditor();
            layoutEditor.Component = toolWindow.Component;
            if (!await Windows.ShowDialog(Core.Instance, Strings.ToolWindowManager_EditLayoutXML, layoutEditor).ConfigureAwait(false))
            {
                return;
            }
            await Windows.Invoke(() => toolWindow.Component = layoutEditor.Component).ConfigureAwait(false);
        }
    }
}
