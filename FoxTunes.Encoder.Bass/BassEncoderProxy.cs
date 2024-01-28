using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace FoxTunes
{
    public class BassEncoderProxy : BaseComponent, IBassEncoder
    {
        public static readonly object ReadSyncRoot = new object();

        public static readonly object WriteSyncRoot = new object();

        public BassEncoderProxy(Process process, IEnumerable<EncoderItem> encoderItems)
        {
            this.Process = process;
            this.EncoderItems = encoderItems;
        }

        public Process Process { get; private set; }

        public IEnumerable<EncoderItem> EncoderItems { get; private set; }

        public bool IsCancelling { get; private set; }

        public bool IsComplete { get; private set; }

        public void Encode()
        {
            Logger.Write(this, LogLevel.Debug, "Sending {0} items to encoder container process.", this.EncoderItems.Count());
            this.Send(this.EncoderItems.ToArray());
            Logger.Write(this, LogLevel.Debug, "Waiting for encoder container process to complete.");
            this.Process.WaitForExit();
            if (this.Process.ExitCode != 0)
            {
                throw new InvalidOperationException("Process does not indicate success.");
            }
        }

        public void Update()
        {
            var value = this.Recieve();
            if (value == null)
            {
                return;
            }
            if (value is EncoderStatus)
            {
                this.UpdateStatus(value as EncoderStatus);
            }
            else if (value is IEnumerable<EncoderItem>)
            {
                this.UpdateItems(value as IEnumerable<EncoderItem>);
            }
        }

        public void Cancel()
        {
            Logger.Write(this, LogLevel.Debug, "Sending cancel command to encoder container process.");
            this.Send(new EncoderCommand(EncoderCommandType.Cancel));
            this.Process.StandardInput.Close();
        }

        public void Quit()
        {
            Logger.Write(this, LogLevel.Debug, "Sending quit command to encoder container process.");
            this.Send(new EncoderCommand(EncoderCommandType.Quit));
            this.Process.StandardInput.Close();
        }

        protected virtual void Send(object value)
        {
            lock (WriteSyncRoot)
            {
                try
                {
                    if (this.Process.StandardInput.BaseStream != null && this.Process.StandardInput.BaseStream.CanWrite)
                    {
                        Serializer.Instance.Write(this.Process.StandardInput.BaseStream, value);
                        this.Process.StandardInput.Flush();
                    }
                }
                catch
                {
                    //Nothing can be done.
                }
            }
        }

        protected virtual object Recieve()
        {
            lock (ReadSyncRoot)
            {
                try
                {
                    if (this.Process.StandardOutput.BaseStream != null && this.Process.StandardOutput.BaseStream.CanRead)
                    {
                        var value = Serializer.Instance.Read(this.Process.StandardOutput.BaseStream);
                        return value;
                    }
                }
                catch
                {
                    //Nothing can be done.
                }
            }
            return null;
        }

        protected virtual void UpdateStatus(EncoderStatus status)
        {
            Logger.Write(this, LogLevel.Debug, "Recieved status from encoder container process: {0}", Enum.GetName(typeof(EncoderStatusType), status.Type));
            switch (status.Type)
            {
                case EncoderStatusType.Complete:
                case EncoderStatusType.Error:
                    Logger.Write(this, LogLevel.Debug, "Fetching final status and shutting down encoder container process.");
                    this.Update();
                    this.Quit();
                    this.Process.StandardInput.Close();
                    this.Process.StandardOutput.Close();
                    break;
            }
        }

        protected virtual void UpdateItems(IEnumerable<EncoderItem> encoderItems)
        {
            this.EncoderItems = encoderItems;
        }

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
            if (this.Process != null)
            {
                if (!this.Process.HasExited)
                {
                    Logger.Write(this, LogLevel.Warn, "Process is incomplete.");
                    this.Process.Kill();
                }
                this.Process.Dispose();
            }
        }

        ~BassEncoderProxy()
        {
            Logger.Write(this, LogLevel.Error, "Component was not disposed: {0}", this.GetType().Name);
            try
            {
                this.Dispose(true);
            }
            catch
            {
                //Nothing can be done, never throw on GC thread.
            }
        }
    }
}
