using System;
using System.Runtime.InteropServices;

namespace FoxTunes
{
    [Guid("886d8eeb-8cf2-4446-8d02-cdba1dbdcf99"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IPropertyStore
    {
        int GetCount(out int count);

        int GetAt(int property, out PropertyKey key);

        int GetValue(ref PropertyKey key, out PropVariant value);

        int SetValue(ref PropertyKey key, ref PropVariant value);

        int Commit();
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct PropVariant
    {
        [FieldOffset(0)]
        private short vt;

        [FieldOffset(2)]
        private readonly short wReserved1;

        [FieldOffset(4)]
        private readonly short wReserved2;

        [FieldOffset(6)]
        private readonly short wReserved3;

        [FieldOffset(8)]
        private readonly sbyte cVal;

        [FieldOffset(8)]
        private readonly byte bVal;

        [FieldOffset(8)]
        private readonly short iVal;

        [FieldOffset(8)]
        private readonly ushort uiVal;

        [FieldOffset(8)]
        private readonly int lVal;

        [FieldOffset(8)]
        private readonly uint ulVal;

        [FieldOffset(8)]
        private readonly int intVal;

        [FieldOffset(8)]
        private readonly uint uintVal;

        [FieldOffset(8)]
        private long hVal;

        [FieldOffset(8)]
        private readonly long uhVal;

        [FieldOffset(8)]
        private readonly float fltVal;

        [FieldOffset(8)]
        private readonly double dblVal;

        [FieldOffset(8)]
        private readonly bool boolVal;

        [FieldOffset(8)]
        private readonly int scode;

        [FieldOffset(8)]
        private readonly DateTime date;

        [FieldOffset(8)]
        private readonly global::System.Runtime.InteropServices.ComTypes.FILETIME filetime;

        [FieldOffset(8)]
        private Blob blobVal;

        [FieldOffset(8)]
        private readonly IntPtr pointerValue;
    }

    internal struct Blob
    {
#pragma warning disable 0649
        public IntPtr Data;
#pragma warning restore 0649

#pragma warning disable 0649
        public int Length;
#pragma warning restore 0649
    }
}