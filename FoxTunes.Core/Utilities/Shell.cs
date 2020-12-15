using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace FoxTunes
{
    public static class Shell
    {
        public static string GetShortcut(string location)
        {
            if (!File.Exists(location))
            {
                return null;
            }
            var targetPath = default(string);
            var wshShell = (IWshShell)new WshShell();
            try
            {
                var shortcut = wshShell.CreateShortcut(location);
                try
                {
                    targetPath = shortcut.TargetPath;
                }
                finally
                {
                    Marshal.FinalReleaseComObject(shortcut);
                }
            }
            finally
            {
                Marshal.FinalReleaseComObject(wshShell);
            }
            return targetPath;
        }

        public static void CreateShortcut(string location, string target, IDictionary<PropertyKey, string> properties = null)
        {
            var shellLink = new CShellLink();

            try
            {
                var shellLinkW = (IShellLinkW)shellLink;
                shellLinkW.SetPath(target);

                if (properties != null && properties.Count > 0)
                {
                    if (shellLink is IPropertyStore propertyStore)
                    {
                        foreach (var key in properties.Keys)
                        {
                            var value = properties[key];
                            var property = new PropertyVariant();
                            property.SetValue(value);
                            propertyStore.SetValue(key, property);
                        }
                    }
                    else
                    {
                        //Properties were specified but the platform does not support them.
                    }
                }

                var persistFile = (IPersistFile)shellLink;
                persistFile.Save(location, true);
            }
            finally
            {
                Marshal.FinalReleaseComObject(shellLink);
            }
        }

        [ComImport, Guid("72C24DD5-D70A-438B-8A42-98424B88AFB8")]
        public class WshShell
        {
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIDispatch), Guid("F935DC21-1CF0-11D0-ADB9-00C04FD58A0B")]
        public interface IWshShell
        {
            IWshShortcut CreateShortcut(string pathLink);
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIDispatch), Guid("F935DC23-1CF0-11D0-ADB9-00C04FD58A0B")]
        public interface IWshShortcut
        {
            string FullName { get; }

            string Arguments { get; set; }

            string Description { get; set; }

            string Hotkey { get; set; }

            string IconLocation { get; set; }

            string RelativePath { set; }

            string TargetPath { get; set; }

            int WindowStyle { get; set; }

            string WorkingDirectory { get; set; }

            void Load([In] string pathLink);

            void Save();
        }

        [ComImport, Guid("000214F9-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IShellLinkW
        {
            void GetPath([Out(), MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile, int cchMaxPath, IntPtr pfd, uint fFlags);

            void GetIDList(out IntPtr ppidl);

            void SetIDList(IntPtr pidl);

            void GetDescription([Out(), MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile, int cchMaxName);

            void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);

            void GetWorkingDirectory([Out(), MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir, int cchMaxPath);

            void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);

            void GetArguments([Out(), MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs, int cchMaxPath);

            void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);

            void GetHotKey(out short wHotKey);

            void SetHotKey(short wHotKey);

            void GetShowCmd(out uint iShowCmd);

            void SetShowCmd(uint iShowCmd);

            void GetIconLocation([Out(), MarshalAs(UnmanagedType.LPWStr)] out StringBuilder pszIconPath, int cchIconPath, out int iIcon);

            void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);

            void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel, uint dwReserved);

            void Resolve(IntPtr hwnd, uint fFlags);

            void SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);
        }

        [ComImport, Guid("0000010b-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IPersistFile
        {
            void GetCurFile([Out(), MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile);

            void IsDirty();

            void Load([MarshalAs(UnmanagedType.LPWStr)] string pszFileName, [MarshalAs(UnmanagedType.U4)] long dwMode);

            void Save([MarshalAs(UnmanagedType.LPWStr)] string pszFileName, bool fRemember);

            void SaveCompleted([MarshalAs(UnmanagedType.LPWStr)] string pszFileName);
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct PropertyVariant
        {
            [FieldOffset(0)]
            public ushort vt;

            [FieldOffset(8)]
            public IntPtr unionmember;

            [FieldOffset(8)]
            public UInt64 forceStructToLargeEnoughSize;

            public void SetValue(Guid value)
            {
                PropVariantClear(ref this);
                byte[] guid = value.ToByteArray();
                this.vt = (ushort)VarEnum.VT_CLSID;
                this.unionmember = Marshal.AllocCoTaskMem(guid.Length);
                Marshal.Copy(guid, 0, this.unionmember, guid.Length);
            }

            public void SetValue(string val)
            {
                PropVariantClear(ref this);
                this.vt = (ushort)VarEnum.VT_LPWSTR;
                this.unionmember = Marshal.StringToCoTaskMemUni(val);
            }

            [DllImport("Ole32.dll", PreserveSig = false)]
            public static extern void PropVariantClear(ref PropertyVariant pvar);
        }

        [ComImport, Guid("886D8EEB-8CF2-4446-8D02-CDBA1DBDCF99"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IPropertyStore
        {
            void GetCount([Out] out uint propertyCount);

            void GetAt([In] uint propertyIndex, [Out, MarshalAs(UnmanagedType.Struct)] out PropertyKey key);

            void GetValue([In, MarshalAs(UnmanagedType.Struct)] ref PropertyKey key, [Out, MarshalAs(UnmanagedType.Struct)] out PropertyVariant pv);

            void SetValue([In, MarshalAs(UnmanagedType.Struct)] ref PropertyKey key, [In, MarshalAs(UnmanagedType.Struct)] ref PropertyVariant pv);

            void Commit();
        }

        [ComImport, Guid("00021401-0000-0000-C000-000000000046"), ClassInterface(ClassInterfaceType.None)]
        public class CShellLink
        {
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct PropertyKey
        {
            public Guid fmtid;
            public uint pid;

            public PropertyKey(Guid guid, uint id)
            {
                fmtid = guid;
                pid = id;
            }

            public static readonly PropertyKey AppUserModel_ID = new PropertyKey(new Guid("9F4C2855-9F79-4B39-A8D0-E1D42DE1D5F3"), 5);

            public static readonly PropertyKey AppUserModel_ToastActivatorCLSID = new PropertyKey(new Guid("9F4C2855-9F79-4B39-A8D0-E1D42DE1D5F3"), 26);
        }
    }
}
