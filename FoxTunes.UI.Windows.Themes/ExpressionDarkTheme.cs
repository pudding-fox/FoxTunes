using System;
using System.IO;
using System.Windows;

namespace FoxTunes
{
    [Component(ID, ComponentSlots.None)]
    [ComponentDependency(Slot = ComponentSlots.UserInterface)]
    public class ExpressionDarkTheme : ThemeBase
    {
        public const string ID = "3E9EFE8C-5245-4F8B-97D1-EB47CC70E373";

        public ExpressionDarkTheme()
            : base(ID, "ExpressionDark")
        {
            this.ResourceDictionary = new Lazy<ResourceDictionary>(() => new ResourceDictionary()
            {
                Source = new Uri("/FoxTunes.UI.Windows.Themes;component/Themes/ExpressionDark.xaml", UriKind.Relative)
            });
        }

        public Lazy<ResourceDictionary> ResourceDictionary { get; private set; }

        public override Stream ArtworkPlaceholder
        {
            get
            {
                return typeof(ExpressionDarkTheme).Assembly.GetManifestResourceStream("FoxTunes.UI.Windows.Themes.Images.ExpressionDark_Artwork.png");
            }
        }

        public override void Enable()
        {
            if (this.ResourceDictionary.Value != null)
            {
                Application.Current.Resources.MergedDictionaries.Add(this.ResourceDictionary.Value);
            }
        }

        public override void Disable()
        {
            if (this.ResourceDictionary.Value != null)
            {
                Application.Current.Resources.MergedDictionaries.Remove(this.ResourceDictionary.Value);
            }
        }
    }
}
