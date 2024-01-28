using System;
using System.IO;
using System.Windows;

namespace FoxTunes
{
    public class SystemTheme : ThemeBase
    {
        public SystemTheme()
            : base("D4EBB53F-BF59-4D61-99E1-D6D52926AE3F", "System")
        {
            this.ResourceDictionary = new ResourceDictionary()
            {
                Source = new Uri("/FoxTunes.UI.Windows.Themes;component/Themes/System.xaml", UriKind.Relative)
            };
        }

        public ResourceDictionary ResourceDictionary { get; private set; }

        public override Stream ArtworkPlaceholder
        {
            get
            {
                return typeof(SystemTheme).Assembly.GetManifestResourceStream("FoxTunes.UI.Windows.Themes.Resources.System_Artwork.png");
            }
        }

        public override void Enable()
        {
            Application.Current.Resources.MergedDictionaries.Add(this.ResourceDictionary);
        }

        public override void Disable()
        {
            Application.Current.Resources.MergedDictionaries.Remove(this.ResourceDictionary);
        }
    }
}
