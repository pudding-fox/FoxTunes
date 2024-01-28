#if VISTA
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
            var shellItems = default(Shell.IShellItemArray);
            Shell.SHCreateShellItemArrayFromDataObject(
                (global::System.Runtime.InteropServices.ComTypes.IDataObject)dataObject,
                new Guid(Shell.InterfaceGuids.IShellItemArray),
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

        private static IEnumerable<Shell.IShellItem> GetItems(Shell.IShellItemArray shellItems)
        {
            var count = default(uint);
            shellItems.GetCount(out count);
            for (var position = default(uint); position < count; position++)
            {
                var shellItem = default(Shell.IShellItem);
                shellItems.GetItemAt(position, out shellItem);
                yield return shellItem;
            }
        }

        private static string GetDisplayName(Shell.IShellItem shellItem, Shell.SIGDN sigdn)
        {
            var pointer = shellItem.GetDisplayName(sigdn);
            var result = Marshal.PtrToStringUni(pointer);
            Marshal.FreeCoTaskMem(pointer);
            return result;
        }

        private static IEnumerable<string> GetFolders(Shell.IShellItem shellItem)
        {
            var id = new Guid(Shell.ShellGuids.IShellItemArray);
            var shellLibrary = (Shell.IShellLibrary)new Shell.ShellLibrary();
            var shellItems = default(Shell.IShellItemArray);
            try
            {
                shellLibrary.LoadLibraryFromItem(shellItem, Shell.AccessModes.Read);
                try
                {
                    var result = shellLibrary.GetFolders(Shell.LibraryFolderFilter.AllItems, ref id, out shellItems);
                    if (result == Shell.HResult.S_OK)
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

        private static IEnumerable<string> GetFolders(Shell.IShellItemArray shellItems)
        {
            foreach (var shellItem in GetItems(shellItems))
            {
                yield return GetDisplayName(shellItem, Shell.SIGDN.DESKTOPABSOLUTEPARSING);
                Marshal.ReleaseComObject(shellItem);
            }
        }
    }
}
#endif
