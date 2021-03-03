using System.Collections.Generic;

namespace FoxTunes
{
    public static class VorbisEncoderSettingsConfiguration
    {
        public const string SECTION = BassEncoderBehaviourConfiguration.SECTION;

        public const string ENABLED_ELEMENT = BassEncoderBehaviourConfiguration.ENABLED_ELEMENT;

        public const string QUALITY_ELEMENT = "AAAA4AE0-3259-40F8-8C64-D1FBF075DD3E";

        public const int MIN_QUALITY = 1;

        public const int MAX_QUALITY = 10;

        public const int DEFAULT_QUALITY = 5;

        public static IEnumerable<ConfigurationSection> GetConfigurationSections(IBassEncoderSettings settings)
        {
            yield return new ConfigurationSection(SECTION)
                .WithElement(new IntegerConfigurationElement(QUALITY_ELEMENT, "Quality", path: settings.Name)
                    .WithValue(DEFAULT_QUALITY)
                    .WithValidationRule(new IntegerValidationRule(MIN_QUALITY, MAX_QUALITY))
                    .DependsOn(SECTION, ENABLED_ELEMENT)
            );
        }
    }
}
