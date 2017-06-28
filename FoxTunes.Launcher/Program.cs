using FoxTunes.Interfaces;
using System;

namespace FoxTunes.Launcher
{
    public static class Program
    {
        static Program()
        {
            AssemblyResolver.Instance.Enable();
        }

        private static readonly Type[] References = new[]
        {
            typeof(Configuration),
            typeof(SqliteDatabase),
            typeof(CSCoreOutput),
            typeof(TagLibMetaDataSource),
            typeof(WindowsUserInterface)
        };

        [STAThread]
        public static void Main(string[] args)
        {
            var core = new Core();
            core.Load();
            core.Components.UserInterface.Show();
        }
    }
}
