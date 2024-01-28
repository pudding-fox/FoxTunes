using System;
using System.Windows;

namespace FoxTunes
{
    public class ExpressionDarkTheme : ThemeBase
    {
        public ExpressionDarkTheme()
            : base("3E9EFE8C-5245-4F8B-97D1-EB47CC70E373", "ExpressionDark")
        {
            this.ResourceDictionary = new ResourceDictionary()
            {
                Source = new Uri("/FoxTunes.UI.Windows.Themes;component/Themes/ExpressionDark.xaml", UriKind.Relative)
            };
        }

        public ResourceDictionary ResourceDictionary { get; private set; }

        public override string ArtworkPlaceholder
        {
            get
            {
                return string.Format("/{0};Component/Resources/ExpressionDark_Artwork.png", typeof(ExpressionDarkTheme).Assembly.GetName().Name);
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
