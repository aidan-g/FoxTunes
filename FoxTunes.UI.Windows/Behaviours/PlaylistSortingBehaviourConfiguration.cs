using System.Collections.Generic;

namespace FoxTunes
{
    public static class PlaylistSortingBehaviourConfiguration
    {
        public const string SORT_ENABLED_ELEMENT = "F2EB04ED-2E13-4B03-9866-D7D197CB8A98";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(PlaylistBehaviourConfiguration.SECTION)
                .WithElement(
                    new BooleanConfigurationElement(SORT_ENABLED_ELEMENT, Strings.PlaylistSortingBehaviourConfiguration_Enabled).WithValue(false)
            );
        }
    }
}
