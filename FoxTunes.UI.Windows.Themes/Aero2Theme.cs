using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace FoxTunes
{
    [WindowsUserInterfaceDependency]
    public class Aero2Theme : ThemeBase
    {
        public const string ID = "FB90B62E-839D-43B3-B9C2-DBFF498BFE82";

        public Aero2Theme() : base(ID, Strings.Aero2Theme_Name, Strings.Aero2Theme_Description, GetColorPalettes())
        {

        }

        public override int CornerRadius
        {
            get
            {
                return 5;
            }
        }

        public override ResourceDictionary GetResourceDictionary()
        {
            var resourceDictionary = new ResourceDictionary();
            resourceDictionary.MergedDictionaries.Add(new ResourceDictionary()
            {
                Source = new Uri("/PresentationFramework.Aero2,Version=4.0.0.0,Culture=neutral,PublicKeyToken=31bf3856ad364e35,processorArchitecture=MSIL;component/themes/Aero2.NormalColor.xaml", UriKind.Relative)
            });
            resourceDictionary.MergedDictionaries.Add(new ResourceDictionary()
            {
                Source = new Uri("/FoxTunes.UI.Windows.Themes;component/Themes/Aero2.xaml", UriKind.Relative)
            });
            return resourceDictionary;
        }

        public override Stream GetArtworkPlaceholder()
        {
            return typeof(SystemTheme).Assembly.GetManifestResourceStream("FoxTunes.UI.Windows.Themes.Images.System_Artwork.png");
        }

        public static IEnumerable<IColorPalette> GetColorPalettes()
        {
            return new[]
            {
                new ColorPalette(
                    ID + "_AAAA",
                    ColorPaletteRole.Visualization,
                    Strings.Aero2Theme_ColorPalette_Default_Name,
                    Strings.Aero2Theme_ColorPalette_Default_Description,
                    Resources.Blue
                ),
                new ColorPalette(
                    ID + "_BBBB",
                    ColorPaletteRole.Visualization,
                    Strings.Aero2Theme_ColorPalette_Gradient1_Name,
                    Strings.Aero2Theme_ColorPalette_Gradient1_Description,
                    Resources.Transparent_Blue
                ),
            };
        }
    }
}
