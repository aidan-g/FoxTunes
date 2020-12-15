namespace FoxTunes
{
    [PlatformDependency(Major = 6, Minor = 0)]
    [ComponentDependency(Slot = ComponentSlots.Output)]
    public class BassDirectSoundMonitorBehaviour : BassDeviceMonitorBehaviour
    {
        public BassDirectSoundMonitorBehaviour() : base(BassDirectSoundStreamOutputConfiguration.OUTPUT_DS_OPTION)
        {

        }

        protected override bool RestartRequired(DataFlow? flow, Role? role)
        {
            if (!BassDirectSoundDevice.IsDefaultDevice)
            {
                return false;
            }
            return base.RestartRequired(flow, role);
        }
    }
}