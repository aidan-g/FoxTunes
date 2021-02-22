using System.Collections.Generic;

namespace FoxTunes
{
    public static class ChartLyricsProviderConfiguration
    {
        public const string SECTION = LyricsBehaviourConfiguration.SECTION;

        public const string AUTO_LOOKUP = LyricsBehaviourConfiguration.AUTO_LOOKUP;

        public const string AUTO_LOOKUP_PROVIDER = LyricsBehaviourConfiguration.AUTO_LOOKUP_PROVIDER;

        public const string BASE_URL = "AAAA023E-E339-4DA3-BEEE-CB197C63BC8B";

        public const string MIN_CONFIDENCE = "BBBB58A3-05EF-42A8-8E28-54F794298899";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(SECTION, Strings.LyricsBehaviourConfiguration_Section)
                .WithElement(new SelectionConfigurationElement(AUTO_LOOKUP_PROVIDER, Strings.LyricsBehaviourConfiguration_AutoLookupProvider)
                    .WithOptions(new[] { new SelectionConfigurationOption(ChartLyricsProvider.ID, Strings.ChartLyrics) })
                    .DependsOn(SECTION, AUTO_LOOKUP))
                .WithElement(new TextConfigurationElement(BASE_URL, Strings.ChartLyricsProviderConfiguration_BaseUrl, path: Strings.ChartLyrics)
                    .WithValue(ChartLyricsProvider.BASE_URL)
                    .DependsOn(MetaDataBehaviourConfiguration.SECTION, MetaDataBehaviourConfiguration.ENABLE_ELEMENT))
                .WithElement(new DoubleConfigurationElement(MIN_CONFIDENCE, Strings.ChartLyricsProviderConfiguration_MinConfidence, path: Strings.ChartLyrics)
                    .WithValue(0.8)
                    .WithValidationRule(new DoubleValidationRule(0, 1, 0.1))
                    .DependsOn(MetaDataBehaviourConfiguration.SECTION, MetaDataBehaviourConfiguration.ENABLE_ELEMENT)
            );
        }
    }
}
