using System;

namespace FoxTunes
{
    public static class DeviceEnumeratorFactory
    {
        private static readonly Guid ID = new Guid("BCDE0395-E52F-467C-8E3D-C4579291692E");

        public static IMMDeviceEnumerator Create()
        {
            var type = Type.GetTypeFromCLSID(ID);
            return (IMMDeviceEnumerator)Activator.CreateInstance(type);
        }
    }
}