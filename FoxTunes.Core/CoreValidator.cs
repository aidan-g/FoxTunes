using FoxTunes.Interfaces;
using System;
using System.Linq;

namespace FoxTunes
{
    public class CoreValidator : BaseComponent, ICoreValidator
    {
        public static Type[] REQUIRED = new[]
        {
            typeof(IStandardComponent),
            typeof(IStandardManager),
            typeof(IStandardBehaviour),
            typeof(IStandardFactory)
        };

        public bool Validate(ICore core)
        {
            return this.ValidateInterfaces(core) && this.ValidateSlots(core);
        }

        protected virtual bool ValidateInterfaces(ICore core)
        {
            var success = true;
            var types = typeof(ICore).Assembly
                .GetExportedTypes()
                .Where(IsRequiredInterface);
            foreach (var type in types)
            {
                if (ComponentRegistry.Instance.GetComponent(type) == null)
                {
                    if (core.Components.UserInterface != null)
                    {
                        core.Components.UserInterface.Warn(string.Format("Component missing or invalid for interface \"{0}\".", type.FullName));
                    }
                    success = false;
                }
            }
            return success;
        }

        protected virtual bool ValidateSlots(ICore core)
        {
            var success = true;
            foreach (var key in ComponentSlots.Lookup.Keys)
            {
                var value = ComponentSlots.Lookup[key];
                if (ComponentRegistry.Instance.GetComponents(value).Count() > 1)
                {
                    ComponentResolver.Instance.Add(value);
                    if (core.Components.UserInterface != null)
                    {
                        core.Components.UserInterface.Warn(string.Format("Multiple components are installed for slot \"{0}\".\nEdit {1} to resolve the conflict.", key, ComponentResolver.FILE_NAME.GetName()));
                    }
                    success = false;
                }
            }
            return success;
        }

        private static bool IsRequiredInterface(Type type)
        {
            return type.IsInterface && REQUIRED.Any(required => required != type && required.IsAssignableFrom(type));
        }

        public static readonly ICoreValidator Instance = new CoreValidator();
    }
}
