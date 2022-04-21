using FoxTunes.Interfaces;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace FoxTunes
{
    public class Resampler : BaseComponent, IDisposable
    {
        public static string FileName
        {
            get
            {
                var directory = Path.GetDirectoryName(
                    typeof(Resampler).Assembly.Location
                );
                return Path.Combine(directory, "Encoders\\sox.exe");
            }
        }

        public static string WorkingDirectory
        {
            get
            {
                return Path.GetDirectoryName(FileName);
            }
        }

        public Resampler(ResamplerFormat inputFormat, ResamplerFormat outputFormat)
        {
            this.Process = CreateProcess(inputFormat, outputFormat);
        }

        public Process Process { get; private set; }

        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.IsDisposed || !disposing)
            {
                return;
            }
            this.OnDisposing();
            this.IsDisposed = true;
        }

        protected virtual void OnDisposing()
        {
            this.Process.Dispose();
        }

        ~Resampler()
        {
            Logger.Write(this.GetType(), LogLevel.Error, "Component was not disposed: {0}", this.GetType().Name);
            try
            {
                this.Dispose(true);
            }
            catch
            {
                //Nothing can be done, never throw on GC thread.
            }
        }

        private static Process CreateProcess(ResamplerFormat inputFormat, ResamplerFormat outputFormat)
        {
            var processStartInfo = new ProcessStartInfo()
            {
                FileName = FileName,
                WorkingDirectory = WorkingDirectory,
                Arguments = GetArguments(inputFormat, outputFormat),
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            var process = Process.Start(processStartInfo);
            try
            {
                process.PriorityClass = ProcessPriorityClass.BelowNormal;
            }
            catch
            {
                //Nothing can be done, probably access denied.
            }
            process.ErrorDataReceived += (sender, e) =>
            {
                if (string.IsNullOrEmpty(e.Data))
                {
                    return;
                }
                Logger.Write(typeof(Resampler), LogLevel.Trace, "{0}: {1}", FileName, e.Data);
            };
            process.BeginErrorReadLine();
            return process;
        }

        private static string GetArguments(ResamplerFormat inputFormat, ResamplerFormat outputFormat)
        {
            var builder = new StringBuilder();
            builder.Append(GetArguments(inputFormat));
            builder.Append(" ");
            builder.Append(GetArguments(outputFormat));
            return builder.ToString();
        }

        private static string GetArguments(ResamplerFormat format)
        {
            var encoding = default(string);
            var endian = default(string);
            GetEncoding(format.Encoding, out encoding, out endian);
            return string.Format(
                "-t raw --encoding {0} --endian {1} --bits {2} --rate {3} --channels {4} -",
                encoding,
                endian,
                format.Depth,
                format.Rate,
                format.Channels
            );
        }

        private static void GetEncoding(ResamplerEncoding format, out string encoding, out string endian)
        {
            if (format.HasFlag(ResamplerEncoding.SignedInteger))
            {
                encoding = "signed-integer";
            }
            else if (format.HasFlag(ResamplerEncoding.UnsignedInteger))
            {
                encoding = "unsigned-integer";
            }
            else if (format.HasFlag(ResamplerEncoding.FloatingPoint))
            {
                encoding = "floating-point";
            }
            else
            {
                throw new NotImplementedException();
            }

            if (format.HasFlag(ResamplerEncoding.EndianBig))
            {
                endian = "big";
            }
            else if (format.HasFlag(ResamplerEncoding.EndianBig))
            {
                endian = "little";
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }

    public class ResamplerFormat
    {
        public ResamplerFormat(ResamplerEncoding encoding, int depth, int rate, int channels)
        {
            this.Encoding = encoding;
            this.Depth = depth;
            this.Rate = rate;
            this.Channels = channels;
        }

        public ResamplerEncoding Encoding { get; private set; }

        public int Depth { get; private set; }

        public int Rate { get; private set; }

        public int Channels { get; private set; }
    }

    [Flags]
    public enum ResamplerEncoding : byte
    {
        None = 0,
        SignedInteger = 1,
        UnsignedInteger = 2,
        FloatingPoint = 4,
        EndianBig = 8,
        EndianLittle = 16
    }
}
