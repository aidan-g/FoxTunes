using FoxTunes.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System;

namespace FoxTunes
{
    public abstract class BaseLoader<T> : BaseComponent, IBaseLoader<T> where T : IBaseComponent
    {
        public virtual IEnumerable<T> Load(ICore core)
        {
            var components = new List<T>();
            foreach (var type in ComponentScanner.Instance.GetComponents(typeof(T)))
            {
                if (!this.IncludeType(type))
                {
                    continue;
                }
                var component = default(T);
                try
                {
                    component = ComponentActivator.Instance.Activate<T>(type);
                }
                catch (Exception e)
                {
                    Logger.Write(this, LogLevel.Warn, "Failed to activate component: {0} => {1}: {2}", type.Name, typeof(T).Name, e.Message);
                    continue;
                }
                Logger.Write(this, LogLevel.Debug, "Activated component: {0} => {1}", type.Name, typeof(T).Name);
                components.Add(component);
            }
            return components.OrderBy(this.ComponentPriority);
        }

        public virtual bool IncludeType(Type type)
        {
            var dependencies = default(IEnumerable<ComponentDependencyAttribute>);
            if (type.HasCustomAttributes<ComponentDependencyAttribute>(out dependencies))
            {
                foreach (var dependency in dependencies)
                {
                    if (!string.IsNullOrEmpty(dependency.Id))
                    {
                        throw new NotImplementedException();
                    }
                    if (!string.IsNullOrEmpty(dependency.Slot))
                    {
                        var id = ComponentResolver.Instance.Get(dependency.Slot);
                        if (string.Equals(id, ComponentSlots.Blocked, StringComparison.OrdinalIgnoreCase))
                        {
                            Logger.Write(this, LogLevel.Debug, "Not loading component \"{0}\": Missing dependency \"{1}\".", type.FullName, dependency.Slot);
                            return false;
                        }
                    }
                }
            }
            var component = default(ComponentAttribute);
            if (!type.HasCustomAttribute<ComponentAttribute>(out component))
            {
                return true;
            }
            if (component == null)
            {
                return true;
            }
            {
                var id = ComponentResolver.Instance.Get(component.Slot);
                if (string.Equals(id, ComponentSlots.None, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
                if (string.Equals(id, component.Id, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            Logger.Write(this, LogLevel.Debug, "Not loading component \"{0}\": Blocked by configuration.", type.FullName);
            return false;
        }

        public Func<T, byte> ComponentPriority
        {
            get
            {
                return component =>
                {
                    var attribute = component.GetType().GetCustomAttribute<ComponentAttribute>();
                    if (attribute == null)
                    {
                        return ComponentAttribute.PRIORITY_NORMAL;
                    }
                    return attribute.Priority;
                };
            }
        }
    }
}
