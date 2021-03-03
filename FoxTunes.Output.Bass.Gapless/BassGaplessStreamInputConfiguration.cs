using System.Collections.Generic;

namespace FoxTunes
{
    public static class BassGaplessStreamInputConfiguration
    {
        public const string INPUT_GAPLESS_OPTION = "AAAA9EBD-B309-4ABF-99E8-05D913E63877";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(BassOutputConfiguration.SECTION)
                .WithElement(new SelectionConfigurationElement(BassOutputConfiguration.INPUT_ELEMENT)
                    .WithOptions(new[] { new SelectionConfigurationOption(INPUT_GAPLESS_OPTION, Strings.BassGaplessStreamInput_Name).Default() })
            );
        }
    }
}
