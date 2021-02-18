using System.Collections.Generic;

namespace FoxTunes
{
    public static class ChartLyricsProviderConfiguration
    {
        public const string SECTION = LyricsBehaviourConfiguration.SECTION;

        public const string BASE_URL = "AAAA023E-E339-4DA3-BEEE-CB197C63BC8B";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(SECTION, Strings.LyricsBehaviourConfiguration_Section)
                .WithElement(new TextConfigurationElement(BASE_URL, Strings.ChartLyricsProviderConfiguration_BaseUrl)
                    .WithValue(ChartLyricsProvider.BASE_URL)
                    .DependsOn(MetaDataBehaviourConfiguration.SECTION, MetaDataBehaviourConfiguration.ENABLE_ELEMENT)
            );
        }
    }
}
