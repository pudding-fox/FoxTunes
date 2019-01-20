using System;
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

        public override string ArtworkPlaceholder
        {
            get
            {
                return string.Format("/{0};Component/Resources/System_Artwork.png", typeof(ExpressionDarkTheme).Assembly.FullName);
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
