using FoxDb;
using FoxTunes.Interfaces;
using System.Collections.Generic;

namespace FoxTunes
{
    public static class BassOutputConfiguration
    {
        public const string SECTION = "8399D051-81BC-41A6-940D-36E6764213D2";

        public const string RATE_ELEMENT = "AAAAA558-F1ED-41B1-A3DC-95158E01003C";

        public const string ENFORCE_RATE_ELEMENT = "JJJJ5B16-1B49-4C50-A8CF-BE3A6CCD4A87";

        public const string RESAMPLE_QUALITY_ELEMENT = "JJKKF36D-41BC-462F-8F8F-C3E6BD5A4661";

        public const string DEPTH_ELEMENT = "KKKKA2A6-DCA0-4E27-9812-498BB2A2C4BC";

        public const string DEPTH_16_OPTION = "LLLL8466-7582-4B1C-8687-7AB75D636CD8";

        public const string DEPTH_32_OPTION = "MMMM73F2-2E08-4F2B-B94E-DAA945D96497";

        public const string INPUT_ELEMENT = "MMNN52A1-79E9-43AA-BC17-C9DB335AFC9C";

        public const string OUTPUT_ELEMENT = "NNNN6B39-2F8A-4667-9C03-9742FF2D1EA7";

        public const string PLAY_FROM_RAM_ELEMENT = "OOOOBED1-7965-47A3-9798-E46124386A8D";

        public const string BUFFER_LENGTH_ELEMENT = "PPPP3629-1AE5-451F-A545-8B864FEAD038";

        public const string VOLUME_ENABLED_ELEMENT = "PPQQE42E-E530-4995-B3AC-CF6B2D9FE95B";

        public const string VOLUME_ELEMENT = "QQQQ1AF3-507A-426A-AE65-F118D1E71F2D";

        public const string DEVICE_MONITOR_ELEMENT = "RRRRDF0E-CC69-41E2-B149-DBCDDC2622EA";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            var releaseType = StandardComponents.Instance.Configuration.ReleaseType;
            yield return new ConfigurationSection(SECTION, "Output")
                .WithElement(new SelectionConfigurationElement(RATE_ELEMENT, "Rate", path: "Advanced").WithOptions(GetRateOptions()))
                .WithElement(new SelectionConfigurationElement(DEPTH_ELEMENT, "Depth", path: "Advanced").WithOptions(GetDepthOptions()))
                .WithElement(new SelectionConfigurationElement(INPUT_ELEMENT, "Input"))
                .WithElement(new SelectionConfigurationElement(OUTPUT_ELEMENT, "Output"))
                .WithElement(new BooleanConfigurationElement(ENFORCE_RATE_ELEMENT, "Enforce Rate", path: "Advanced").WithValue(false))
                .WithElement(new BooleanConfigurationElement(PLAY_FROM_RAM_ELEMENT, "Play From Memory", path: "Advanced").WithValue(false))
                .WithElement(new IntegerConfigurationElement(BUFFER_LENGTH_ELEMENT, "Buffer Length", path: "Advanced").WithValue(500).WithValidationRule(new IntegerValidationRule(10, 5000)))
                .WithElement(new BooleanConfigurationElement(VOLUME_ENABLED_ELEMENT, "Software Volume Control", path: "Advanced").WithValue(false))
                .WithElement(new DoubleConfigurationElement(VOLUME_ELEMENT, "Software Volume Level", path: "Advanced").WithValue(1).WithValidationRule(new DoubleValidationRule(0, 1, 0.01)).DependsOn(SECTION, VOLUME_ENABLED_ELEMENT))
                .WithElement(new IntegerConfigurationElement(RESAMPLE_QUALITY_ELEMENT, "Resampling Quality", path: "Advanced").WithValue(2).WithValidationRule(new IntegerValidationRule(1, 10)))
                .WithElement(new BooleanConfigurationElement(DEVICE_MONITOR_ELEMENT, "Monitor Device Settings", path: "Advanced").WithValue(releaseType == ReleaseType.Default)
            );
        }

        public static IEnumerable<SelectionConfigurationOption> GetRateOptions()
        {
            foreach (var rate in OutputRate.PCM)
            {
                yield return new SelectionConfigurationOption(rate.ToString(), rate.ToString());
            }
        }

        public static int GetRate(SelectionConfigurationOption option)
        {
            var rate = default(int);
            if (int.TryParse(option.Id, out rate))
            {
                return rate;
            }
            return OutputRate.PCM_44100;
        }

        public static IEnumerable<SelectionConfigurationOption> GetDepthOptions()
        {
            var releaseType = StandardComponents.Instance.Configuration.ReleaseType;
            var i16 = new SelectionConfigurationOption(DEPTH_16_OPTION, "16bit");
            var f32 = new SelectionConfigurationOption(DEPTH_32_OPTION, "32bit floating point");
            if (releaseType == ReleaseType.Minimal)
            {
                i16.Default();
            }
            else
            {
                f32.Default();
            }
            yield return i16;
            yield return f32;
        }

        public static bool GetFloat(SelectionConfigurationOption option)
        {
            switch (option.Id)
            {
                default:
                case DEPTH_16_OPTION:
                    return false;
                case DEPTH_32_OPTION:
                    return true;
            }
        }
    }
}
