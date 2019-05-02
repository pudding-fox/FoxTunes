using FoxTunes.Interfaces;
using ManagedBass;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
            AssemblyResolver.Instance.Enable();
        }

        private BassEncoder()
        {
            this.Processes = new ConcurrentDictionary<Process, EncoderItem>();
            this.Threads = new ConcurrentDictionary<Thread, EncoderItem>();
        }

        public BassEncoder(AppDomain domain) : this()
        {
            this.Domain = domain;
        }

        public ConcurrentDictionary<Process, EncoderItem> Processes { get; private set; }

        public ConcurrentDictionary<Thread, EncoderItem> Threads { get; private set; }

        public bool IsCancellationRequested { get; private set; }

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
                if (this.IsCancellationRequested)
                {
                    Logger.Write(this.GetType(), LogLevel.Warn, "Skipping file \"{0}\" due to cancellation.", encoderItem.InputFileName);
                    encoderItem.Status = EncoderItemStatus.Cancelled;
                    return;
                }
                encoderItem.OutputFileName = settings.GetOutput(encoderItem.InputFileName);
                if (File.Exists(encoderItem.OutputFileName))
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
            var streamFactory = ComponentRegistry.Instance.GetComponent<IBassStreamFactory>();
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
            //TODO: Bad .Result
            var stream = streamFactory.CreateStream(
                new PlaylistItem()
                {
                    FileName = encoderItem.InputFileName
                },
                false,
                flags
            ).Result;
            if (stream == null || stream.ChannelHandle == 0)
            {
                throw new InvalidOperationException(string.Format("Failed to create decoder stream for file \"{0}\".", encoderItem.InputFileName));
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
            encoderItem.Status = EncoderItemStatus.Complete;
        }

        protected virtual void Encode(EncoderItem encoderItem, IBassStream stream, IBassEncoderSettings settings)
        {
            using (var encoderProcess = this.CreateEncoderProcess(encoderItem, stream, settings))
            {
                this.Processes.TryAdd(encoderProcess, encoderItem);
                try
                {
                    this.Encode(encoderItem, stream, encoderProcess);
                    encoderProcess.WaitForExit();
                    var errors = encoderProcess.StandardError.ReadToEnd();
                    if (!string.IsNullOrEmpty(errors))
                    {
                        Logger.Write(this.GetType(), LogLevel.Trace, "Encoder process output: {0}", errors);
                        encoderItem.AddError(string.Format("Encoder: {0}", errors));
                    }
                    if (encoderProcess.ExitCode != 0)
                    {
                        throw new InvalidOperationException(string.Format("Encoder process \"{0}\" failed.", encoderProcess.Id));
                    }
                }
                finally
                {
                    this.Processes.TryRemove(encoderProcess);
                }
            }
        }

        protected virtual void Encode(EncoderItem encoderItem, IBassStream input, Process encoderProcess)
        {
            var channelReader = new ChannelReader(encoderItem, input);
            var encoderWriter = new ProcessWriter(encoderProcess);
            channelReader.CopyTo(encoderWriter);
        }

        protected virtual void EncodeWithResampler(EncoderItem encoderItem, IBassStream stream, IBassEncoderSettings settings)
        {
            using (var resamplerProcess = this.CreateResamplerProcess(encoderItem, stream, new SoxEncoderSettings(settings)))
            {
                this.Processes.TryAdd(resamplerProcess, encoderItem);
                try
                {
                    using (var encoderProcess = this.CreateEncoderProcess(encoderItem, stream, settings))
                    {
                        this.Processes.TryAdd(encoderProcess, encoderItem);
                        try
                        {
                            this.EncodeWithResampler(encoderItem, stream, resamplerProcess, encoderProcess);
                            try
                            {
                                resamplerProcess.WaitForExit();
                                {
                                    var errors = resamplerProcess.StandardError.ReadToEnd();
                                    if (!string.IsNullOrEmpty(errors))
                                    {
                                        Logger.Write(this.GetType(), LogLevel.Trace, "Resampler process output: {0}", errors);
                                        encoderItem.AddError(string.Format("Resampler: {0}", errors));
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                Logger.Write(this.GetType(), LogLevel.Trace, "Resampler process error: {0}", e.Message);
                                encoderItem.Status = EncoderItemStatus.Failed;
                                encoderItem.AddError(e.Message);
                            }
                            try
                            {
                                encoderProcess.WaitForExit();
                                {
                                    var errors = encoderProcess.StandardError.ReadToEnd();
                                    if (!string.IsNullOrEmpty(errors))
                                    {
                                        Logger.Write(this.GetType(), LogLevel.Trace, "Encoder process output: {0}", errors);
                                        encoderItem.AddError(string.Format("Encoder: {0}", errors));
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                Logger.Write(this.GetType(), LogLevel.Trace, "Encoder process error: {0}", e.Message);
                                encoderItem.Status = EncoderItemStatus.Failed;
                                encoderItem.AddError(e.Message);
                            }
                            if (resamplerProcess.ExitCode != 0)
                            {
                                throw new InvalidOperationException(string.Format("Resampler process \"{0}\" failed.", resamplerProcess.Id));
                            }
                            if (encoderProcess.ExitCode != 0)
                            {
                                throw new InvalidOperationException(string.Format("Encoder process \"{0}\" failed.", encoderProcess.Id));
                            }
                        }
                        finally
                        {
                            this.Processes.TryRemove(encoderProcess);
                        }
                    }
                }
                finally
                {
                    this.Processes.TryRemove(resamplerProcess);
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
                    this.Try(() => channelReader.CopyTo(resamplerWriter), this.GetErrorHandler(encoderItem));
                })
                {
                    Name = string.Format("ChannelReader(\"{0}\", {1})", encoderItem.InputFileName, stream.ChannelHandle),
                    IsBackground = true
                },
                new Thread(() =>
                {
                    this.Try(() => resamplerReader.CopyTo(encoderWriter), this.GetErrorHandler(encoderItem));
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
                this.Threads.TryAdd(thread, encoderItem);
            }
            Logger.Write(this.GetType(), LogLevel.Debug, "Completing background threads for file \"{0}\".", encoderItem.InputFileName);
            foreach (var thread in threads)
            {
                //TODO: Timeout.
                thread.Join();
                this.Threads.TryRemove(thread);
            }
        }

        public void Cancel()
        {
            Logger.Write(this.GetType(), LogLevel.Warn, "Cancellation requested, stopping background processes.");
            this.IsCancellationRequested = true;
            this.CancelProcesses();
            this.CancelThreads();
        }

        protected virtual void CancelProcesses()
        {
            var processes = this.Processes.Keys.ToArray();
            foreach (var process in processes)
            {
                try
                {
                    if (!process.HasExited)
                    {
                        Logger.Write(this.GetType(), LogLevel.Warn, "Stopping background process {0}.", process.Id);
                        try
                        {
                            process.Close();
                        }
                        catch (Exception e)
                        {
                            Logger.Write(this.GetType(), LogLevel.Warn, "Failed to stop background process {0}: {1}", process.Id, e.Message);
                        }
                    }
                }
                catch
                {
                    //Nothing can be done.
                }
                this.Processes.TryRemove(process);
            }
        }

        protected virtual void CancelThreads()
        {
            const int TIMEOUT = 1000;
            var threads = this.Threads.Keys.ToArray();
            foreach (var thread in threads)
            {
                try
                {
                    if (thread.IsAlive)
                    {
                        thread.Abort();
                        if (!thread.Join(TIMEOUT))
                        {
                            Logger.Write(this.GetType(), LogLevel.Warn, "Failed to cancel background thread {0}: Timeout.", thread.Name);
                        }
                    }
                }
                catch
                {
                    //Nothing can be done.
                }
                this.Threads.TryRemove(thread);
            }
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
    }
}
