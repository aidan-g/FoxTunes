using System.Collections.Generic;

namespace FoxTunes
{
    public static class ExpressionDarkThemeConfiguration
    {
        public const string SECTION = WindowsUserInterfaceConfiguration.SECTION;

        public const string THEME = WindowsUserInterfaceConfiguration.THEME_ELEMENT;

        public const string LIST_ROW_SHADING = "TTTTB8F9-2144-4CF9-86C5-58F91EF89AFC";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(SECTION)
                .WithElement(new BooleanConfigurationElement(LIST_ROW_SHADING, Strings.ExpressionDarkThemeConfiguration_ListRowShading, path: Strings.General_Advanced)
                    .WithValue(true)
                    .DependsOn(SECTION, THEME, ExpressionDarkTheme.ID)
            );
        }
    }
}
