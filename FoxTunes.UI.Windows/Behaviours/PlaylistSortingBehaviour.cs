using FoxTunes.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.UserInterface)]
    public class PlaylistSortingBehaviour : StandardBehaviour, IInvocableComponent, IConfigurableComponent
    {
        public const string TOGGLE_SORTING = "NNNN";

        public IConfiguration Configuration { get; private set; }

        public BooleanConfigurationElement Sorting { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Configuration = core.Components.Configuration;
            this.Sorting = this.Configuration.GetElement<BooleanConfigurationElement>(
                PlaylistBehaviourConfiguration.SECTION,
                PlaylistSortingBehaviourConfiguration.SORT_ENABLED_ELEMENT
            );
            base.InitializeComponent(core);
        }

        public IEnumerable<IInvocationComponent> Invocations
        {
            get
            {
                yield return new InvocationComponent(
                    InvocationComponent.CATEGORY_PLAYLIST_HEADER,
                    TOGGLE_SORTING,
                    Strings.PlaylistSortingBehaviourConfiguration_Enabled,
                    attributes: this.Sorting.Value ? InvocationComponent.ATTRIBUTE_SELECTED : InvocationComponent.ATTRIBUTE_NONE
                );
            }
        }

        public Task InvokeAsync(IInvocationComponent component)
        {
            switch (component.Id)
            {
                case TOGGLE_SORTING:
                    this.Sorting.Toggle();
                    this.Configuration.Save();
                    break;
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        public IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return PlaylistSortingBehaviourConfiguration.GetConfigurationSections();
        }
    }
}