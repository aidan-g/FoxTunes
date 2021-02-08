using System.Collections.Generic;

namespace FoxTunes
{
    public static class ProfilesBehaviourConfiguration
    {
        public const string SECTION = "B9886870-FBB3-4C9D-9C9F-741E217F023A";

        public const string ENABLED = "CA700DBD-5F0F-45BF-8E91-0FE0574381FE";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(SECTION, Strings.ProfilesBehaviourConfiguration_Section)
                .WithElement(new BooleanConfigurationElement(ENABLED, Strings.ProfilesBehaviourConfiguration_Enabled).WithValue(false));
        }
    }
}
