using FoxTunes.Interfaces;
using ManagedBass.Wasapi;
using System;
using System.Collections.Generic;

namespace FoxTunes
{
    public static class BassWasapiStreamOutputConfiguration
    {
        public const string SECTION = BassOutputConfiguration.SECTION;

        public const string OUTPUT_ELEMENT = BassOutputConfiguration.OUTPUT_ELEMENT;

        public const string ELEMENT_WASAPI_DEVICE = "AAAA9BD4-C90C-46A8-A486-2EBAE7152051";

        public const string OUTPUT_WASAPI_OPTION = "BBBB737C-0B07-4842-A59A-03679E1716F3";

        public const string ELEMENT_WASAPI_EXCLUSIVE = "CCCC20A2-3A5B-4F64-A574-89561114AAF4";

        public const string ELEMENT_WASAPI_EVENT = "DDDD16CF-03A5-4DDC-BE23-2C619D21F447";

        public const string ELEMENT_WASAPI_ASYNC = "DDEEEC5F-D47E-4EE3-9B48-AEC33E056E26";

        public const string ELEMENT_WASAPI_DITHER = "EEEEBF20-7B2A-489F-9FBD-E6FE5458F6B5";

        public const string ELEMENT_WASAPI_RAW = "EEFF5737-B44C-48BF-83A0-F7592FD101ED";

        public const string MIXER_ELEMENT = "FFFF34F9-BB72-4DB6-BDD0-F5C9BFD2F9EE";

        public const string DOUBLE_BUFFER_ELEMENT = "FFGGCB2C-0C9B-420A-8C74-862E9D9052B5";

        public const string BUFFER_LENGTH_ELEMENT = "FGGGD382-F6FD-485F-BB6E-40CA0B661D2B";

        public const string ELEMENT_REFRESH = "ZZZZ5945-E6DA-48FD-B89C-F1F35C4822FB";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(SECTION, Strings.BassWasapiStreamOutputConfiguration_Section)
                .WithElement(new SelectionConfigurationElement(OUTPUT_ELEMENT, Strings.BassWasapiStreamOutputConfiguration_Mode)
                    .WithOptions(new[] { new SelectionConfigurationOption(OUTPUT_WASAPI_OPTION, Strings.WASAPI) })
                    .DependsOn(SECTION, OUTPUT_ELEMENT, OUTPUT_WASAPI_OPTION))
                .WithElement(new SelectionConfigurationElement(ELEMENT_WASAPI_DEVICE, Strings.BassWasapiStreamOutputConfiguration_Device, path: Strings.WASAPI)
                    .WithOptions(GetWASAPIDevices())
                    .DependsOn(SECTION, OUTPUT_ELEMENT, OUTPUT_WASAPI_OPTION))
                .WithElement(new BooleanConfigurationElement(ELEMENT_WASAPI_EXCLUSIVE, Strings.BassWasapiStreamOutputConfiguration_Exclusive, path: Strings.WASAPI)
                    .WithValue(true)
                    .DependsOn(SECTION, OUTPUT_ELEMENT, OUTPUT_WASAPI_OPTION))
                .WithElement(new BooleanConfigurationElement(ELEMENT_WASAPI_EVENT, Strings.BassWasapiStreamOutputConfiguration_Event, path: Strings.WASAPI)
                    .WithValue(false)
                    .DependsOn(SECTION, OUTPUT_ELEMENT, OUTPUT_WASAPI_OPTION))
                .WithElement(new BooleanConfigurationElement(ELEMENT_WASAPI_ASYNC, Strings.BassWasapiStreamOutputConfiguration_Async, path: Strings.WASAPI)
                    .WithValue(false)
                    .DependsOn(SECTION, OUTPUT_ELEMENT, OUTPUT_WASAPI_OPTION)
                    .DependsOn(SECTION, ELEMENT_WASAPI_EXCLUSIVE)
                    .DependsOn(SECTION, ELEMENT_WASAPI_EVENT))
                .WithElement(new BooleanConfigurationElement(ELEMENT_WASAPI_DITHER, Strings.BassWasapiStreamOutputConfiguration_Dither, path: Strings.WASAPI)
                    .WithValue(false)
                    .DependsOn(SECTION, OUTPUT_ELEMENT, OUTPUT_WASAPI_OPTION))
                .WithElement(new BooleanConfigurationElement(MIXER_ELEMENT, Strings.BassWasapiStreamOutputConfiguration_Mixer, path: Strings.WASAPI)
                    .WithValue(false)
                    .DependsOn(SECTION, OUTPUT_ELEMENT, OUTPUT_WASAPI_OPTION))
                .WithElement(new BooleanConfigurationElement(DOUBLE_BUFFER_ELEMENT, Strings.BassWasapiStreamOutputConfiguration_DoubleBuffer, path: Strings.WASAPI)
                    .WithValue(true)
                    .DependsOn(SECTION, OUTPUT_ELEMENT, OUTPUT_WASAPI_OPTION))
                .WithElement(new DoubleConfigurationElement(BUFFER_LENGTH_ELEMENT, Strings.BassWasapiStreamOutputConfiguration_BufferLength, path: Strings.WASAPI)
                    .WithValue(BassWasapiDevice.DEFAULT_BUFFER_LENGTH)
                    .WithValidationRule(new DoubleValidationRule(BassWasapiDevice.MIN_BUFFER_LENGTH, BassWasapiDevice.MAX_BUFFER_LENGTH, 0.1))
                    .DependsOn(SECTION, OUTPUT_ELEMENT, OUTPUT_WASAPI_OPTION))
                .WithElement(new BooleanConfigurationElement(ELEMENT_WASAPI_RAW, Strings.BassWasapiStreamOutputConfiguration_Raw, path: Strings.WASAPI)
                    .WithValue(false)
                    .DependsOn(SECTION, OUTPUT_ELEMENT, OUTPUT_WASAPI_OPTION))
                .WithElement(new CommandConfigurationElement(ELEMENT_REFRESH, Strings.BassWasapiStreamOutputConfiguration_RefreshDevices, path: Strings.WASAPI)
                    .WithHandler(() =>
                    {
                        var element = StandardComponents.Instance.Configuration.GetElement<SelectionConfigurationElement>(
                            BassOutputConfiguration.SECTION,
                            ELEMENT_WASAPI_DEVICE
                        );
                        if (element == null)
                        {
                            return;
                        }
                        element.WithOptions(GetWASAPIDevices(), true);
                    })
                    .DependsOn(SECTION, OUTPUT_ELEMENT, OUTPUT_WASAPI_OPTION)
            );
        }

