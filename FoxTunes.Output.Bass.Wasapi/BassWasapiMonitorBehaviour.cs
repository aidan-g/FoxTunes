namespace FoxTunes
{
    [PlatformDependency(Major = 6, Minor = 0)]
    [ComponentDependency(Slot = ComponentSlots.Output)]
    public class BassWasapiMonitorBehaviour : BassDeviceMonitorBehaviour
    {
        public BassWasapiMonitorBehaviour() : base(BassWasapiStreamOutputConfiguration.OUTPUT_WASAPI_OPTION)
        {

        }

        protected override bool RestartRequired(DataFlow? flow, Role? role)
        {
            if (!BassWasapiDevice.IsDefaultDevice)
            {
                return false;
            }
            return base.RestartRequired(flow, role);
        }
    }
}