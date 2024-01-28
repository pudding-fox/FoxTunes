using FoxTunes.Interfaces;
using System;

namespace FoxTunes.Launcher
{
    public static class Program
    {
        public static readonly TimeSpan SHUTDOWN_INTERVAL = TimeSpan.FromSeconds(1);

        public static readonly TimeSpan SHUTDOWN_TIMEOUT = TimeSpan.FromSeconds(10);

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
                using (var core = new Core(CoreFlags.None))
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
                        if (!core.Factories.Database.Test())
                        {
                            core.Components.UserInterface.Warn("The database was not found, attempting to initialize it.");
                            core.Factories.Database.Initialize();
                            if (!core.Factories.Database.Test())
                            {
                                throw new InvalidOperationException("Failed to initialize the database.");
                            }
                            using (var database = core.Factories.Database.Create())
                            {
                                core.CreateDefaultData(database);
                            }
                        }
                        core.Initialize();
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
                        //TODO: Bad .Result
                        if (!BackgroundTask.Shutdown(SHUTDOWN_INTERVAL, SHUTDOWN_TIMEOUT).Result)
                        {
                            //TODO: Warn.
                        }
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
