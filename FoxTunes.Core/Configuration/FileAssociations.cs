using FoxTunes.Interfaces;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace FoxTunes
{
    public class FileAssociations : IFileAssociations
    {
        public string Id
        {
            get
            {
                return Process.GetCurrentProcess().MainModule.ModuleName;
            }
        }

        public string FileName
        {
            get
            {
                return Process.GetCurrentProcess().MainModule.FileName;
            }
        }

        public IEnumerable<IFileAssociation> Associations
        {
            get
            {
                foreach (var extension in GetAssociations(this.Create(null)))
                {
                    yield return this.Create(extension);
                }
            }
        }

        public IFileAssociation Create(string extension)
        {
            return new FileAssociation(extension, this.Id, this.FileName);
        }

        public void Enable()
        {
            AddProgram(this.Create(null));
        }

        public void Enable(IEnumerable<IFileAssociation> associations)
        {
            AddAssociations(associations);
        }

        public void Disable()
        {
            RemoveProgram(this.Create(null));
        }

        public void Disable(IEnumerable<IFileAssociation> associations)
        {
            RemoveAssociations(associations);
        }

        [DllImport("Shell32.dll")]
        private static extern int SHChangeNotify(int eventId, int flags, IntPtr item1, IntPtr item2);

        private const int SHCNE_ASSOCCHANGED = 0x8000000;
        private const int SHCNF_FLUSH = 0x1000;

        public static IEnumerable<string> GetAssociations(IFileAssociation association)
        {
            foreach (var key in GetKeys(@"Software\Classes", association.ProgId))
            {
                yield return key;
            }
        }

        private static string GetOpenString(string path)
        {
            return "\"" + path + "\" \"%1\"";
        }

        private static void AddAssociations(IEnumerable<IFileAssociation> associations)
        {
            var success = false;
            foreach (var association in associations)
            {
                success |= AddAssociation(association);
            }
            if (success)
            {
                SHChangeNotify(SHCNE_ASSOCCHANGED, SHCNF_FLUSH, IntPtr.Zero, IntPtr.Zero);
            }
        }

        private static void RemoveAssociations(IEnumerable<IFileAssociation> associations)
        {
            var success = false;
            foreach (var association in associations)
            {
                success |= RemoveAssociation(association);
            }
            if (success)
            {
                SHChangeNotify(SHCNE_ASSOCCHANGED, SHCNF_FLUSH, IntPtr.Zero, IntPtr.Zero);
            }
        }

        private static bool AddProgram(IFileAssociation association)
        {
            return SetKeyValue(@"Software\Classes\" + association.ProgId + @"\shell\open\command", association.ProgId);
        }

        private static bool RemoveProgram(IFileAssociation association)
        {
            return DeleteKey(@"Software\Classes\" + association.ProgId);
        }

        private static bool AddAssociation(IFileAssociation association)
        {
            return SetKeyValue(@"Software\Classes\" + association.Extension, association.ProgId);
        }

        private static bool RemoveAssociation(IFileAssociation association)
        {
            return DeleteKey(@"Software\Classes\" + association.Extension);
        }

        private static IEnumerable<string> GetKeys(string path, string value)
        {
            using (var key = Registry.CurrentUser.OpenSubKey(path))
            {
                foreach (var name in key.GetSubKeyNames())
                {
                    if (string.Equals(GetKeyValue(path + @"\" + name), value, StringComparison.OrdinalIgnoreCase))
                    {
                        yield return name;
                    }
                }
            }
        }

        private static string GetKeyValue(string path)
        {
            using (var key = Registry.CurrentUser.OpenSubKey(path))
            {
                if (key == null)
                {
                    return null;
                }
                return key.GetValue(null) as string;
            }
        }

        private static bool SetKeyValue(string path, string value)
        {
            using (var key = Registry.CurrentUser.CreateSubKey(path))
            {
                if (string.Equals(key.GetValue(null) as string, value, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
                key.SetValue(null, value);
                return true;
            }
        }

        private static bool DeleteKey(string path)
        {
            using (var key = Registry.CurrentUser.OpenSubKey(path))
            {
                if (key == null)
                {
                    return false;
                }
            }
            Registry.CurrentUser.DeleteSubKeyTree(path);
            return true;
        }
    }
}
