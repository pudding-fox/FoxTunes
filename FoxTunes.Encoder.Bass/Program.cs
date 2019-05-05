using FoxTunes.Interfaces;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

namespace FoxTunes
{
    public static class Program
    {
        const int INTERVAL = 1000;

        const int TIMEOUT = 10000;

        public static readonly object ReadSyncRoot = new object();

        public static readonly object WriteSyncRoot = new object();

        static Program()
        {
            LoggingBehaviour.FILE_NAME = string.Format(
                "Log_Converter_{0}.txt",
                DateTime.UtcNow.ToFileTime()
            );
            AssemblyResolver.Instance.Enable();
        }

        [STAThread]
        public static int Main(string[] args)
        {
            using (Stream input = Console.OpenStandardInput(), output = Console.OpenStandardOutput(), error = Console.OpenStandardError())
            {
                try
                {
                    Encode(input, output, error);
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

        private static void Encode(Stream input, Stream output, Stream error)
        {
            var encoderItems = ReadInput<EncoderItem[]>(input);
            using (var core = new Core(CoreFlags.Headless))
            {
                core.Load();
                core.Initialize();
                using (var encoder = new BassEncoder(encoderItems))
                {
                    encoder.InitializeComponent(core);
                    var thread1 = new Thread(() =>
                    {
                        try
                        {
                            encoder.Encode();
                            WriteOutput(output, new EncoderStatus(EncoderStatusType.Complete));
                            error.Flush();
                        }
                        catch (Exception e)
                        {
                            WriteOutput(output, new EncoderStatus(EncoderStatusType.Error));
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
                            while (thread1.IsAlive)
                            {
                                ProcessOutput(encoder, output);
                                Thread.Sleep(INTERVAL);
                            }
                            ProcessOutput(encoder, output);
                        }
                        catch
                        {
                            //Nothing can be done.
                        }
                    })
                    {
                        IsBackground = true
                    };
                    var thread3 = new Thread(() =>
                    {
                        try
                        {
                            while (thread1.IsAlive)
                            {
                                ProcessInput(encoder, input, output);
                                Thread.Sleep(INTERVAL);
                            }
                            ProcessInput(encoder, input, output);
                        }
                        catch
                        {
                            //Nothing can be done.
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
                        thread2.Abort();
                    }
                    if (!thread3.Join(TIMEOUT))
                    {
                        thread3.Abort();
                    }
                }
            }
        }

        private static void ProcessInput(IBassEncoder encoder, Stream input, Stream output)
        {
            var command = ReadInput<EncoderCommand>(input);
            switch (command.Type)
            {
                case EncoderCommandType.Cancel:
                    encoder.Cancel();
                    input.Close();
                    break;
                case EncoderCommandType.Quit:
                    input.Close();
                    output.Close();
                    break;
            }
        }

        private static void ProcessOutput(IBassEncoder encoder, Stream output)
        {
            WriteOutput(output, encoder.EncoderItems);
        }

        private static T ReadInput<T>(Stream stream)
        {
            lock (ReadSyncRoot)
            {
                var formatter = new BinaryFormatter();
                var input = formatter.Deserialize(stream);
                return (T)input;
            }
        }

        private static void WriteOutput(Stream stream, object value)
        {
            lock (WriteSyncRoot)
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(stream, value);
                stream.Flush();
            }
        }
    }
}
