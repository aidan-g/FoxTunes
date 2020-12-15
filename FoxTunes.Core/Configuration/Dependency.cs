using System;

namespace FoxTunes
{
    public abstract class Dependency
    {
        protected Dependency(string sectionId, string elementId)
        {
            this.SectionId = sectionId;
            this.ElementId = elementId;
        }

        public string SectionId { get; private set; }

        public string ElementId { get; private set; }

        public abstract bool Validate(ConfigurationElement element);

        public abstract void AddHandler(ConfigurationElement element, EventHandler handler);
    }
}
