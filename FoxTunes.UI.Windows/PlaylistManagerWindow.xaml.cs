using System.Windows;

#if NET40
using Microsoft.Windows.Shell;
#else
using System.Windows.Shell;
#endif

namespace FoxTunes
{
    /// <summary>
    /// Interaction logic for PlaylistManagerWindow.xaml
    /// </summary>
    public partial class PlaylistManagerWindow : Window
    {
        public PlaylistManagerWindow()
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
