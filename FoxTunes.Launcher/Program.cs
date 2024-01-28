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
            using (var server = new Server())
            {
                if (server.IsDisposed)
                {
                    var client = new Client();
                    //TODO: Bad .Wait()
                    client.Send(Environment.CommandLine).Wait();
                    return;
                }
                Log4NetLogger.EnableFileAppender();
                using (var core = new Core())
                {
                    core.Load();
                    if (!CoreValidator.Instance.Validate(core))
                    {
                        throw new InvalidOperationException("One or more required components were not loaded.");
                    }
                    server.Message += (sender, e) =>
                    {
                        core.Components.UserInterface.Run(e.Message);
                    };
                    core.Components.UserInterface.Run(Environment.CommandLine);
                    core.Components.UserInterface.Show();
                }
            }
        }
    }
}
