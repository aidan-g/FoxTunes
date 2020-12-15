using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;

namespace FoxTunes
{
    public static class ShellIDListHelper
    {
        const string DataType = "Shell IDList Array";

        public static bool GetDataPresent(IDataObject dataObject)
        {
            return dataObject.GetDataPresent(DataType);
        }

        public static IEnumerable<string> GetData(IDataObject dataObject)
        {
            var shellItems = default(ModernShell.IShellItemArray);
            ModernShell.SHCreateShellItemArrayFromDataObject(
                (global::System.Runtime.InteropServices.ComTypes.IDataObject)dataObject,
                new Guid(ModernShell.InterfaceGuids.IShellItemArray),
                out shellItems
            );

            try
            {
                foreach (var shellItem in GetItems(shellItems))
                {
                    foreach (var folder in GetFolders(shellItem))
                    {
                        yield return folder;
                    }
                    Marshal.ReleaseComObject(shellItem);
                }
            }
            finally
            {
                Marshal.ReleaseComObject(shellItems);
            }
        }

        private static IEnumerable<ModernShell.IShellItem> GetItems(ModernShell.IShellItemArray shellItems)
        {
            var count = default(uint);
            shellItems.GetCount(out count);
            for (var position = default(uint); position < count; position++)
            {
                var shellItem = default(ModernShell.IShellItem);
                shellItems.GetItemAt(position, out shellItem);
                yield return shellItem;
            }
        }

        private static string GetDisplayName(ModernShell.IShellItem shellItem, ModernShell.SIGDN sigdn)
        {
            var pointer = shellItem.GetDisplayName(sigdn);
            var result = Marshal.PtrToStringUni(pointer);
            Marshal.FreeCoTaskMem(pointer);
            return result;
        }

        private static IEnumerable<string> GetFolders(ModernShell.IShellItem shellItem)
        {
            var id = new Guid(ModernShell.InterfaceGuids.IShellItemArray);
            var shellLibrary = (ModernShell.IShellLibrary)new ModernShell.ShellLibrary();
            var shellItems = default(ModernShell.IShellItemArray);
            try
            {
                shellLibrary.LoadLibraryFromItem(shellItem, ModernShell.AccessModes.Read);
                try
                {
                    var result = shellLibrary.GetFolders(ModernShell.LibraryFolderFilter.AllItems, ref id, out shellItems);
                    if (result == ModernShell.HResult.S_OK)
                    {
                        foreach (var folder in GetFolders(shellItems))
                        {
                            yield return folder;
                        }
                    }
                }
                finally
                {
                    Marshal.ReleaseComObject(shellItems);
                }
            }
            finally
            {
                Marshal.ReleaseComObject(shellLibrary);
            }
        }

        private static IEnumerable<string> GetFolders(ModernShell.IShellItemArray shellItems)
        {
            foreach (var shellItem in GetItems(shellItems))
            {
                yield return GetDisplayName(shellItem, ModernShell.SIGDN.DESKTOPABSOLUTEPARSING);
                Marshal.ReleaseComObject(shellItem);
            }
        }
    }
}
