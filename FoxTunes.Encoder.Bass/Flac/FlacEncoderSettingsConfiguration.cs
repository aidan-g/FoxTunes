using System.Collections.Generic;

namespace FoxTunes
{
    public static class FlacEncoderSettingsConfiguration
    {
        public const string SECTION = BassEncoderBehaviourConfiguration.SECTION;

        public const string ENABLED_ELEMENT = BassEncoderBehaviourConfiguration.ENABLED_ELEMENT;

        public const string DEPTH_ELEMENT = "AAAA45EA-6B59-43F8-A578-5C1C3640978F";

        public const string DEPTH_AUTO_OPTION = "BBBB5EF5-0BEA-4B1F-897C-A296B4827B98";

        public const string DEPTH_16_OPTION = "DDDD1942-99FF-4014-BD61-245BC7C15AA6";

        public const string DEPTH_24_OPTION = "EEEE2E2D-B2FC-45E8-95E7-6C56B93248D6";

        public const string COMPRESSION_ELEMENT = "FFFF737A-B151-436D-8EAE-603A2202597A";

        public const string IGNORE_ERRORS_ELEMENT = "GGGG821F-7A7B-4B3F-BE5D-F046AEFAAC6A";

        public const int DEFAULT_DEPTH = 16;

        public const int MIN_COMPRESSION = 0;

        public const int MAX_COMPRESSION = 8;

        public const int DEFAULT_COMPRESSION = 5;

        public static IEnumerable<ConfigurationSection> GetConfigurationSections(IBassEncoderSettings settings)
        {
            yield return new ConfigurationSection(SECTION)
                .WithElement(new SelectionConfigurationElement(DEPTH_ELEMENT, "Depth", path: settings.Name)
                    .WithOptions(GetDepthOptions())
                    .DependsOn(SECTION, ENABLED_ELEMENT))
                .WithElement(new IntegerConfigurationElement(COMPRESSION_ELEMENT, "Compression Level", path: settings.Name)
                    .WithValue(DEFAULT_COMPRESSION)
                    .WithValidationRule(new IntegerValidationRule(MIN_COMPRESSION, MAX_COMPRESSION))
                    .DependsOn(SECTION, ENABLED_ELEMENT))
                .WithElement(new BooleanConfigurationElement(IGNORE_ERRORS_ELEMENT, "Ignore Errors", path: settings.Name)
                    .WithValue(true)
                    .DependsOn(SECTION, ENABLED_ELEMENT)
            );
        }

        private static IEnumerable<SelectionConfigurationOption> GetDepthOptions()
        {
            yield return new SelectionConfigurationOption(DEPTH_AUTO_OPTION, "Auto");
            yield return new SelectionConfigurationOption(DEPTH_16_OPTION, "16bit");
            yield return new SelectionConfigurationOption(DEPTH_24_OPTION, "24bit");
        }

        public static int GetDepth(SelectionConfigurationOption option)
        {
            switch (option.Id)
            {
                case DEPTH_AUTO_OPTION:
                    return BassEncoderSettings.DEPTH_AUTO;
                case DEPTH_16_OPTION:
                    return BassEncoderSettings.DEPTH_16;
                case DEPTH_24_OPTION:
                    return BassEncoderSettings.DEPTH_24;
                default:
                    return DEFAULT_DEPTH;
            }
        }
    }
}
