using System;

namespace FoxTunes
{
    public class BooleanDependency : Dependency
    {
        public BooleanDependency(string sectionId, string elementId) : base(sectionId, elementId)
        {

        }

        public override bool Validate(ConfigurationElement element)
        {
            if (element is BooleanConfigurationElement booleanConfigurationElement)
            {
                return booleanConfigurationElement.Value;
            }
            return false;
        }

        public override void AddHandler(ConfigurationElement element, EventHandler handler)
        {
            if (element is BooleanConfigurationElement booleanConfigurationElement)
            {
                booleanConfigurationElement.ValueChanged += handler;
            }
        }
    }
}
