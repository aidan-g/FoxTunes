using System.Collections.Generic;

namespace FoxTunes
{
    public static class WaveBarBehaviourConfiguration
    {
        public const string SECTION = "1355A5DB-84D3-4582-B294-5B6EEC8B80D2";

        public const string MODE_ELEMENT = "402DE29B-50AB-4345-89FB-174D87824F98";

        public const string MODE_MONO_OPTION = "AAAA21C9-A9FF-4CFD-8EFB-96EE79455050";

        public const string MODE_STEREO_OPTION = "BBBB6F8B-5FCD-4EEC-997E-D3DF8B63BD36";

        public const string MODE_SEPERATE_OPTION = "CCCC3286-5097-4977-A4CC-5CEDA5E2E099";

        public const string RESOLUTION_ELEMENT = "AAAACCC0-596C-489C-BD39-E74C0AE3697C";

        public const string AMPLITUDE_ELEMENT = "AABB7D69-3C36-44EB-8960-4147A148F31A";

        public const string CACHE_ELEMENT = "BBBBAD4B-8BB4-47C9-9D18-2121C48115CE";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(SECTION, "Wave Form")
                .WithElement(new SelectionConfigurationElement(MODE_ELEMENT, "Mode").WithOptions(GetModeOptions()))
                .WithElement(new IntegerConfigurationElement(RESOLUTION_ELEMENT, "Resolution", path: "Advanced").WithValue(10).WithValidationRule(new IntegerValidationRule(1, 100)))
                .WithElement(new IntegerConfigurationElement(AMPLITUDE_ELEMENT, "Amplitude", path: "Advanced").WithValue(8).WithValidationRule(new IntegerValidationRule(1, 10)))
                .WithElement(new BooleanConfigurationElement(CACHE_ELEMENT, "Cache", path: "Advanced").WithValue(false)
            );
        }

        private static IEnumerable<SelectionConfigurationOption> GetModeOptions()
        {
            yield return new SelectionConfigurationOption(MODE_MONO_OPTION, "Mono").Default();
            yield return new SelectionConfigurationOption(MODE_SEPERATE_OPTION, "Seperate");
        }

        public static WaveFormRendererMode GetMode(SelectionConfigurationOption option)
        {
            switch (option.Id)
            {
                default:
                case MODE_MONO_OPTION:
                    return WaveFormRendererMode.Mono;
                case MODE_SEPERATE_OPTION:
                    return WaveFormRendererMode.Seperate;
            }
        }
    }
}
