using FoxTunes.Interfaces;
using ManagedBass;
using System;
using System.Diagnostics;
using System.IO;
using System.Security.Permissions;
using System.Threading;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class BassEncoder : MarshalByRefObject, IBassEncoder
    {
        protected static ILogger Logger
        {
            get
            {
                return LogManager.Logger;
            }
        }

        static BassEncoder()
        {
            LoggingBehaviour.FILE_NAME = string.Format(
                "Log_{0}_{1}.txt",
                typeof(BassEncoder).Name,
                DateTime.UtcNow.ToFileTime()
            );
            AssemblyResolver.Instance.Enable();
        }

        private BassEncoder()
        {
            this.CancellationToken = new CancellationToken();
        }

        public BassEncoder(AppDomain domain) : this()
        {
            this.Domain = domain;
        }

        public CancellationToken CancellationToken { get; private set; }

        public AppDomain Domain { get; private set; }

        public void Encode(EncoderItem[] encoderItems)
        {
            using (var core = new Core(CoreFlags.Headless))
            {
                core.Load();
                core.Initialize();
                Logger.Write(this.GetType(), LogLevel.Debug, "Initializing BASS (NoSound).");
                Bass.Init(Bass.NoSoundDevice);
                try
                {
                    Logger.Write(this.GetType(), LogLevel.Debug, "Fetching settings.");
                    var factory = ComponentRegistry.Instance.GetComponent<BassEncoderSettingsFactory>();
                    var settings = factory.CreateSettings();
                    this.Encode(core, encoderItems, settings);
                }
                finally
                {
                    Logger.Write(this.GetType(), LogLevel.Debug, "Releasing BASS (NoSound).");
                    Bass.Free();
                }
            }
        }

        protected virtual void Encode(ICore core, EncoderItem[] encoderItems, IBassEncoderSettings settings)
        {
            if (settings.Threads > 1)
            {
                Logger.Write(this.GetType(), LogLevel.Debug, "Beginning parallel encoding with {0} threads.", settings.Threads);
            }
            else
            {
                Logger.Write(this.GetType(), LogLevel.Debug, "Beginning single threaded encoding.");
            }
            var parallelOptions = new ParallelOptions()
            {
                MaxDegreeOfParallelism = settings.Threads
            };
            Parallel.ForEach(encoderItems, parallelOptions, encoderItem =>
            {
                if (this.CancellationToken.IsCancellationRequested)
                {
                    Logger.Write(this.GetType(), LogLevel.Warn, "Skipping file \"{0}\" due to cancellation.", encoderItem.InputFileName);
                    encoderItem.Status = EncoderItemStatus.Cancelled;
                    return;
                }
                encoderItem.OutputFileName = settings.GetOutput(encoderItem.InputFileName);
                if (!this.CheckPaths(encoderItem.InputFileName, encoderItem.OutputFileName))
                {
                    Logger.Write(this.GetType(), LogLevel.Warn, "Skipping file \"{0}\" due to output file \"{1}\" already exists.", encoderItem.InputFileName, encoderItem.OutputFileName);
                    encoderItem.Status = EncoderItemStatus.Failed;
                    encoderItem.AddError(string.Format("Output file \"{0}\" already exists.", encoderItem.OutputFileName));
                    return;
                }
                try
                {
                    Logger.Write(this.GetType(), LogLevel.Debug, "Beginning encoding file \"{0}\" to output file \"{1}\".", encoderItem.InputFileName, encoderItem.OutputFileName);
                    encoderItem.Progress = EncoderItem.PROGRESS_NONE;
                    encoderItem.Status = EncoderItemStatus.Processing;
                    this.Encode(core, encoderItem, settings);
                    if (encoderItem.Status == EncoderItemStatus.Complete)
                    {
                        Logger.Write(this.GetType(), LogLevel.Debug, "Encoding file \"{0}\" to output file \"{1}\" completed successfully.", encoderItem.InputFileName, encoderItem.OutputFileName);
                    }
                    else
                    {
                        Logger.Write(this.GetType(), LogLevel.Warn, "Encoding file \"{0}\" failed: Unknown error.", encoderItem.InputFileName);
                    }
                }
                catch (Exception e)
                {
                    Logger.Write(this.GetType(), LogLevel.Warn, "Encoding file \"{0}\" failed: {1}", encoderItem.InputFileName, e.Message);
                    encoderItem.Status = EncoderItemStatus.Failed;
                    encoderItem.AddError(e.Message);
                }
                finally
                {
                    encoderItem.Progress = EncoderItem.PROGRESS_COMPLETE;
                }
            });
            Logger.Write(this.GetType(), LogLevel.Debug, "Encoding completed successfully.");
        }

        protected virtual void Encode(ICore core, EncoderItem encoderItem, IBassEncoderSettings settings)
        {
            var flags = BassFlags.Decode;
            if (this.ShouldDecodeFloat(encoderItem, settings))
            {
                Logger.Write(this.GetType(), LogLevel.Debug, "Decoding file \"{0}\" in high quality mode (32 bit floating point).", encoderItem.InputFileName);
                flags |= BassFlags.Float;
            }
            else
            {
                Logger.Write(this.GetType(), LogLevel.Debug, "Decoding file \"{0}\" in standaed quality mode (16 bit integer).", encoderItem.InputFileName);
            }
            var stream = this.CreateStream(encoderItem.InputFileName, flags);
            if (stream.IsEmpty)
            {
                Logger.Write(this.GetType(), LogLevel.Debug, "Failed to create stream for file \"{0}\": Unknown error.", encoderItem.InputFileName);
                return;
            }
            Logger.Write(this.GetType(), LogLevel.Debug, "Created stream for file \"{0}\": {1}", encoderItem.InputFileName, stream.ChannelHandle);
            try
            {
                if (flags.HasFlag(BassFlags.Float))
                {
                    this.EncodeWithResampler(encoderItem, stream, settings);
                }
                else
                {
                    this.Encode(encoderItem, stream, settings);
                }
            }
            finally
            {
                Logger.Write(this.GetType(), LogLevel.Debug, "Releasing stream for file \"{0}\": {1}", encoderItem.InputFileName, stream.ChannelHandle);
                Bass.StreamFree(stream.ChannelHandle);
            }
            if (this.CancellationToken.IsCancellationRequested)
            {
                encoderItem.Status = EncoderItemStatus.Cancelled;
            }
            else
            {
                encoderItem.Status = EncoderItemStatus.Complete;
            }
        }

        protected virtual void Encode(EncoderItem encoderItem, IBassStream stream, IBassEncoderSettings settings)
        {
            using (var encoderProcess = this.CreateEncoderProcess(encoderItem, stream, settings))
            {
                this.Encode(encoderItem, stream, encoderProcess);
                if (this.WaitForExit(encoderProcess))
                {
                    if (encoderProcess.ExitCode != 0)
                    {
                        throw new InvalidOperationException("Process does not indicate success.");
                    }
                }
            }
        }

        protected virtual void Encode(EncoderItem encoderItem, IBassStream input, Process encoderProcess)
        {
            var channelReader = new ChannelReader(encoderItem, input);
            var encoderWriter = new ProcessWriter(encoderProcess);
            channelReader.CopyTo(encoderWriter, this.CancellationToken);
            encoderWriter.Close();
        }

        protected virtual void EncodeWithResampler(EncoderItem encoderItem, IBassStream stream, IBassEncoderSettings settings)
        {
            using (var resamplerProcess = this.CreateResamplerProcess(encoderItem, stream, new SoxEncoderSettings(settings)))
            {
                using (var encoderProcess = this.CreateEncoderProcess(encoderItem, stream, settings))
                {
                    var success = true;
                    this.EncodeWithResampler(encoderItem, stream, resamplerProcess, encoderProcess);
                    if (this.WaitForExit(resamplerProcess))
                    {
                        if (resamplerProcess.ExitCode != 0)
                        {
                            success = false;
                        }
                    }
                    if (this.WaitForExit(encoderProcess))
                    {
                        if (encoderProcess.ExitCode != 0)
                        {
                            success = false;
                        }
                    }
                    if (!success)
                    {
                        throw new InvalidOperationException("Process does not indicate success.");
                    }
                }
            }
        }

        protected virtual void EncodeWithResampler(EncoderItem encoderItem, IBassStream stream, Process resamplerProcess, Process encoderProcess)
        {
            var channelReader = new ChannelReader(encoderItem, stream);
            var resamplerReader = new ProcessReader(resamplerProcess);
            var resamplerWriter = new ProcessWriter(resamplerProcess);
            var encoderWriter = new ProcessWriter(encoderProcess);
            var threads = new[]
            {
                new Thread(() =>
                {
                    this.Try(() => channelReader.CopyTo(resamplerWriter,this.CancellationToken), this.GetErrorHandler(encoderItem));
                })
                {
                    Name = string.Format("ChannelReader(\"{0}\", {1})", encoderItem.InputFileName, stream.ChannelHandle),
                    IsBackground = true
                },
                new Thread(() =>
                {
                    this.Try(() => resamplerReader.CopyTo(encoderWriter,this.CancellationToken), this.GetErrorHandler(encoderItem));
                })
                {
                    Name = string.Format("ProcessReader(\"{0}\", {1})", encoderItem.InputFileName, resamplerProcess.Id),
                    IsBackground = true
                }
            };
            Logger.Write(this.GetType(), LogLevel.Debug, "Starting background threads for file \"{0}\".", encoderItem.InputFileName);
            foreach (var thread in threads)
            {
                thread.Start();
            }
            Logger.Write(this.GetType(), LogLevel.Debug, "Completing background threads for file \"{0}\".", encoderItem.InputFileName);
            foreach (var thread in threads)
            {
                this.Join(thread);
            }
            resamplerReader.Close();
            resamplerWriter.Close();
            encoderWriter.Close();
        }

        public void Cancel()
        {
            if (this.CancellationToken.IsCancellationRequested)
            {
                return;
            }
            Logger.Write(this.GetType(), LogLevel.Warn, "Cancellation requested, shutting down.");
            this.CancellationToken.Cancel();
        }

        protected virtual bool CheckPaths(string inputFileName, string outputFileName)
        {
            if (!string.IsNullOrEmpty(Path.GetPathRoot(inputFileName)) && !File.Exists(inputFileName))
            {
                //TODO: Bad .Result
                if (!NetworkDrive.IsRemotePath(inputFileName) || !NetworkDrive.ConnectRemotePath(inputFileName).Result)
                {
                    throw new FileNotFoundException(string.Format("File not found: {0}", inputFileName), inputFileName);
                }
            }
            return !File.Exists(outputFileName);
        }

        protected virtual IBassStream CreateStream(string fileName, BassFlags flags)
        {
            const int INTERVAL = 5000;
            var streamFactory = ComponentRegistry.Instance.GetComponent<IBassStreamFactory>();
            var playlistItem = new PlaylistItem()
            {
                FileName = fileName
            };
        retry:
            if (this.CancellationToken.IsCancellationRequested)
            {
                return BassStream.Empty;
            }
            //TODO: Bad .Result
            var stream = streamFactory.CreateStream(
                playlistItem,
                false,
                flags
            ).Result;
            if (stream.IsEmpty)
            {
                if (stream.Errors == Errors.Already)
                {
                    Logger.Write(this.GetType(), LogLevel.Trace, "Failed to create decoder stream for file \"{0}\": Device is already in use.", fileName);
                    Thread.Sleep(INTERVAL);
                    goto retry;
                }
                throw new InvalidOperationException(string.Format("Failed to create decoder stream for file \"{0}\".", fileName));
            }
            return stream;
        }

        protected virtual Process CreateResamplerProcess(EncoderItem encoderItem, IBassStream stream, IBassEncoderSettings settings)
        {
            Logger.Write(this.GetType(), LogLevel.Debug, "Creating resampler process for file \"{0}\".", encoderItem.InputFileName);
            var processStartInfo = new ProcessStartInfo()
            {
                FileName = settings.Executable,
                WorkingDirectory = settings.Directory,
                Arguments = settings.GetArguments(encoderItem, stream),
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            var process = Process.Start(processStartInfo);
            process.ErrorDataReceived += (sender, e) =>
            {
                if (string.IsNullOrEmpty(e.Data))
                {
                    return;
                }
                Logger.Write(this.GetType(), LogLevel.Trace, "{0}: {1}", settings.Executable, e.Data);
                encoderItem.AddError(e.Data);
            };
            process.BeginErrorReadLine();
            Logger.Write(this.GetType(), LogLevel.Debug, "Created resampler process for file \"{0}\": \"{1}\" {2}", encoderItem.InputFileName, processStartInfo.FileName, processStartInfo.Arguments);
            return process;
        }

        protected virtual Process CreateEncoderProcess(EncoderItem encoderItem, IBassStream stream, IBassEncoderSettings settings)
        {
            Logger.Write(this.GetType(), LogLevel.Debug, "Creating encoder process for file \"{0}\".", encoderItem.InputFileName);
            var processStartInfo = new ProcessStartInfo()
            {
                FileName = settings.Executable,
                WorkingDirectory = settings.Directory,
                Arguments = settings.GetArguments(encoderItem, stream),
                RedirectStandardInput = true,
                RedirectStandardOutput = false,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            var process = Process.Start(processStartInfo);
            process.ErrorDataReceived += (sender, e) =>
            {
                if (string.IsNullOrEmpty(e.Data))
                {
                    return;
                }
                Logger.Write(this.GetType(), LogLevel.Trace, "{0}: {1}", settings.Executable, e.Data);
                encoderItem.AddError(e.Data);
            };
            process.BeginErrorReadLine();
            Logger.Write(this.GetType(), LogLevel.Debug, "Created encoder process for file \"{0}\": \"{1}\" {2}", encoderItem.InputFileName, processStartInfo.FileName, processStartInfo.Arguments);
            return process;
        }

        protected virtual bool ShouldDecodeFloat(EncoderItem encoderItem, IBassEncoderSettings settings)
        {
            if (encoderItem.BitsPerSample == 1)
            {
                Logger.Write(this.GetType(), LogLevel.Debug, "Suggesting high quality mode for file \"{0}\": dsd.", encoderItem.InputFileName);
                return true;
            }
            if (encoderItem.BitsPerSample > 16 || settings.Format.Depth > 16)
            {
                Logger.Write(this.GetType(), LogLevel.Debug, "Suggesting high quality mode for file \"{0}\": >16 bit.", encoderItem.InputFileName);
                return true;
            }
            Logger.Write(this.GetType(), LogLevel.Debug, "Suggesting standard quality mode for file \"{0}\": <=16 bit.", encoderItem.InputFileName);
            return false;
        }

        protected virtual Action<Exception> GetErrorHandler(EncoderItem encoderItem)
        {
            return e =>
            {
                Logger.Write(this.GetType(), LogLevel.Warn, "Encoder background thread for file \"{0}\" error: {1}", encoderItem.InputFileName, e.Message);
                encoderItem.AddError(e.Message);
            };
        }

        protected virtual bool Join(Thread thread)
        {
            const int INTERVAL = 5000;
            while (thread.IsAlive)
            {
                if (thread.Join(INTERVAL))
                {
                    break;
                }
                if (this.CancellationToken.IsCancellationRequested)
                {
                    if (thread.Join(INTERVAL))
                    {
                        break;
                    }
                    thread.Abort();
                    return false;
                }
            }
            return true;
        }

        protected virtual bool WaitForExit(Process process)
        {
            const int INTERVAL = 5000;
            while (!process.HasExited)
            {
                if (process.WaitForExit(INTERVAL))
                {
                    break;
                }
                if (this.CancellationToken.IsCancellationRequested)
                {
                    if (process.WaitForExit(INTERVAL))
                    {
                        break;
                    }
                    process.Kill();
                    return false;
                }
            }
            return true;
        }

        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.Infrastructure)]
        public override object InitializeLifetimeService()
        {
            //Disable the 5 minute lease default.
            return null;
        }
    }
}
