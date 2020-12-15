using System.Collections.Generic;

namespace FoxTunes
{
    public static class BassOutputTempoStreamComponentBehaviourConfiguration
    {
        public const string ENABLED = "AAAA1050-B08A-4EDB-9A06-C7A3758F5B7E";

        public const string TEMPO = "BBBBBDC6-889F-4461-BA5F-140F551F3F0F";

        public const string PITCH = "CCCC9C66-A64F-4408-B7C5-98055E40A03C";

        public const string RATE = "DDDD2C8A-B2F9-4F6C-9AE9-BD3BCB6C5594";

        public const string AA_FILTER = "EEEE9F21-3EEB-4D67-ABB0-8AC163FEB5CD";

        public const string AA_FILTER_LENGTH = "FFFFFF38-8567-46EA-A1DB-5EEE64BA3099";

        public const string FAST = "GGGG5143-4B5D-4B87-A1A7-1E2A83F10E78";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(BassOutputConfiguration.SECTION, "Output")
                .WithElement(
                    new BooleanConfigurationElement(ENABLED, "Enabled", path: "Tempo")
                        .WithValue(false))
                .WithElement(
                    new IntegerConfigurationElement(TEMPO, "Tempo", path: "Tempo")
                        .WithValue(0)
                        .DependsOn(BassOutputConfiguration.SECTION, ENABLED)
                        .WithValidationRule(new IntegerValidationRule(Tempo.MIN_TEMPO, Tempo.MAX_TEMPO, 1)))
                .WithElement(
                    new IntegerConfigurationElement(PITCH, "Pitch", path: "Tempo")
                        .WithValue(0)
                        .DependsOn(BassOutputConfiguration.SECTION, ENABLED)
                        .WithValidationRule(new IntegerValidationRule(Tempo.MIN_PITCH, Tempo.MAX_PITCH, 1)))
                .WithElement(
                    new IntegerConfigurationElement(RATE, "Rate", path: "Tempo")
                        .WithValue(0)
                        .DependsOn(BassOutputConfiguration.SECTION, ENABLED)
                        .WithValidationRule(new IntegerValidationRule(Tempo.MIN_RATE, Tempo.MAX_RATE, 1)))
                .WithElement(
                    new BooleanConfigurationElement(AA_FILTER, "Anti Alias Filter", path: "Tempo")
                        .WithValue(true)
                        .DependsOn(BassOutputConfiguration.SECTION, ENABLED))
                .WithElement(
                    new IntegerConfigurationElement(AA_FILTER_LENGTH, "Anti Alias Filter Length", path: "Tempo")
                        .WithValue(32)
                        .DependsOn(BassOutputConfiguration.SECTION, ENABLED)
                        .DependsOn(BassOutputConfiguration.SECTION, AA_FILTER)
                        .WithValidationRule(new IntegerValidationRule(Tempo.MIN_AA_FILTER_LENGTH, Tempo.MAX_AA_FILTER_LENGTH, 1)))
                .WithElement(
                    new BooleanConfigurationElement(FAST, "Fast Algorithm", path: "Tempo")
                        .WithValue(false)
                        .DependsOn(BassOutputConfiguration.SECTION, ENABLED));
        }
    }
}
