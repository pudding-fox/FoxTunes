using System;

namespace FoxTunes.Launcher
{
    public static class Program
    {
        static Program()
        {
            AssemblyResolver.Instance.Enable();
        }

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
