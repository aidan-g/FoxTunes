using System.Collections.Generic;

namespace FoxTunes
{
    public static class DefaultLayoutProviderConfiguration
    {
        public const string ID = "BBBB4ED2-782D-4622-ADF4-AAE2B543E0F3";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(WindowsUserInterfaceConfiguration.SECTION)
                .WithElement(
                    new SelectionConfigurationElement(WindowsUserInterfaceConfiguration.LAYOUT_ELEMENT).WithOptions(GetLayoutOptions())
            );
        }

        private static IEnumerable<SelectionConfigurationOption> GetLayoutOptions()
        {
            yield return new SelectionConfigurationOption(ID, Strings.DefaultLayoutProvider_Name);
        }
    }
}
