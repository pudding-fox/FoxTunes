using System;
using System.Runtime.InteropServices;

namespace FoxTunes
{
    public static class Shell
    {
        public static void CreateShortcut(string location, string target)
        {
            WithShell(shell =>
            {
                var shortcut = shell.CreateShortcut(location);
                try
                {
                    shortcut.TargetPath = target;
                    shortcut.Save();
                }
                finally
                {
                    Marshal.FinalReleaseComObject(shortcut);
                }
            });
        }

        public static void WithShell(Action<dynamic> action)
        {
            var type = Type.GetTypeFromCLSID(new Guid("72C24DD5-D70A-438B-8A42-98424B88AFB8"));
            dynamic shell = Activator.CreateInstance(type);
            try
            {
                action(shell);
            }
            finally
            {
                Marshal.FinalReleaseComObject(shell);
            }
        }
    }
}
