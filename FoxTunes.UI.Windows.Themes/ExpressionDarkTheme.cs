using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace FoxTunes
{
    [Component(ID)]
    [WindowsUserInterfaceDependency]
    public class ExpressionDarkTheme : ThemeBase, IConfigurableComponent
    {
        public const string ID = "3E9EFE8C-5245-4F8B-97D1-EB47CC70E373";

        public ExpressionDarkTheme()
            : base(ID, Strings.ExpressionDarkTheme_Name, Strings.ExpressionDarkTheme_Description, GetColorPalettes())
        {

        }

        public override void InitializeComponent(ICore core)
        {
            base.InitializeComponent(core);
            this.ConnectSetting(
                ExpressionDarkThemeConfiguration.SECTION,
                ExpressionDarkThemeConfiguration.LIST_ROW_SHADING,
                () => new ResourceDictionary()
                {
                    Source = new Uri("/FoxTunes.UI.Windows.Themes;component/Themes/ExpressionDark_ListRowShading.xaml", UriKind.Relative)
                }
            );
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
            return new ResourceDictionary()
            {
                Source = new Uri("/FoxTunes.UI.Windows.Themes;component/Themes/ExpressionDark.xaml", UriKind.Relative)
            };
        }

        public override Stream GetArtworkPlaceholder()
        {
            return typeof(ExpressionDarkTheme).Assembly.GetManifestResourceStream("FoxTunes.UI.Windows.Themes.Images.ExpressionDark_Artwork.png");
        }

        public static IEnumerable<IColorPalette> GetColorPalettes()
        {
            return new[]
            {
                new ColorPalette(
                    ID + "_AAAA",
                    ColorPaletteRole.Visualization,
                    Strings.ExpressionDarkTheme_ColorPalette_Default_Name,
                    Strings.ExpressionDarkTheme_ColorPalette_Default_Description,
                    Resources.White
                ),
                new ColorPalette(
                    ID + "_BBBB",
                    ColorPaletteRole.Visualization,
                    Strings.ExpressionDarkTheme_ColorPalette_Gradient1_Name,
                    Strings.ExpressionDarkTheme_ColorPalette_Gradient1_Description,
                    Resources.Transparent_White
                ),
            };
        }

        public IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return ExpressionDarkThemeConfiguration.GetConfigurationSections();
        }
    }
}
