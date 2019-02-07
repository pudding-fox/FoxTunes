using System;
using System.Reflection;

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
                using (var core = new Core())
                {
                    AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
                    {
                        if (core.Components.UserInterface != null)
                        {
                            core.Components.UserInterface.Fatal(e.ExceptionObject as Exception);
                        }
                    };
                    try
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
                    }
                    catch (Exception e)
                    {
                        if (core.Components.UserInterface != null)
                        {
                            core.Components.UserInterface.Fatal(e);
                            return;
                        }
                    }
                    try
                    {
                        core.Components.UserInterface.Show();
                    }
                    catch (Exception e)
                    {
                        core.Components.UserInterface.Fatal(e);
                    }
                }
            }
        }
    }
}
