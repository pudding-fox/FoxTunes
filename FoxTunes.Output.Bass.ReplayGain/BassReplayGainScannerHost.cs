using FoxTunes.Interfaces;
using System;
using System.IO;
using System.Threading;

namespace FoxTunes
{
    public static class BassReplayGainScannerHost
    {
        private static ILogger Logger
        {
            get
            {
                return LogManager.Logger;
            }
        }

        public static string Location
        {
            get
            {
                return typeof(BassReplayGainScannerHost).Assembly.Location;
            }
        }

        const int INTERVAL = 1000;

        const int TIMEOUT = 10000;

        public static readonly object ReadSyncRoot = new object();

        public static readonly object WriteSyncRoot = new object();

        public static void Init()
        {
            LogManager.FileName = Path.Combine(
                Publication.StoragePath,
                string.Format(
                    "Log_Converter_{0}.txt",
                    DateTime.UtcNow.ToFileTime()
                )
            );
            AssemblyResolver.Instance.EnableExecution();
            AssemblyResolver.Instance.EnableReflectionOnly();
        }

        public static int Scan()
        {
            using (Stream input = Console.OpenStandardInput(), output = Console.OpenStandardOutput(), error = Console.OpenStandardError())
            {
                try
                {
                    Scan(input, output, error);
                    return 0;
                }
                catch (Exception e)
                {
                    new StreamWriter(error).Write(e.Message);
                    error.Flush();
                    return -1;
                }
            }
        }

        private static void Scan(Stream input, Stream output, Stream error)
        {
            var setup = new CoreSetup();
            setup.Disable(ComponentSlots.All);
            setup.Enable(ComponentSlots.Configuration);
            setup.Enable(ComponentSlots.Logger);
            using (var core = new Core(setup))
            {
                try
                {
                    core.Load();
                    core.Initialize();
                }
                catch (Exception e)
                {
                    Logger.Write(typeof(BassReplayGainScannerHost), LogLevel.Error, "Failed to initialize core: {0}", e.Message);
                    throw;
                }
                try
                {
                    Logger.Write(typeof(BassReplayGainScannerHost), LogLevel.Debug, "Begin reading items.");
                    var scannerItems = ReadInput<ScannerItem[]>(input);
                    Logger.Write(typeof(BassReplayGainScannerHost), LogLevel.Debug, "Read {0} items.", scannerItems.Length);
                    using (var scanner = new BassReplayGainScanner(scannerItems))
                    {
                        scanner.InitializeComponent(core);
                        Encode(scanner, input, output, error);
                    }
                }
                catch (Exception e)
                {
                    Logger.Write(typeof(BassReplayGainScannerHost), LogLevel.Error, "Failed to encode items: {0}", e.Message);
                    throw;
                }
            }
        }

        private static void Encode(IBassReplayGainScanner scanner, Stream input, Stream output, Stream error)
        {
            var thread1 = new Thread(() =>
            {
                try
                {
                    Logger.Write(typeof(BassReplayGainScannerHost), LogLevel.Debug, "Starting scanner main thread.");
                    scanner.Scan();
                    Logger.Write(typeof(BassReplayGainScannerHost), LogLevel.Debug, "Finished scanner main thread.");
                    WriteOutput(output, new ScannerStatus(ScannerStatusType.Complete));
                    error.Flush();
                }
                catch (Exception e)
                {
                    Logger.Write(typeof(BassReplayGainScannerHost), LogLevel.Error, "Error on scanner main thread: {0}", e.Message);
                    WriteOutput(output, new ScannerStatus(ScannerStatusType.Error));
                    new StreamWriter(error).Write(e.Message);
                    error.Flush();
                }
            })
            {
                IsBackground = true
            };
            var thread2 = new Thread(() =>
            {
                try
                {
                    Logger.Write(typeof(BassReplayGainScannerHost), LogLevel.Debug, "Starting scanner output thread.");
                    while (thread1.IsAlive)
                    {
                        ProcessOutput(scanner, output);
                        Thread.Sleep(INTERVAL);
                    }
                    ProcessOutput(scanner, output);
                    Logger.Write(typeof(BassReplayGainScannerHost), LogLevel.Debug, "Finished scanner output thread.");
                }
                catch (Exception e)
                {
                    Logger.Write(typeof(BassReplayGainScannerHost), LogLevel.Error, "Error on scanner output thread: {0}", e.Message);
                }
            })
            {
                IsBackground = true
            };
            var thread3 = new Thread(() =>
            {
                try
                {
                    Logger.Write(typeof(BassReplayGainScannerHost), LogLevel.Debug, "Starting scanner input thread.");
                    while (thread1.IsAlive)
                    {
                        ProcessInput(scanner, input, output);
                        Thread.Sleep(INTERVAL);
                    }
                    ProcessInput(scanner, input, output);
                    Logger.Write(typeof(BassReplayGainScannerHost), LogLevel.Debug, "Finished scanner input thread.");
                }
                catch (Exception e)
                {
                    Logger.Write(typeof(BassReplayGainScannerHost), LogLevel.Error, "Error on scanner input thread: {0}", e.Message);
                }
            })
            {
                IsBackground = true
            };
            thread1.Start();
            thread2.Start();
            thread3.Start();
            thread1.Join();
            if (!thread2.Join(TIMEOUT))
            {
                Logger.Write(typeof(BassReplayGainScannerHost), LogLevel.Warn, "Scanner output thread did not complete gracefully, aborting.");
                thread2.Abort();
            }
            if (!thread3.Join(TIMEOUT))
            {
                Logger.Write(typeof(BassReplayGainScannerHost), LogLevel.Warn, "Scanner input thread did not complete gracefully, aborting.");
                thread3.Abort();
            }
        }

        private static void ProcessInput(IBassReplayGainScanner scanner, Stream input, Stream output)
        {
            Logger.Write(typeof(BassReplayGainScannerHost), LogLevel.Debug, "Begin reading command.");
            var command = ReadInput<ScannerCommand>(input);
            Logger.Write(typeof(BassReplayGainScannerHost), LogLevel.Debug, "Read command: {0}", Enum.GetName(typeof(ScannerCommandType), command.Type));
            switch (command.Type)
            {
                case ScannerCommandType.Cancel:
                    Logger.Write(typeof(BassReplayGainScannerHost), LogLevel.Debug, "Sending cancellation signal to scanner.");
                    scanner.Cancel();
                    Logger.Write(typeof(BassReplayGainScannerHost), LogLevel.Debug, "Closing stdin.");
                    input.Close();
                    break;
                case ScannerCommandType.Quit:
                    Logger.Write(typeof(BassReplayGainScannerHost), LogLevel.Debug, "Closing stdin/stdout.");
                    input.Close();
                    output.Close();
                    break;
            }
        }

        private static void ProcessOutput(IBassReplayGainScanner scanner, Stream output)
        {
            WriteOutput(output, scanner.ScannerItems);
        }

        private static T ReadInput<T>(Stream stream)
        {
            lock (ReadSyncRoot)
            {
                var input = Serializer.Instance.Read(stream);
                return (T)input;
            }
        }

        private static void WriteOutput(Stream stream, object value)
        {
            lock (WriteSyncRoot)
            {
                Serializer.Instance.Write(stream, value);
                stream.Flush();
            }
        }
    }
}
