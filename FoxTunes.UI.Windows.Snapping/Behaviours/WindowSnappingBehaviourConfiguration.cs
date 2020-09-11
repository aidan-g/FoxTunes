using System;
using System.Collections.Generic;

namespace FoxTunes
{
    public static class WindowSnappingBehaviourConfiguration
    {
        public const string SECTION = "735BD4BE-5E73-4DE1-A5C7-10058059B436";

        public const string ENABLED = "AAAAEC3B-7C68-4CCC-AB7A-11257BC30374";

        public const string PROXIMITY = "BBBBCABB-8382-435A-B03E-294166AE7559";

        public const string STICKY = "CCCC9892-313E-4E03-8642-DBAA04D2A022";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(SECTION, "Window Snapping")
                .WithElement(new BooleanConfigurationElement(ENABLED, "Enabled").WithValue(false))
                .WithElement(new IntegerConfigurationElement(PROXIMITY, "Proximity").WithValue(20).WithValidationRule(new IntegerValidationRule(10, 40)))
                .WithElement(new TextConfigurationElement(STICKY, "Sticky").WithValue(string.Empty).WithFlags(ConfigurationElementFlags.MultiLine));
        }

        public static bool GetIsSticky(string value, string id)
        {
            var sequence = value.Split(new[]
            {
                Environment.NewLine
            }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var element in sequence)
            {
                if (!string.Equals(element.Trim(), id.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                return true;
            }
            return false;
        }
    }
}
