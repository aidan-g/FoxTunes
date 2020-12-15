using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FoxTunes
{
    public static class LibraryBrowserBehaviourConfiguration
    {
        const int DEFAULT_GRID_TILE_SIZE = 160;

        const int DEFAULT_LIST_TILE_SIZE = 80;

        public const string SECTION = WindowsUserInterfaceConfiguration.SECTION;

        public const string LIBRARY_BROWSER_VIEW = "GGHH4863-172A-40E5-B57D-EFD0DCCC8110";

        public const string LIBRARY_BROWSER_VIEW_GRID = "AAAADACF-67FE-4D9F-B6C9-F9AD6D7B20C2";

        public const string LIBRARY_BROWSER_VIEW_LIST = "BBBB0E54-FC32-4DBC-A13F-AE0B2060AC1A";

        public const string LIBRARY_BROWSER_TILE_SIZE = "IIII0E5E-FB67-43D3-AE30-BF7571A1A8B1";

        public const string LIBRARY_BROWSER_TILE_IMAGE = "HHIIFCBE-2AA0-4B99-9C6E-E3F196540807";

        public const string LIBRARY_BROWSER_TILE_IMAGE_FIRST = "AAAA9419-8D7E-4150-8FB8-999D2C019941";

        public const string LIBRARY_BROWSER_TILE_IMAGE_COMPOUND = "BBBB32EB-B876-4F72-BA33-88D84817EE30";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(SECTION, "Appearance")
                .WithElement(
                    new SelectionConfigurationElement(LIBRARY_BROWSER_VIEW, "View Type", path: "Library Browser")
                        .WithOptions(GetLibraryViewOptions()))
                .WithElement(
                    new SelectionConfigurationElement(LIBRARY_BROWSER_TILE_IMAGE, "Image Type", path: "Library Browser")
                        .WithOptions(GetLibraryImageOptions()))
                .WithElement(
                    new IntegerConfigurationElement(LIBRARY_BROWSER_TILE_SIZE, "Tile Size", path: "Library Browser")
                        .WithValue(DEFAULT_GRID_TILE_SIZE)
                        .DependsOn(SECTION, LIBRARY_BROWSER_VIEW, LIBRARY_BROWSER_VIEW_GRID)
                        .WithValidationRule(new IntegerValidationRule(60, 300, 4))
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

        private static IEnumerable<SelectionConfigurationOption> GetLibraryImageOptions()
        {
            yield return new SelectionConfigurationOption(LIBRARY_BROWSER_TILE_IMAGE_FIRST, "First");
            yield return new SelectionConfigurationOption(LIBRARY_BROWSER_TILE_IMAGE_COMPOUND, "Compound").Default();
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

        private static void OnLibraryViewChanged(object sender, EventArgs e)
        {
            var view = ComponentRegistry.Instance.GetComponent<IConfiguration>().GetElement<SelectionConfigurationElement>(
                WindowsUserInterfaceConfiguration.SECTION,
                LIBRARY_BROWSER_VIEW
            );
            var image = ComponentRegistry.Instance.GetComponent<IConfiguration>().GetElement<SelectionConfigurationElement>(
                WindowsUserInterfaceConfiguration.SECTION,
                LIBRARY_BROWSER_TILE_IMAGE
            );
            var size = ComponentRegistry.Instance.GetComponent<IConfiguration>().GetElement<IntegerConfigurationElement>(
                WindowsUserInterfaceConfiguration.SECTION,
                LIBRARY_BROWSER_TILE_SIZE
            );
            switch (GetLibraryView(view.Value))
            {
                default:
                case LibraryBrowserViewMode.Grid:
                    image.Value = image.Value = image.Options.FirstOrDefault(
                        option => string.Equals(option.Id, LIBRARY_BROWSER_TILE_IMAGE_COMPOUND, StringComparison.OrdinalIgnoreCase)
                    );
                    size.Value = DEFAULT_GRID_TILE_SIZE;
                    break;
                case LibraryBrowserViewMode.List:
                    image.Value = image.Value = image.Options.FirstOrDefault(
                        option => string.Equals(option.Id, LIBRARY_BROWSER_TILE_IMAGE_FIRST, StringComparison.OrdinalIgnoreCase)
                    );
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

    public enum LibraryBrowserImageMode : byte
    {
        None,
        First,
        Compound
    }
}
