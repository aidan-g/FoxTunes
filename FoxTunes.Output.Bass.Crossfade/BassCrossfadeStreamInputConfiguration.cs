using ManagedBass.Crossfade;
using System;
using System.Collections.Generic;

namespace FoxTunes
{
    public static class BassCrossfadeStreamInputConfiguration
    {
        public const string SECTION = BassOutputConfiguration.SECTION;

        public const string INPUT_ELEMENT = BassOutputConfiguration.INPUT_ELEMENT;

        public const string INPUT_CROSSFADE_OPTION = "BBBBFDE4-7361-456F-AB55-85364B51C2A2";

        public const string MODE_ELEMENT = "AAAA0393-436D-4500-9E4C-D7A3CAE8AC28";

        public const string MODE_ALWAYS_OPTION = "AAAA51B8-2F7B-46BB-BA0F-7D71316055A0";

        public const string MODE_MANUAL_OPTION = "BBBB2352-F7A9-40AF-A654-C76BB6C59A6B";

        public const string PERIOD_IN_ELEMENT = "BBBB69B8-E637-4B00-B46C-37BEAA9873F8";

        public const string PERIOD_OUT_ELEMENT = "CCCC079C-98AB-4773-B55F-E2C91CDDE0BE";

        public const string TYPE_IN_ELEMENT = "DDDDD685-3F5A-44C3-8286-8ED88F5B385E";

        public const string TYPE_OUT_ELEMENT = "EEEEB3EF-2B3A-4CB8-93B5-946A34F9FEF5";

        public const string TYPE_LINEAR_OPTION = "AAAA6A54-B0D9-4CFA-9AEC-4AE730D62716";

        public const string TYPE_IN_QUAD_OPTION = "BBBBC8C7-FFE1-4522-AAC6-17D9F2E2638A";

        public const string TYPE_OUT_QUAD_OPTION = "CCCC416A-08A4-4255-87CF-B4DF467A2285";

        public const string TYPE_IN_EXPO_OPTION = "DDDD083D-8F1D-460C-A557-66DD71366F5C";

        public const string TYPE_OUT_EXPO_OPTION = "EEEED4F2-9E6E-4499-8D36-F219AB45AC85";

        public const string MIX_ELEMENT = "FFFF9C57-11C6-4D50-A0FF-BD38AE70EF0E";

        public const string START_ELEMENT = "EEEF1406-957D-493C-A520-FC999E525F8A";

        public const string PAUSE_RESUME_ELEMENT = "EEEG5ABC-E0FC-43D1-8CD0-54D269FD809D";

