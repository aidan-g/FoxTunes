using System;

namespace FoxTunes
{
    public class SelectionDependency : Dependency
    {
        public SelectionDependency(string sectionId, string elementId, string optionId) : base(sectionId, elementId)
        {
            this.OptionId = optionId;
        }

        public string OptionId { get; private set; }

        public override bool Validate(ConfigurationElement element)
        {
            if (element is SelectionConfigurationElement selectionConfigurationElement)
            {
                return string.Equals(selectionConfigurationElement.Value.Id, this.OptionId, StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }

        public override void AddHandler(ConfigurationElement element, EventHandler handler)
        {
            if (element is SelectionConfigurationElement selectionConfigurationElement)
            {
                selectionConfigurationElement.ValueChanged += handler;
            }
        }
    }
}
