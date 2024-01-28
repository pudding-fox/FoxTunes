using System.Windows;

#if NET40
using Microsoft.Windows.Shell;
#else
using System.Windows.Shell;
#endif

namespace FoxTunes
{
    /// <summary>
    /// Interaction logic for ToolWindowManagerWindow.xaml
    /// </summary>
    public partial class ToolWindowManagerWindow : Window
    {
        public ToolWindowManagerWindow()
        {
            this.SetValue(WindowChrome.WindowChromeProperty, new WindowChrome()
            {
                CaptionHeight = 30,
                ResizeBorderThickness = new Thickness(5)
            });
            this.InitializeComponent();
        }
    }
}
