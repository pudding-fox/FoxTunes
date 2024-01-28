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
            Log4NetLogger.EnableFileAppender();
            using (var core = new Core())
            {
                core.Load();
                if (!CoreValidator.Instance.Validate(core))
                {
                    throw new InvalidOperationException("One or more required components were not loaded.");
                }
                core.Components.UserInterface.Show();
            }
        }
    }
}
