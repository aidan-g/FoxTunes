using System.Collections.Generic;

namespace FoxTunes
{
    public static class InputManagerConfiguration
    {
        public const string SECTION = "4B5B0E73-8000-484E-8F68-77E11FC8AD45";

        public const string ENABLED_ELEMENT = "AAAA41A0-976D-4573-8758-984AACBD235B";

        public const string PLAY_ELEMENT = "BBBBD9B9-1D10-4EEB-81BA-7D54C7A86198";

        public const string PREVIOUS_ELEMENT = "CCCC497E-32FA-4E0F-B73F-8BF4705782B0";

        public const string NEXT_ELEMENT = "DDDD25C3-6A2C-4D2C-985B-1D7210304EFB";

        public const string STOP_ELEMENT = "EEEE73AA-0388-4988-8068-59B768F2A02E";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(SECTION, Strings.InputManagerConfiguration_Section)
                .WithElement(
                    new BooleanConfigurationElement(ENABLED_ELEMENT, Strings.InputManagerConfiguration_Enabled, path: Strings.InputManagerConfiguration_Path_GlobalHotkeys).WithValue(false))
                .WithElement(
                    new TextConfigurationElement(PLAY_ELEMENT, Strings.InputManagerConfiguration_Play, path: Strings.InputManagerConfiguration_Path_GlobalHotkeys)
                        .WithValue("MediaPlayPause").DependsOn(SECTION, ENABLED_ELEMENT))
                .WithElement(
                    new TextConfigurationElement(PREVIOUS_ELEMENT, Strings.InputManagerConfiguration_Previous, path: Strings.InputManagerConfiguration_Path_GlobalHotkeys)
                        .WithValue("MediaPreviousTrack").DependsOn(SECTION, ENABLED_ELEMENT))
                 .WithElement(
                    new TextConfigurationElement(NEXT_ELEMENT, Strings.InputManagerConfiguration_Next, path: Strings.InputManagerConfiguration_Path_GlobalHotkeys)
                        .WithValue("MediaNextTrack").DependsOn(SECTION, ENABLED_ELEMENT))
                .WithElement(
                    new TextConfigurationElement(STOP_ELEMENT, Strings.InputManagerConfiguration_Stop, path: Strings.InputManagerConfiguration_Path_GlobalHotkeys)
                        .WithValue("MediaStop").DependsOn(SECTION, ENABLED_ELEMENT)
            );
        }
    }
}
