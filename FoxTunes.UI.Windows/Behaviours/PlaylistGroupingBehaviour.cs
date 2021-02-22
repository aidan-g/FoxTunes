#if NET40

//ListView grouping is too slow under net40 due to lack of virtualization.

#else

using FoxTunes.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.UserInterface)]
    public class PlaylistGroupingBehaviour : StandardBehaviour, IInvocableComponent, IConfigurableComponent
    {
        public const string TOGGLE_GROUPING = "MMMM";

        public IConfiguration Configuration { get; private set; }

        public BooleanConfigurationElement Grouping { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Configuration = core.Components.Configuration;
            this.Grouping = this.Configuration.GetElement<BooleanConfigurationElement>(
                PlaylistBehaviourConfiguration.SECTION,
                PlaylistGroupingBehaviourConfiguration.GROUP_ENABLED_ELEMENT
            );
            base.InitializeComponent(core);
        }

        public IEnumerable<IInvocationComponent> Invocations
        {
            get
            {
                yield return new InvocationComponent(
                    InvocationComponent.CATEGORY_PLAYLIST_HEADER,
                    TOGGLE_GROUPING,
                    Strings.PlaylistGroupingBehaviourConfiguration_Enabled,
                    attributes: this.Grouping.Value ? InvocationComponent.ATTRIBUTE_SELECTED : InvocationComponent.ATTRIBUTE_NONE
                );
            }
        }

        public Task InvokeAsync(IInvocationComponent component)
        {
            switch (component.Id)
            {
                case TOGGLE_GROUPING:
                    this.Grouping.Toggle();
                    this.Configuration.Save();
                    break;
            }
            return Task.CompletedTask;
        }

        public IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return PlaylistGroupingBehaviourConfiguration.GetConfigurationSections();
        }
    }
}

#endif