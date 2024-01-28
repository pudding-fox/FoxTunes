using FoxTunes.Interfaces;
using ManagedBass;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class BassEncoder : MarshalByRefObject, IBassEncoder
    {
        static BassEncoder()
        {
            AssemblyResolver.Instance.Enable();
        }

        const BassFlags BASS_FLAGS = BassFlags.Decode | BassFlags.Float;

        private BassEncoder()
        {
            this.Errors = new ConcurrentDictionary<int, string>();
        }

        public BassEncoder(AppDomain domain, int concurrency) : this()
        {
            this.Domain = domain;
            this.Concurrency = concurrency;
        }

        public ConcurrentDictionary<int, string> Errors { get; private set; }

        public AppDomain Domain { get; private set; }

        public int Concurrency { get; private set; }

        public ParallelOptions ParallelOptions
        {
            get
            {
                return new ParallelOptions()
                {
                    MaxDegreeOfParallelism = this.Concurrency
                };
            }
        }

        public void Encode(string[] fileNames, IBassEncoderSettings settings)
        {
            using (var core = new Core(CoreFlags.Headless))
            {
                core.Load();
                core.Initialize();
                Bass.Init(Bass.NoSoundDevice);
                try
                {
                    this.Encode(core, fileNames, settings);
                }
                finally
                {
                    Bass.Free();
                }
            }
        }

        protected virtual void Encode(ICore core, string[] fileNames, IBassEncoderSettings settings)
        {
            Parallel.ForEach(fileNames, this.ParallelOptions, fileName =>
            {
                this.Encode(core, fileName, settings);
            });
        }

        protected virtual void Encode(ICore core, string fileName, IBassEncoderSettings settings)
        {
            var streamFactory = ComponentRegistry.Instance.GetComponent<IBassStreamFactory>();
            //TODO: Bad .Result
            var stream = streamFactory.CreateStream(new PlaylistItem()
            {
                FileName = fileName
            }, false, BASS_FLAGS).Result;
            if (stream == null || stream.ChannelHandle == 0)
            {
                //TODO: Warn.
                return;
            }
            try
            {
                this.Encode(fileName, stream, settings);
            }
            finally
            {
                Bass.StreamFree(stream.ChannelHandle);
            }
        }

        protected virtual void Encode(string fileName, IBassStream stream, IBassEncoderSettings settings)
        {
            using (var resamplerProcess = this.CreateResamplerProcess(fileName, stream, new SoxEncoderSettings(settings.Format.Depth, BASS_FLAGS)))
            {
                using (var encoderProcess = this.CreateEncoderProcess(fileName, stream, settings))
                {
                    using (var output = this.GetOutput(fileName, settings))
                    {
                        this.Encode(stream, output, resamplerProcess, encoderProcess);
                        resamplerProcess.WaitForExit();
                        encoderProcess.WaitForExit();
                        {
                            var errors = resamplerProcess.StandardError.ReadToEnd();
                            if (!string.IsNullOrEmpty(errors))
                            {
                                this.Errors.TryAdd(resamplerProcess.Id, errors);
                            }
                        }
                        {
                            var errors = encoderProcess.StandardError.ReadToEnd();
                            if (!string.IsNullOrEmpty(errors))
                            {
                                this.Errors.TryAdd(encoderProcess.Id, errors);
                            }
                        }
                    }
                }
            }
        }

        protected virtual void Encode(IBassStream input, Stream output, Process resamplerProcess, Process encoderProcess)
        {
            var channelReader = new ChannelReader(input);
            var resamplerReader = new ProcessReader(resamplerProcess);
            var resamplerWriter = new ProcessWriter(resamplerProcess);
            var encoderReader = new ProcessReader(encoderProcess);
            var encoderWriter = new ProcessWriter(encoderProcess);
            var threads = new[]
            {
                new Thread(() =>
                {
                    channelReader.CopyTo(resamplerWriter);
                }),
                new Thread(() =>
                {
                    resamplerReader.CopyTo(encoderWriter);
                }),
                new Thread(() =>
                {
                    encoderReader.CopyTo(output);
                })
            };
            foreach (var thread in threads)
            {
                thread.Start();
            }
            foreach (var thread in threads)
            {
                thread.Join();
            }
        }

        protected virtual Process CreateResamplerProcess(string fileName, IBassStream stream, IBassEncoderSettings settings)
        {
            var channelInfo = default(ChannelInfo);
            if (!Bass.ChannelGetInfo(stream.ChannelHandle, out channelInfo))
            {
                throw new NotImplementedException();
            }
            var length = this.GetEffectiveLength(Bass.ChannelGetLength(stream.ChannelHandle, PositionFlags.Bytes), settings);
            var processStartInfo = new ProcessStartInfo()
            {
                FileName = settings.Executable,
                WorkingDirectory = settings.Directory,
                Arguments = settings.GetArguments(channelInfo.Frequency, channelInfo.Channels, length),
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };
            var process = Process.Start(processStartInfo);
            return process;
        }

        protected virtual Process CreateEncoderProcess(string fileName, IBassStream stream, IBassEncoderSettings settings)
        {
            var channelInfo = default(ChannelInfo);
            if (!Bass.ChannelGetInfo(stream.ChannelHandle, out channelInfo))
            {
                throw new NotImplementedException();
            }
            var length = this.GetEffectiveLength(Bass.ChannelGetLength(stream.ChannelHandle, PositionFlags.Bytes), settings);
            var processStartInfo = new ProcessStartInfo()
            {
                FileName = settings.Executable,
                WorkingDirectory = settings.Directory,
                Arguments = settings.GetArguments(channelInfo.Frequency, channelInfo.Channels, length),
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };
            var process = Process.Start(processStartInfo);
            return process;
        }

        protected virtual Stream GetOutput(string fileName, IBassEncoderSettings settings)
        {
            return File.Create(settings.GetOutput(fileName));
        }

        protected virtual long GetEffectiveLength(long length, IBassEncoderSettings settings)
        {
            var source = default(int);
            if (BASS_FLAGS.HasFlag(BassFlags.Float))
            {
                source = 32;
            }
            else
            {
                source = 16;
            }
            return length / (source / settings.Format.Depth);
        }
    }
}
