#if NET40

//ListView grouping is too slow under net40 due to lack of virtualization.

#else

using System.Collections.Generic;

namespace FoxTunes
{
    public static class PlaylistGroupingBehaviourConfiguration
    {
        public const string GROUP_ENABLED_ELEMENT = "BBA65418-FF22-486D-AFF4-85F2F738A852";

        public const string GROUP_SCRIPT_ELEMENT = "CE8245BF-9BE2-4853-9E2F-40389293EFED";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(PlaylistBehaviourConfiguration.SECTION)
                .WithElement(
                    new BooleanConfigurationElement(GROUP_ENABLED_ELEMENT, Strings.PlaylistGroupingBehaviourConfiguration_Enabled)
                        .WithValue(false))
                .WithElement(
                    new TextConfigurationElement(GROUP_SCRIPT_ELEMENT, Strings.PlaylistGroupingBehaviourConfiguration_Script, path: Strings.General_Advanced)
                        .WithValue(Resources.Grouping)
                        .WithFlags(ConfigurationElementFlags.MultiLine)
                        .DependsOn(PlaylistBehaviourConfiguration.SECTION, GROUP_ENABLED_ELEMENT)
            );
        }
    }
}

#endif
