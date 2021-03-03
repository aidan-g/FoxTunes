using FoxTunes.Interfaces;
using System.Collections.Generic;

namespace FoxTunes
{
    public static class UIComponentLayoutProviderConfiguration
    {
        public const string ID = "CCCC083C-3C19-4AAC-97C7-565AF8F83115";

        public const string MAIN = "AAAAE4B-657B-45C2-B365-8FD69498D7C2";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(WindowsUserInterfaceConfiguration.SECTION)
                .WithElement(
                    new SelectionConfigurationElement(WindowsUserInterfaceConfiguration.LAYOUT_ELEMENT).WithOptions(GetLayoutOptions()))
                .WithElement(
                    new TextConfigurationElement(MAIN, "Main Layout", path: "Advanced\\Layouts").WithValue(Resources.Main)
            );
            ComponentRegistry.Instance.GetComponent<IConfiguration>().GetElement<SelectionConfigurationElement>(
                WindowsUserInterfaceConfiguration.SECTION,
                WindowsUserInterfaceConfiguration.LAYOUT_ELEMENT
            ).ConnectValue(value => UpdateConfiguration(value));
        }

        private static IEnumerable<SelectionConfigurationOption> GetLayoutOptions()
        {
            var releaseType = StandardComponents.Instance.Configuration.ReleaseType;
            var option = new SelectionConfigurationOption(ID, Strings.UIComponentLayoutProvider_Name);
            if (releaseType == ReleaseType.Default)
            {
                option.Default();
            }
            yield return option;
        }

        private static void UpdateConfiguration(SelectionConfigurationOption value)
        {
            switch (value.Id)
            {
                case ID:
                    ComponentRegistry.Instance.GetComponent<IConfiguration>().GetElement(
                        WindowsUserInterfaceConfiguration.SECTION,
                        MAIN
                    ).Show();
                    break;
                default:
                    ComponentRegistry.Instance.GetComponent<IConfiguration>().GetElement(
                        WindowsUserInterfaceConfiguration.SECTION,
                        MAIN
                    ).Hide();
                    break;
            }
        }
    }
}