        public static int GetWasapiDevice(SelectionConfigurationOption option)
        {
            if (option != null && !string.Equals(option.Id, BassWasapi.DefaultDevice.ToString()))
            {
                for (int a = 0, b = BassWasapi.DeviceCount; a < b; a++)
                {
                    var deviceInfo = default(WasapiDeviceInfo);
                    BassUtils.OK(BassWasapi.GetDeviceInfo(a, out deviceInfo));
                    if (string.Equals(deviceInfo.ID, option.Id, StringComparison.OrdinalIgnoreCase))
                    {
                        return a;
                    }
                }
            }
            return BassWasapi.DefaultDevice;
        }

        private static IEnumerable<SelectionConfigurationOption> GetWASAPIDevices()
        {
            yield return new SelectionConfigurationOption(BassWasapi.DefaultDevice.ToString(), "Default Device");
            for (int a = 0, b = BassWasapi.DeviceCount; a < b; a++)
            {
                var deviceInfo = default(WasapiDeviceInfo);
                BassUtils.OK(BassWasapi.GetDeviceInfo(a, out deviceInfo));
                if (deviceInfo.IsInput || !deviceInfo.IsEnabled || deviceInfo.IsDisabled || deviceInfo.IsLoopback || deviceInfo.IsUnplugged)
                {
                    continue;
                }
                LogManager.Logger.Write(typeof(BassWasapiStreamOutputConfiguration), LogLevel.Debug, "WASAPI Device: {0} => {1} => {2} => {3} => {4}", a, deviceInfo.ID, deviceInfo.Name, Enum.GetName(typeof(WasapiDeviceType), deviceInfo.Type), deviceInfo.MixFrequency);
                yield return new SelectionConfigurationOption(deviceInfo.ID, deviceInfo.Name, string.Format("{0} ({1})", deviceInfo.Name, Enum.GetName(typeof(WasapiDeviceType), deviceInfo.Type)));
            }
        }
    }
}
