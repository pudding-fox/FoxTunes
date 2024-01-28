using System;
using System.IO;
using System.Windows;

namespace FoxTunes
{
    [Component(ID, ComponentSlots.None)]
    [ComponentDependency(Slot = ComponentSlots.UserInterface)]
    public class SystemTheme : ThemeBase
    {
        public const string ID = "392B2FEE-B1E5-4776-AF3A-C28260EE8E81";

        public SystemTheme()
            : base(ID, "System")
        {
            this.ResourceDictionary = new Lazy<ResourceDictionary>(() => new ResourceDictionary()
            {
                Source = new Uri("/FoxTunes.UI.Windows.Themes;component/Themes/System.xaml", UriKind.Relative)
            });
        }

        public Lazy<ResourceDictionary> ResourceDictionary { get; private set; }

        public override Stream ArtworkPlaceholder
        {
            get
            {
                return typeof(SystemTheme).Assembly.GetManifestResourceStream("FoxTunes.UI.Windows.Themes.Images.System_Artwork.png");
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
