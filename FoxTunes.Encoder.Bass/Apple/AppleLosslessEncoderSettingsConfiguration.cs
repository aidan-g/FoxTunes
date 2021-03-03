using System.Collections.Generic;

namespace FoxTunes
{
    public static class AppleLosslessEncoderSettingsConfiguration
    {
        public const string SECTION = BassEncoderBehaviourConfiguration.SECTION;

        public const string ENABLED_ELEMENT = BassEncoderBehaviourConfiguration.ENABLED_ELEMENT;

        public const string DEPTH_ELEMENT = "AAAA6691D-4FCE-427D-A563-159A6A5FCFC5";

        public const string DEPTH_AUTO_OPTION = "BBBB1569-442B-4F8A-9A00-3F9D033F2294";

        public const string DEPTH_16_OPTION = "DDDD98BF-B80F-4311-A00D-6E1E5E342C30";

        public const string DEPTH_24_OPTION = "FFFFD77D-63EE-4994-816A-775C142C9B4A";

        public const string DEPTH_32_OPTION = "GGGG1FBA-916C-4BEA-943A-BF19E238F1EB";

        public const int DEFAULT_DEPTH = 16;

        public static IEnumerable<ConfigurationSection> GetConfigurationSections(IBassEncoderSettings settings)
        {
            yield return new ConfigurationSection(SECTION)
                .WithElement(new SelectionConfigurationElement(DEPTH_ELEMENT, "Depth", path: settings.Name)
                    .WithOptions(GetDepthOptions())
                    .DependsOn(SECTION, ENABLED_ELEMENT)
            );
        }

        private static IEnumerable<SelectionConfigurationOption> GetDepthOptions()
        {
            yield return new SelectionConfigurationOption(DEPTH_AUTO_OPTION, "Auto");
            yield return new SelectionConfigurationOption(DEPTH_16_OPTION, "16bit");
            yield return new SelectionConfigurationOption(DEPTH_24_OPTION, "24bit");
            yield return new SelectionConfigurationOption(DEPTH_32_OPTION, "32bit");
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
                case DEPTH_32_OPTION:
                    return BassEncoderSettings.DEPTH_32;
                default:
                    return DEFAULT_DEPTH;
            }
        }
    }
}
