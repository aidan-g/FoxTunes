using System.Collections.Generic;

namespace FoxTunes
{
    public static class LameEncoderSettingsConfiguration
    {
        public const string SECTION = BassEncoderBehaviourConfiguration.SECTION;

        public const string ENABLED_ELEMENT = BassEncoderBehaviourConfiguration.ENABLED_ELEMENT;

        public const string BITRATE_ELEMENT = "AAAAD40F-27B5-4D3D-AF42-D1DBA114EFEB";

        public const string BITRATE_65_OPTION = "65";

        public const string BITRATE_85_OPTION = "85";

        public const string BITRATE_100_OPTION = "100";

        public const string BITRATE_115_OPTION = "115";

        public const string BITRATE_130_OPTION = "130";

        public const string BITRATE_165_OPTION = "165";

        public const string BITRATE_175_OPTION = "175";

        public const string BITRATE_190_OPTION = "190";

        public const string BITRATE_225_OPTION = "225";

        public const string BITRATE_245_OPTION = "245";

        public const string BITRATE_320_OPTION = "320";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections(IBassEncoderSettings settings)
        {
            yield return new ConfigurationSection(SECTION)
                .WithElement(new SelectionConfigurationElement(BITRATE_ELEMENT, "Bitrate", path: settings.Name)
                    .WithOptions(GetBitrateOptions())
                    .DependsOn(SECTION, ENABLED_ELEMENT)
            );
        }

        private static IEnumerable<SelectionConfigurationOption> GetBitrateOptions()
        {
            yield return new SelectionConfigurationOption(BITRATE_65_OPTION, string.Format("{0}kbps", BITRATE_65_OPTION));
            yield return new SelectionConfigurationOption(BITRATE_85_OPTION, string.Format("{0}kbps", BITRATE_85_OPTION));
            yield return new SelectionConfigurationOption(BITRATE_100_OPTION, string.Format("{0}kbps", BITRATE_100_OPTION));
            yield return new SelectionConfigurationOption(BITRATE_115_OPTION, string.Format("{0}kbps", BITRATE_115_OPTION));
            yield return new SelectionConfigurationOption(BITRATE_130_OPTION, string.Format("{0}kbps", BITRATE_130_OPTION));
            yield return new SelectionConfigurationOption(BITRATE_165_OPTION, string.Format("{0}kbps", BITRATE_165_OPTION));
            yield return new SelectionConfigurationOption(BITRATE_175_OPTION, string.Format("{0}kbps", BITRATE_175_OPTION));
            yield return new SelectionConfigurationOption(BITRATE_190_OPTION, string.Format("{0}kbps", BITRATE_190_OPTION));
            yield return new SelectionConfigurationOption(BITRATE_225_OPTION, string.Format("{0}kbps", BITRATE_225_OPTION));
            yield return new SelectionConfigurationOption(BITRATE_245_OPTION, string.Format("{0}kbps", BITRATE_245_OPTION));
            yield return new SelectionConfigurationOption(BITRATE_320_OPTION, string.Format("{0}kbps", BITRATE_320_OPTION)).Default();
        }

        public static int GetBitrate(SelectionConfigurationOption option)
        {
            var bitrate = default(int);
            if (!int.TryParse(option.Id, out bitrate))
            {
                bitrate = 320;
            }
            return bitrate;
        }
    }
}
