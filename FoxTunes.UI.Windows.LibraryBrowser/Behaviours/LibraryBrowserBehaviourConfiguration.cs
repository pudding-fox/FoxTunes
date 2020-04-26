using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;

namespace FoxTunes
{
    public static class LibraryBrowserBehaviourConfiguration
    {
        const int DEFAULT_GRID_TILE_SIZE = 160;

        const int DEFAULT_LIST_TILE_SIZE = 80;

        public const string LIBRARY_BROWSER_VIEW = "GGHH4863-172A-40E5-B57D-EFD0DCCC8110";

        public const string LIBRARY_BROWSER_VIEW_GRID = "AAAADACF-67FE-4D9F-B6C9-F9AD6D7B20C2";

        public const string LIBRARY_BROWSER_VIEW_LIST = "BBBB0E54-FC32-4DBC-A13F-AE0B2060AC1A";

        public const string LIBRARY_BROWSER_TILE_SIZE = "HHHH0E5E-FB67-43D3-AE30-BF7571A1A8B1";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(WindowsUserInterfaceConfiguration.SECTION, "Appearance")
                .WithElement(
                    new SelectionConfigurationElement(LIBRARY_BROWSER_VIEW, "Library View").WithOptions(GetLibraryViewOptions()))
                .WithElement(
                    new IntegerConfigurationElement(LIBRARY_BROWSER_TILE_SIZE, "Library Tile Size", path: "Advanced").WithValue(DEFAULT_GRID_TILE_SIZE).WithValidationRule(new IntegerValidationRule(60, 300, 4))
            );
            var element = ComponentRegistry.Instance.GetComponent<IConfiguration>().GetElement<SelectionConfigurationElement>(
                WindowsUserInterfaceConfiguration.SECTION,
                LIBRARY_BROWSER_VIEW
            );
            element.ValueChanged += OnLibraryViewChanged;
        }

        private static IEnumerable<SelectionConfigurationOption> GetLibraryViewOptions()
        {
            yield return new SelectionConfigurationOption(LIBRARY_BROWSER_VIEW_GRID, "Grid").Default();
            yield return new SelectionConfigurationOption(LIBRARY_BROWSER_VIEW_LIST, "List");
        }

        public static LibraryBrowserViewMode GetLibraryView(SelectionConfigurationOption option)
        {
            switch (option.Id)
            {
                default:
                case LIBRARY_BROWSER_VIEW_GRID:
                    return LibraryBrowserViewMode.Grid;
                case LIBRARY_BROWSER_VIEW_LIST:
                    return LibraryBrowserViewMode.List;
            }
        }

        private static void OnLibraryViewChanged(object sender, EventArgs e)
        {
            var view = ComponentRegistry.Instance.GetComponent<IConfiguration>().GetElement<SelectionConfigurationElement>(
                WindowsUserInterfaceConfiguration.SECTION,
                LIBRARY_BROWSER_VIEW
            );
            var size = ComponentRegistry.Instance.GetComponent<IConfiguration>().GetElement<IntegerConfigurationElement>(
                WindowsUserInterfaceConfiguration.SECTION,
                LIBRARY_BROWSER_TILE_SIZE
            );
            switch (GetLibraryView(view.Value))
            {
                default:
                case LibraryBrowserViewMode.Grid:
                    size.Value = DEFAULT_GRID_TILE_SIZE;
                    break;
                case LibraryBrowserViewMode.List:
                    size.Value = DEFAULT_LIST_TILE_SIZE;
                    break;
            }
        }
    }

    public enum LibraryBrowserViewMode : byte
    {
        None,
        Grid,
        List
    }
}
