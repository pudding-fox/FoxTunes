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
            typeof(SQLiteDatabase),
            typeof(CSCoreOutput),
            typeof(TagLibMetaDataSource),
            typeof(WindowsUserInterface),
            typeof(JSScriptingRuntime)
        };

        [STAThread]
        public static void Main(string[] args)
        {
            using (var core = new Core())
            {
                core.Load();
                core.Components.UserInterface.Show();
            }
        }
    }
}
