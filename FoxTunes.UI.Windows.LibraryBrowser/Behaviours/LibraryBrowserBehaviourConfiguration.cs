using System.Collections.Generic;

namespace FoxTunes
{
    public static class LibraryBrowserBehaviourConfiguration
    {
        public const string SECTION = WindowsUserInterfaceConfiguration.SECTION;

        public const string LIBRARY_BROWSER_GRID_TILE_SIZE = "IIII0E5E-FB67-43D3-AE30-BF7571A1A8B1";

        public const string LIBRARY_BROWSER_LIST_TILE_SIZE = "IIIJ77C6-D258-4E3C-B68A-14978F71B1E6";

        public const int MIN_TILE_SIZE = 60;

        public const int MAX_TIME_SIZE = 300;

        public const int DEFAULT_GRID_TILE_SIZE = 160;

        public const int DEFAULT_LIST_TILE_SIZE = 80;

        public const string LIBRARY_BROWSER_TILE_IMAGE = "HHIIFCBE-2AA0-4B99-9C6E-E3F196540807";

        public const string LIBRARY_BROWSER_TILE_IMAGE_FIRST = "AAAA9419-8D7E-4150-8FB8-999D2C019941";

        public const string LIBRARY_BROWSER_TILE_IMAGE_COMPOUND = "BBBB32EB-B876-4F72-BA33-88D84817EE30";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(SECTION)
                .WithElement(
                    new SelectionConfigurationElement(LIBRARY_BROWSER_TILE_IMAGE, Strings.LibraryBrowserBehaviourConfiguration_Image, path: Strings.LibraryBrowserBehaviourConfiguration_Path)
                        .WithOptions(GetLibraryImageOptions()))
                .WithElement(
                    new IntegerConfigurationElement(LIBRARY_BROWSER_GRID_TILE_SIZE, Strings.LibraryBrowserBehaviourConfiguration_Size_Grid, path: Strings.LibraryBrowserBehaviourConfiguration_Path)
                        .WithValue(DEFAULT_GRID_TILE_SIZE)
                        .WithValidationRule(new IntegerValidationRule(MIN_TILE_SIZE, MAX_TIME_SIZE, 4)))
                .WithElement(
                    new IntegerConfigurationElement(LIBRARY_BROWSER_LIST_TILE_SIZE, Strings.LibraryBrowserBehaviourConfiguration_Size_List, path: Strings.LibraryBrowserBehaviourConfiguration_Path)
                        .WithValue(DEFAULT_LIST_TILE_SIZE)
                        .WithValidationRule(new IntegerValidationRule(MIN_TILE_SIZE, MAX_TIME_SIZE, 4))
            );
        }

        private static IEnumerable<SelectionConfigurationOption> GetLibraryImageOptions()
        {
            yield return new SelectionConfigurationOption(LIBRARY_BROWSER_TILE_IMAGE_FIRST, Strings.LibraryBrowserBehaviourConfiguration_Image_First);
            yield return new SelectionConfigurationOption(LIBRARY_BROWSER_TILE_IMAGE_COMPOUND, Strings.LibraryBrowserBehaviourConfiguration_Image_Compound).Default();
        }

        public static LibraryBrowserImageMode GetLibraryImage(SelectionConfigurationOption option)
        {
            switch (option.Id)
            {
                case LIBRARY_BROWSER_TILE_IMAGE_FIRST:
                    return LibraryBrowserImageMode.First;
                default:
                case LIBRARY_BROWSER_TILE_IMAGE_COMPOUND:
                    return LibraryBrowserImageMode.Compound;
            }
        }
    }

    public enum LibraryBrowserImageMode : byte
    {
        None,
        First,
        Compound
    }
}