        public const string STOP_ELEMENT = "EEEIB80E-222F-4A0A-8C83-AE8D03C3F479";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(SECTION)
                .WithElement(new SelectionConfigurationElement(INPUT_ELEMENT)
                    .WithOptions(new[] { new SelectionConfigurationOption(INPUT_CROSSFADE_OPTION, Strings.BassCrossfadeStreamInput_Name) }))
                .WithElement(new SelectionConfigurationElement(MODE_ELEMENT, "Mode", path: "Fading")
                    .WithOptions(GetModeOptions())
                    .DependsOn(SECTION, INPUT_ELEMENT, INPUT_CROSSFADE_OPTION))
                .WithElement(new IntegerConfigurationElement(PERIOD_IN_ELEMENT, "Fade In Period", path: "Fading")
                    .WithValue(100)
                    .WithValidationRule(new IntegerValidationRule(0, 5000, step: 100))
                    .DependsOn(SECTION, INPUT_ELEMENT, INPUT_CROSSFADE_OPTION))
                .WithElement(new IntegerConfigurationElement(PERIOD_OUT_ELEMENT, "Fade Out Period", path: "Fading")
                    .WithValue(100)
                    .WithValidationRule(new IntegerValidationRule(0, 5000, step: 100))
                    .DependsOn(SECTION, INPUT_ELEMENT, INPUT_CROSSFADE_OPTION))
                .WithElement(new SelectionConfigurationElement(TYPE_IN_ELEMENT, "Fade In Curve", path: "Fading")
                    .WithOptions(GetTypeOptions(TYPE_OUT_QUAD_OPTION))
                    .DependsOn(SECTION, INPUT_ELEMENT, INPUT_CROSSFADE_OPTION))
                .WithElement(new SelectionConfigurationElement(TYPE_OUT_ELEMENT, "Fade Out Curve", path: "Fading")
                    .WithOptions(GetTypeOptions(TYPE_OUT_QUAD_OPTION))
                    .DependsOn(SECTION, INPUT_ELEMENT, INPUT_CROSSFADE_OPTION))
                .WithElement(new BooleanConfigurationElement(MIX_ELEMENT, "Crossfade", path: "Fading")
                    .WithValue(false)
                    .DependsOn(SECTION, INPUT_ELEMENT, INPUT_CROSSFADE_OPTION))
                .WithElement(new BooleanConfigurationElement(START_ELEMENT, "Start", path: "Fading")
                    .WithValue(false)
                    .DependsOn(SECTION, INPUT_ELEMENT, INPUT_CROSSFADE_OPTION))
                .WithElement(new BooleanConfigurationElement(PAUSE_RESUME_ELEMENT, "Pause/Resume", path: "Fading")
                    .WithValue(false)
                    .DependsOn(SECTION, INPUT_ELEMENT, INPUT_CROSSFADE_OPTION))
                .WithElement(new BooleanConfigurationElement(STOP_ELEMENT, "Stop", path: "Fading")
                    .WithValue(false)
                    .DependsOn(SECTION, INPUT_ELEMENT, INPUT_CROSSFADE_OPTION)
            );
        }

        private static IEnumerable<SelectionConfigurationOption> GetModeOptions()
        {
            yield return new SelectionConfigurationOption(MODE_ALWAYS_OPTION, "Always");
            yield return new SelectionConfigurationOption(MODE_MANUAL_OPTION, "Manual").Default();
        }

        public static BassCrossfadeMode GetMode(SelectionConfigurationOption option)
        {
            switch (option.Id)
            {
                case MODE_ALWAYS_OPTION:
                    return BassCrossfadeMode.Always;
                default:
                case MODE_MANUAL_OPTION:
                    return BassCrossfadeMode.Manual;
            }
        }

        private static IEnumerable<SelectionConfigurationOption> GetTypeOptions(string value)
        {
            var options = new[]
            {
                new SelectionConfigurationOption(TYPE_LINEAR_OPTION, "Linear"),
                new SelectionConfigurationOption(TYPE_IN_QUAD_OPTION, "In Quad"),
                new SelectionConfigurationOption(TYPE_OUT_QUAD_OPTION, "Out Quad"),
                new SelectionConfigurationOption(TYPE_IN_EXPO_OPTION, "In Expo"),
                new SelectionConfigurationOption(TYPE_OUT_EXPO_OPTION, "Out Expo")
            };
            foreach (var option in options)
            {
                if (string.Equals(option.Id, value, StringComparison.OrdinalIgnoreCase))
                {
                    option.Default();
                }
                yield return option;
            }
        }

        public static BassCrossfadeType GetType(SelectionConfigurationOption option)
        {
            switch (option.Id)
            {
                case TYPE_LINEAR_OPTION:
                    return BassCrossfadeType.Linear;
                case TYPE_IN_QUAD_OPTION:
                    return BassCrossfadeType.InQuad;
                default:
                case TYPE_OUT_QUAD_OPTION:
                    return BassCrossfadeType.OutQuad;
                case TYPE_IN_EXPO_OPTION:
                    return BassCrossfadeType.InExpo;
                case TYPE_OUT_EXPO_OPTION:
                    return BassCrossfadeType.OutExpo;
            }
        }
    }
}
