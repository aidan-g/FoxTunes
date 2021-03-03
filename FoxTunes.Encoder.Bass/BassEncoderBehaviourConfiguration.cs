using System;
using System.Collections.Generic;
using System.IO;

namespace FoxTunes
{
    public static class BassEncoderBehaviourConfiguration
    {
        public const string SECTION = "D1D2586A-8FE3-4F1E-A5A2-58C57A99FB11";

        public const string ENABLED_ELEMENT = "AAAA1152-CA98-4FE5-B63B-B6DD90E11B88";

        public const string DESTINATION_ELEMENT = "BBBBBF10-E515-4A99-B49B-4C5821A22945";

        public const string DESTINATION_BROWSE_OPTION = "AAAA0072-4E23-4735-BF80-8AD72A4AE927";

        public const string DESTINATION_SOURCE_OPTION = "BBBB2B59-96E2-49F3-A727-1E102D7C9D3F";

        public const string DESTINATION_SPECIFIC_OPTION = "CCCC1412-8AAE-4191-A920-8BB042D21F39";

        public const string DESTINATION_LOCATION_ELEMENT = "CCCC1639-B91A-44A1-8579-364639C3F52C";

        public const string COPY_TAGS = "DDDD37B0-7CB2-4E55-9DD4-8B1F7DEF0EC9";

        public const string THREADS_ELEMENT = "EEEE8113-0C57-464D-B370-C3B77AC048D2";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(SECTION, Strings.General_Converter)
                .WithElement(
                    new BooleanConfigurationElement(ENABLED_ELEMENT, Strings.BassEncoderBehaviourConfiguration_Enabled)
                        .WithValue(false))
                .WithElement(
                    new SelectionConfigurationElement(DESTINATION_ELEMENT, Strings.BassEncoderBehaviourConfiguration_Destination)
                        .WithOptions(GetDestinationOptions())
                        .DependsOn(SECTION, ENABLED_ELEMENT))
                .WithElement(
                    new TextConfigurationElement(DESTINATION_LOCATION_ELEMENT, Strings.BassEncoderBehaviourConfiguration_Location)
                        .WithValue(Path.Combine(Publication.StoragePath, Strings.General_Converter))
                        .WithFlags(ConfigurationElementFlags.FolderName)
                        .DependsOn(SECTION, ENABLED_ELEMENT)
                        .DependsOn(SECTION, DESTINATION_ELEMENT, DESTINATION_SPECIFIC_OPTION))
                .WithElement(
                    new BooleanConfigurationElement(COPY_TAGS, Strings.BassEncoderBehaviourConfiguration_CopyTags)
                    .WithValue(true)
                    .DependsOn(SECTION, ENABLED_ELEMENT))
                .WithElement(
                    new IntegerConfigurationElement(THREADS_ELEMENT, Strings.BassEncoderBehaviourConfiguration_Threads)
                    .WithValue(Environment.ProcessorCount)
                    .WithValidationRule(new IntegerValidationRule(1, 32))
                    .DependsOn(SECTION, ENABLED_ELEMENT)
            );
        }

        private static IEnumerable<SelectionConfigurationOption> GetDestinationOptions()
        {
            yield return new SelectionConfigurationOption(DESTINATION_BROWSE_OPTION, Strings.BassEncoderBehaviourConfiguration_BrowseFolder);
            yield return new SelectionConfigurationOption(DESTINATION_SOURCE_OPTION, Strings.BassEncoderBehaviourConfiguration_SourceFolder);
            yield return new SelectionConfigurationOption(DESTINATION_SPECIFIC_OPTION, Strings.BassEncoderBehaviourConfiguration_SpecificFolder);
        }

        public static BassEncoderOutputDestination GetDestination(SelectionConfigurationOption option)
        {
            switch (option.Id)
            {
                default:
                case DESTINATION_BROWSE_OPTION:
                    return BassEncoderOutputDestination.Browse;
                case DESTINATION_SOURCE_OPTION:
                    return BassEncoderOutputDestination.Source;
                case DESTINATION_SPECIFIC_OPTION:
                    return BassEncoderOutputDestination.Specific;
            }
        }
    }
}
