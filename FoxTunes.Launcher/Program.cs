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
            typeof(WindowsUserInterface)
        };

        [STAThread]
        public static void Main(string[] args)
        {
            var core = new Core();
            core.LoadComponents();
            core.LoadManagers();
            core.LoadBehaviours();
            core.InitializeComponents();
            core.Components.UserInterface.Show();
        }
    }
}
