using System;
using System.Runtime.InteropServices;

namespace FoxTunes
{
    [Guid("0BD7A1BE-7A1A-44DB-8397-CC5392387B5E"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMMDeviceCollection
    {
        int GetCount(out int count);

        int Item(int number, out IMMDevice device);
    }
}