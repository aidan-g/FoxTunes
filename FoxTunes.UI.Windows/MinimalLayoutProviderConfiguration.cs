using FoxTunes.Interfaces;
using System.Collections.Generic;

namespace FoxTunes
{
    public static class MinimalLayoutProviderConfiguration
    {
        public const string ID = "AAAAA18B-86F0-45A6-988D-B15A56128429";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(WindowsUserInterfaceConfiguration.SECTION)
                .WithElement(
                    new SelectionConfigurationElement(WindowsUserInterfaceConfiguration.LAYOUT_ELEMENT).WithOptions(GetLayoutOptions())
            );
        }

        private static IEnumerable<SelectionConfigurationOption> GetLayoutOptions()
        {
            var releaseType = StandardComponents.Instance.Configuration.ReleaseType;
            var option = new SelectionConfigurationOption(ID, Strings.MinimalLayoutProvider_Name);
            if (releaseType == ReleaseType.Minimal)
            {
                option.Default();
            }
            yield return option;
        }
    }
}
