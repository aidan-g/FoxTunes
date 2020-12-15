using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace FoxTunes
{
    public static class ModernShell
    {
        public static class InterfaceGuids
        {
            public const string IShellLibrary = "11A66EFA-382E-451A-9234-1E0E12EF3085";

            public const string IShellItemArray = "B63EA76D-1F85-456F-A19C-48159EFA858B";
        }

        public static class ClassGuids
        {
            public const string ShellLibrary = "D9B3211D-E57F-4426-AAEF-30A806ADD397";
        }

        [DllImport("shell32.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
        public static extern void SHCreateShellItemArrayFromDataObject([In] global::System.Runtime.InteropServices.ComTypes.IDataObject pdo, [In, MarshalAs(UnmanagedType.LPStruct)] Guid riid, [MarshalAs(UnmanagedType.Interface)] out IShellItemArray ppv);

        [ComImport, Guid(ClassGuids.ShellLibrary), ClassInterface(ClassInterfaceType.None), TypeLibType(TypeLibTypeFlags.FCanCreate)]
        public class ShellLibrary
        {
        }

        [ComImport, Guid(InterfaceGuids.IShellLibrary), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IShellLibrary
        {
            [PreserveSig]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            HResult LoadLibraryFromItem([In, MarshalAs(UnmanagedType.Interface)] IShellItem library, [In] AccessModes grfMode);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void LoadLibraryFromKnownFolder([In] ref Guid knownfidLibrary, [In] AccessModes grfMode);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void AddFolder([In, MarshalAs(UnmanagedType.Interface)] IShellItem location);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void RemoveFolder([In, MarshalAs(UnmanagedType.Interface)] IShellItem location);

            [PreserveSig]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            HResult GetFolders([In] LibraryFolderFilter lff, [In] ref Guid riid, [MarshalAs(UnmanagedType.Interface)] out IShellItemArray ppv);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void ResolveFolder([In, MarshalAs(UnmanagedType.Interface)] IShellItem folderToResolve, [In] uint timeout, [In] ref Guid riid, [MarshalAs(UnmanagedType.Interface)] out IShellItem ppv);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void GetDefaultSaveFolder([In] DefaultSaveFolderType dsft, [In] ref Guid riid, [MarshalAs(UnmanagedType.Interface)] out IShellItem ppv);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void SetDefaultSaveFolder([In] DefaultSaveFolderType dsft, [In, MarshalAs(UnmanagedType.Interface)] IShellItem si);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void GetOptions(out LibraryOptions lofOptions);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void SetOptions([In] LibraryOptions lofMask, [In] LibraryOptions lofOptions);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void GetFolderType(out Guid ftid);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void SetFolderType([In] ref Guid ftid);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void GetIcon([MarshalAs(UnmanagedType.LPWStr)] out string icon);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void SetIcon([In, MarshalAs(UnmanagedType.LPWStr)] string icon);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void Commit();
        };

        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("b63ea76d-1f85-456f-a19c-48159efa858b")]
        public interface IShellItemArray
        {
            void BindToHandler([In] IntPtr pbc, [In, MarshalAs(UnmanagedType.LPStruct)] Guid bhid, [In, MarshalAs(UnmanagedType.LPStruct)] Guid riid, [Out] out IntPtr ppv);

            void GetPropertyStore([In] int flags, [In, MarshalAs(UnmanagedType.LPStruct)] Guid riid, [Out] out IntPtr ppv);

            void GetPropertyDescriptionList([In] int keyType, [In, MarshalAs(UnmanagedType.LPStruct)] Guid riid, [Out] out IntPtr ppv);

            void GetAttributes([In] int dwAttribFlags, [In] int sfgaoMask, [Out] out int psfgaoAttribs);

            void GetCount([Out] out uint pdwNumItems);

            void GetItemAt([In] uint dwIndex, [Out, MarshalAs(UnmanagedType.Interface)] out IShellItem ppsi);

            void EnumItems([Out] out IntPtr ppenumShellItems);
        }

        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("43826d1e-e718-42ee-bc55-a1e261c37bfe")]
        public interface IShellItem
        {
            HResult BindToHandler(IntPtr pbc, [MarshalAs(UnmanagedType.LPStruct)] Guid bhid, [MarshalAs(UnmanagedType.LPStruct)] Guid riid, out IntPtr obj);

            [PreserveSig]
            HResult GetParent(out IShellItem ppsi);

            IntPtr GetDisplayName(SIGDN sigdnName);

            [PreserveSig]
            HResult GetAttributes(SFGAO sfgaoMask, out SFGAO psfgaoAttribs);

            int Compare(IShellItem psi, SICHINT hint);
        };

        [Flags]
        public enum LibraryOptions
        {
            Default = 0,
            PinnedToNavigationPane = 0x1,
            MaskAll = 0x1
        };

        public enum LibrarySaveOptions
        {
            FailIfThere = 0,
            OverrideExisting = 1,
            MakeUniqueName = 2
        };

        public enum LibraryFolderFilter
        {
            ForceFileSystem = 1,
            StorageItems = 2,
            AllItems = 3
        };

        public enum DefaultSaveFolderType
        {
            Detect = 1,
            Private = 2,
            Public = 3
        };

        [Flags]
        public enum AccessModes
        {
            Direct = 0x00000000,
            Transacted = 0x00010000,
            Simple = 0x08000000,
            Read = 0x00000000,
            Write = 0x00000001,
            ReadWrite = 0x00000002,
            ShareDenyNone = 0x00000040,
            ShareDenyRead = 0x00000030,
            ShareDenyWrite = 0x00000020,
            ShareExclusive = 0x00000010,
            Priority = 0x00040000,
            DeleteOnRelease = 0x04000000,
            NoScratch = 0x00100000,
            Create = 0x00001000,
            Convert = 0x00020000,
            FailIfThere = 0x00000000,
            NoSnapshot = 0x00200000,
            DirectSingleWriterMultipleReader = 0x00400000
        };

        public enum SIGDN : uint
        {
            NORMALDISPLAY = 0,
            PARENTRELATIVEPARSING = 0x80018001,
            PARENTRELATIVEFORADDRESSBAR = 0x8001c001,
            DESKTOPABSOLUTEPARSING = 0x80028000,
            PARENTRELATIVEEDITING = 0x80031001,
            DESKTOPABSOLUTEEDITING = 0x8004c000,
            FILESYSPATH = 0x80058000,
            URL = 0x80068000
        }

        [Flags]
        public enum SFGAO : uint
        {
            CANCOPY = 0x00000001,
            CANMOVE = 0x00000002,
            CANLINK = 0x00000004,
            STORAGE = 0x00000008,
            CANRENAME = 0x00000010,
            CANDELETE = 0x00000020,
            HASPROPSHEET = 0x00000040,
            DROPTARGET = 0x00000100,
            CAPABILITYMASK = 0x00000177,
            ENCRYPTED = 0x00002000,
            ISSLOW = 0x00004000,
            GHOSTED = 0x00008000,
            LINK = 0x00010000,
            SHARE = 0x00020000,
            READONLY = 0x00040000,
            HIDDEN = 0x00080000,
            DISPLAYATTRMASK = 0x000FC000,
            STREAM = 0x00400000,
            STORAGEANCESTOR = 0x00800000,
            VALIDATE = 0x01000000,
            REMOVABLE = 0x02000000,
            COMPRESSED = 0x04000000,
            BROWSABLE = 0x08000000,
            FILESYSANCESTOR = 0x10000000,
            FOLDER = 0x20000000,
            FILESYSTEM = 0x40000000,
            HASSUBFOLDER = 0x80000000,
            CONTENTSMASK = 0x80000000,
            STORAGECAPMASK = 0x70C50008,
        }

        public enum SICHINT : uint
        {
            DISPLAY = 0x00000000,
            CANONICAL = 0x10000000,
            ALLFIELDS = 0x80000000
        }

        public enum HResult
        {
            DRAGDROP_S_CANCEL = 0x00040101,
            DRAGDROP_S_DROP = 0x00040100,
            DRAGDROP_S_USEDEFAULTCURSORS = 0x00040102,
            DATA_S_SAMEFORMATETC = 0x00040130,
            S_OK = 0,
            S_FALSE = 1,
            E_CANCELED = unchecked((int)0x800704C7),
            E_NOINTERFACE = unchecked((int)0x80004002),
            E_NOTIMPL = unchecked((int)0x80004001),
            OLE_E_ADVISENOTSUPPORTED = unchecked((int)80040003),
            MK_E_NOOBJECT = unchecked((int)0x800401E5),
            E_INVALIDARG = unchecked((int)0x80070057),
            WTS_E_FAILEDEXTRACTION = unchecked((int)0x8004b200),
            WTS_E_EXTRACTIONTIMEDOUT = unchecked((int)0x8004b201),
            WTS_E_SURROGATEUNAVAILABLE = unchecked((int)0x8004b202),
            WTS_E_FASTEXTRACTIONNOTSUPPORTED = unchecked((int)0x8004b203),
            WTS_E_DATAFILEUNAVAILABLE = unchecked((int)0x8004b204),
            STG_E_FILENOTFOUND = unchecked((int)0x80030002),
        }
    }
}
