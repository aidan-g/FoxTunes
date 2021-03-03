using FoxTunes.Interfaces;
using ManagedBass.Asio;
using System;
using System.Collections.Generic;

namespace FoxTunes
{
    public static class BassAsioStreamOutputConfiguration
    {
        public const int ASIO_NO_DEVICE = -1;

        public const string SECTION = BassOutputConfiguration.SECTION;

        public const string OUTPUT_ELEMENT = BassOutputConfiguration.OUTPUT_ELEMENT;

        public const string ELEMENT_ASIO_DEVICE = "AAAAB9CE-96FC-4FBB-8956-84B9A7E3FEB3";

        public const string DSD_RAW_ELEMENT = "BBBB043A-8A30-42A0-B2CB-3DE379636DD6";

        public const string OUTPUT_ASIO_OPTION = "CCCC87DA-EE55-467A-B2F5-61480F2F12F6";

        public const string MIXER_ELEMENT = "CCDDCD1A-32F6-44B0-81E5-C9CEF0652E69";

        public const string ELEMENT_REFRESH = "DDDDC8BE-2909-42C1-AC82-790EAEE2FB07";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(SECTION)
                .WithElement(new SelectionConfigurationElement(OUTPUT_ELEMENT)
                    .WithOptions(new[] { new SelectionConfigurationOption(OUTPUT_ASIO_OPTION, Strings.ASIO) })
                    .DependsOn(SECTION, OUTPUT_ELEMENT, OUTPUT_ASIO_OPTION))
                .WithElement(new SelectionConfigurationElement(ELEMENT_ASIO_DEVICE, "Device", path: Strings.ASIO)
                    .WithOptions(GetASIODevices())
                    .DependsOn(SECTION, OUTPUT_ELEMENT, OUTPUT_ASIO_OPTION))
                .WithElement(new BooleanConfigurationElement(DSD_RAW_ELEMENT, "DSD Direct", path: Strings.ASIO)
                    .WithValue(false)
                    .DependsOn(SECTION, OUTPUT_ELEMENT, OUTPUT_ASIO_OPTION))
                .WithElement(new BooleanConfigurationElement(MIXER_ELEMENT, "Mixer", path: Strings.ASIO)
                    .WithValue(false)
                    .DependsOn(SECTION, OUTPUT_ELEMENT, OUTPUT_ASIO_OPTION))
                .WithElement(new CommandConfigurationElement(ELEMENT_REFRESH, "Refresh Devices", path: Strings.ASIO)
                    .WithHandler(() =>
                    {
                        var element = StandardComponents.Instance.Configuration.GetElement<SelectionConfigurationElement>(
                            BassOutputConfiguration.SECTION,
                            ELEMENT_ASIO_DEVICE
                        );
                        if (element == null)
                        {
                            return;
                        }
                        element.WithOptions(GetASIODevices(), true);
                    })
                    .DependsOn(SECTION, OUTPUT_ELEMENT, OUTPUT_ASIO_OPTION)
            );
        }

        public static int GetAsioDevice(SelectionConfigurationOption option)
        {
            if (option != null)
            {
                for (int a = 0, b = BassAsio.DeviceCount; a < b; a++)
                {
                    var deviceInfo = default(AsioDeviceInfo);
                    BassAsioUtils.OK(BassAsio.GetDeviceInfo(a, out deviceInfo));
                    if (string.Equals(deviceInfo.Name, option.Id, StringComparison.OrdinalIgnoreCase))
                    {
                        return a;
                    }
                }
            }
            return ASIO_NO_DEVICE;
        }

        private static IEnumerable<SelectionConfigurationOption> GetASIODevices()
        {
            for (int a = 0, b = BassAsio.DeviceCount; a < b; a++)
            {
                var deviceInfo = default(AsioDeviceInfo);
                BassAsioUtils.OK(BassAsio.GetDeviceInfo(a, out deviceInfo));
                LogManager.Logger.Write(typeof(BassAsioStreamOutputConfiguration), LogLevel.Debug, "ASIO Device: {0} => {1} => {2}", a, deviceInfo.Name, deviceInfo.Driver);
                yield return new SelectionConfigurationOption(deviceInfo.Name, deviceInfo.Name, deviceInfo.Driver);
            }
        }
    }
}
