using FoxTunes.Interfaces;
using ManagedBass;
using System;
using System.Collections.Generic;

namespace FoxTunes
{
    public static class BassDirectSoundStreamOutputConfiguration
    {
        public const string SECTION = BassOutputConfiguration.SECTION;

        public const string OUTPUT_ELEMENT = BassOutputConfiguration.OUTPUT_ELEMENT;

        public const string OUTPUT_DS_OPTION = "AAAA1348-069B-4763-89CF-5ACBE53E9F75";

        public const string ELEMENT_DS_DEVICE = "BBBBD4A5-4DD5-4985-A373-565335084B80";

        public const string ELEMENT_REFRESH = "CCCC104A-4E97-4BDC-A246-65D21EF22DE4";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(SECTION)
                .WithElement(new SelectionConfigurationElement(OUTPUT_ELEMENT)
                    .WithOptions(new[] { new SelectionConfigurationOption(OUTPUT_DS_OPTION, Strings.DirectSound).Default() })
                    .DependsOn(SECTION, OUTPUT_ELEMENT, OUTPUT_DS_OPTION))
                .WithElement(new SelectionConfigurationElement(ELEMENT_DS_DEVICE, "Device", path: Strings.DirectSound)
                    .WithOptions(GetDSDevices())
                    .DependsOn(SECTION, OUTPUT_ELEMENT, OUTPUT_DS_OPTION))
                .WithElement(new CommandConfigurationElement(ELEMENT_REFRESH, "Refresh Devices", path: Strings.DirectSound)
                    .WithHandler(() =>
                    {
                        var element = StandardComponents.Instance.Configuration.GetElement<SelectionConfigurationElement>(
                            BassOutputConfiguration.SECTION,
                            ELEMENT_DS_DEVICE
                        );
                        if (element == null)
                        {
                            return;
                        }
                        element.WithOptions(GetDSDevices(), true);
                    })
                    .DependsOn(SECTION, OUTPUT_ELEMENT, OUTPUT_DS_OPTION)
            );
        }

        public static int GetDsDevice(SelectionConfigurationOption option)
        {
            if (option != null)
            {
                if (!string.Equals(option.Id, Bass.DefaultDevice.ToString()))
                {
                    for (int a = 0, b = Bass.DeviceCount; a < b; a++)
                    {
                        var deviceInfo = default(DeviceInfo);
                        BassUtils.OK(Bass.GetDeviceInfo(a, out deviceInfo));
                        if (string.Equals(deviceInfo.Name, option.Id, StringComparison.OrdinalIgnoreCase))
                        {
                            return a;
                        }
                    }
                }
            }
            return Bass.DefaultDevice;
        }

        private static IEnumerable<SelectionConfigurationOption> GetDSDevices()
        {
            yield return new SelectionConfigurationOption(Bass.DefaultDevice.ToString(), "Default Device");
            for (int a = 0, b = Bass.DeviceCount; a < b; a++)
            {
                var deviceInfo = default(DeviceInfo);
                BassUtils.OK(Bass.GetDeviceInfo(a, out deviceInfo));
                LogManager.Logger.Write(typeof(BassDirectSoundStreamOutputConfiguration), LogLevel.Debug, "DS Device: {0} => {1} => {2}", a, deviceInfo.Name, deviceInfo.Driver);
                if (!deviceInfo.IsEnabled)
                {
                    continue;
                }
                yield return new SelectionConfigurationOption(deviceInfo.Name, deviceInfo.Name, deviceInfo.Driver);
            }
        }
    }
}
