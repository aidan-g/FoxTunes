using System;
using System.Collections.Generic;

namespace FoxTunes
{
    public static class LyricsBehaviourConfiguration
    {
        public const string SECTION = "42FB4DBD-E28C-4E42-B64F-6921CFCEF924";

        public const string AUTO_SCROLL = "AAAA70DD-84E9-48D5-B174-E0A0FC498698";

        public const string AUTO_LOOKUP = "BBBB3698-E26A-4D6C-9BBF-E845B0F9D150";

        public const string AUTO_LOOKUP_PROVIDER = "BBCC07C6-5584-4E7E-9526-EB60F9F72E49";

        public const string EDITOR = "CCCCF394-0C2A-4BC3-ADA3-9E89F8C897D9";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(SECTION, Strings.LyricsBehaviourConfiguration_Section)
                .WithElement(new BooleanConfigurationElement(AUTO_SCROLL, Strings.LyricsBehaviourConfiguration_AutoScroll)
                    .WithValue(true)
                    .DependsOn(MetaDataBehaviourConfiguration.SECTION, MetaDataBehaviourConfiguration.ENABLE_ELEMENT))
                .WithElement(new BooleanConfigurationElement(AUTO_LOOKUP, Strings.LyricsBehaviourConfiguration_AutoLookup)
                    .WithValue(false).DependsOn(MetaDataBehaviourConfiguration.SECTION, MetaDataBehaviourConfiguration.ENABLE_ELEMENT))
                .WithElement(new SelectionConfigurationElement(AUTO_LOOKUP_PROVIDER, Strings.LyricsBehaviourConfiguration_AutoLookupProvider)
                    .DependsOn(MetaDataBehaviourConfiguration.SECTION, MetaDataBehaviourConfiguration.ENABLE_ELEMENT)
                    .DependsOn(SECTION, AUTO_LOOKUP))
                .WithElement(new TextConfigurationElement(EDITOR, Strings.LyricsBehaviourConfiguration_Editor).WithValue("notepad.exe").DependsOn(MetaDataBehaviourConfiguration.SECTION, MetaDataBehaviourConfiguration.ENABLE_ELEMENT)
            );
        }
    }
}
