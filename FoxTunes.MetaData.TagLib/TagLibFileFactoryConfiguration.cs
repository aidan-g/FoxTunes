using System.Collections.Generic;

namespace FoxTunes
{
    public static class TagLibFileFactoryConfiguration
    {
        public const string SECTION = MetaDataBehaviourConfiguration.SECTION;

        public const string READ_WINDOWS_MEDIA_TAGS = "JJKK0057-52AD-4914-BF4A-692EF76C7C83";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(SECTION)
                .WithElement(new BooleanConfigurationElement(READ_WINDOWS_MEDIA_TAGS, "Windows Media Tags").WithValue(false).DependsOn(MetaDataBehaviourConfiguration.SECTION, MetaDataBehaviourConfiguration.ENABLE_ELEMENT)
            );
        }
    }
}
