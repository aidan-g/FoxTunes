using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.UserInterface)]
    public class ProfilesBehaviour : StandardBehaviour, IInvocableComponent, IConfigurableComponent
    {
        public const string LOAD = "ZAAA";

        public const string NEW = "ZBBB";

        public const string DELETE = "ZCCC";

        public IUserInterface UserInterface { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public BooleanConfigurationElement Enabled { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.UserInterface = core.Components.UserInterface;
            this.Configuration = core.Components.Configuration;
            this.Enabled = this.Configuration.GetElement<BooleanConfigurationElement>(
                ProfilesBehaviourConfiguration.SECTION,
                ProfilesBehaviourConfiguration.ENABLED
            );
            base.InitializeComponent(core);
        }
        public IEnumerable<IInvocationComponent> Invocations
        {
            get
            {
                if (this.Enabled.Value)
                {
                    foreach (var profile in this.Configuration.AvailableProfiles)
                    {
                        yield return new InvocationComponent(
                            InvocationComponent.CATEGORY_SETTINGS,
                            LOAD,
                            profile,
                            path: Strings.ProfilesBehaviour_Path,
                            attributes: string.Equals(profile, this.Configuration.Profile, StringComparison.OrdinalIgnoreCase) ? InvocationComponent.ATTRIBUTE_SELECTED : InvocationComponent.ATTRIBUTE_NONE
                        );
                    }
                    yield return new InvocationComponent(
                        InvocationComponent.CATEGORY_SETTINGS,
                        NEW,
                        Strings.ProfilesBehaviour_New,
                        path: Strings.ProfilesBehaviour_Path,
                        attributes: InvocationComponent.ATTRIBUTE_SEPARATOR
                    );
                    if (!this.Configuration.IsDefaultProfile)
                    {
                        yield return new InvocationComponent(
                            InvocationComponent.CATEGORY_SETTINGS,
                            DELETE,
                            Strings.ProfilesBehaviour_Delete,
                            path: Strings.ProfilesBehaviour_Path
                        );
                    }
                }
            }
        }

        public Task InvokeAsync(IInvocationComponent component)
        {
            switch (component.Id)
            {
                case LOAD:
                    return this.Load(component.Name);
                case NEW:
                    return this.New();
                case DELETE:
                    return this.Delete();
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        public Task Load(string profile)
        {
            if (!string.Equals(this.Configuration.Profile, profile, StringComparison.OrdinalIgnoreCase))
            {
                this.Configuration.Load(profile);
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        public Task New()
        {
            var profile = this.UserInterface.Prompt(Strings.ProfilesBehaviour_New_Prompt);
            if (!string.IsNullOrEmpty(profile))
            {
                this.Configuration.Save(profile);
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        public Task Delete()
        {
            this.Configuration.Delete();
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        public IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return ProfilesBehaviourConfiguration.GetConfigurationSections();
        }
    }
}
