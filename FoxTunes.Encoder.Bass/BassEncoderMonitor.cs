using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class BassEncoderMonitor : PopulatorBase, IBassEncoderMonitor
    {
        public static readonly TimeSpan INTERVAL = TimeSpan.FromSeconds(1);

        public BassEncoderMonitor(IBassEncoder encoder, bool reportProgress, CancellationToken cancellationToken) : base(reportProgress)
        {
            this.EncoderItems = new Dictionary<EncoderItem, EncoderItemStatus>();
            this.Encoder = encoder;
            this.CancellationToken = cancellationToken;
        }

        public IDictionary<EncoderItem, EncoderItemStatus> EncoderItems { get; private set; }

        public IBassEncoder Encoder { get; private set; }

        public CancellationToken CancellationToken { get; private set; }

        public Task Encode()
        {
#if NET40
            var task = TaskEx.Run(() =>
#else
            var task = Task.Run(() =>
#endif
            {
                this.Encoder.Encode();
            });
#if NET40
            return TaskEx.WhenAll(task, this.Monitor(task));
#else
            return Task.WhenAll(task, this.Monitor(task));
#endif
        }

        protected virtual async Task Monitor(Task task)
        {
            await this.SetName("Converting files");
            while (!task.IsCompleted)
            {
                if (this.CancellationToken.IsCancellationRequested)
                {
                    Logger.Write(this, LogLevel.Debug, "Requesting cancellation from encoder.");
                    this.Encoder.Cancel();
                    await this.SetName("Cancelling");
                    break;
                }
                this.Encoder.Update();
                var position = 0;
                var count = 0;
                var builder = new StringBuilder();
                foreach (var encoderItem in this.Encoder.EncoderItems)
                {
                    this.UpdateStatus(encoderItem);
                    position += encoderItem.Progress;
                    count += EncoderItem.PROGRESS_COMPLETE;
                    if (encoderItem.Status == EncoderItemStatus.Processing)
                    {
                        if (builder.Length > 0)
                        {
                            builder.Append(", ");
                        }
                        builder.Append(Path.GetFileName(encoderItem.InputFileName));
                    }
                }
                await this.SetDescription(builder.ToString());
                await this.SetPosition(position);
                await this.SetCount(count);
#if NET40
                await TaskEx.Delay(INTERVAL);
#else
                await Task.Delay(INTERVAL);
#endif
            }
            while (!task.IsCompleted)
            {
                Logger.Write(this, LogLevel.Debug, "Waiting for encoder to complete.");
                this.Encoder.Update();
#if NET40
                await TaskEx.Delay(INTERVAL);
#else
                await Task.Delay(INTERVAL);
#endif
            }
            var exceptions = new List<Exception>();
            foreach (var encoderItem in this.Encoder.EncoderItems)
            {
                this.UpdateStatus(encoderItem);
                if (encoderItem.Status != EncoderItemStatus.Failed)
                {
                    continue;
                }
                foreach (var error in encoderItem.Errors)
                {
                    exceptions.Add(new Exception(error));
                }
            }
            if (exceptions.Any())
            {
                if (exceptions.Count == 1)
                {
                    throw exceptions.First();
                }
                throw new AggregateException(exceptions);
            }
        }

        protected virtual void UpdateStatus(EncoderItem encoderItem)
        {
            var status = default(EncoderItemStatus);
            if (!this.EncoderItems.TryGetValue(encoderItem, out status))
            {
                this.EncoderItems.Add(encoderItem, EncoderItemStatus.None);
            }
            else if (encoderItem.Status != status)
            {
                Logger.Write(this, LogLevel.Debug, "Encoder status changed for file \"{0}\": {1}", encoderItem.InputFileName, Enum.GetName(typeof(EncoderItemStatus), encoderItem.Status));
                this.EncoderItems[encoderItem] = encoderItem.Status;
                this.OnStatusChanged(encoderItem);
            }
        }

        protected virtual void OnStatusChanged(EncoderItem encoderItem)
        {
            if (this.StatusChanged == null)
            {
                return;
            }
            this.StatusChanged(this, new BassEncoderMonitorEventArgs(encoderItem));
        }

        public event BassEncoderMonitorEventHandler StatusChanged;
    }

    public delegate void BassEncoderMonitorEventHandler(object sender, BassEncoderMonitorEventArgs e);

    public class BassEncoderMonitorEventArgs : EventArgs
    {
        public BassEncoderMonitorEventArgs(EncoderItem encoderItem)
        {
            this.EncoderItem = encoderItem;
        }

        public EncoderItem EncoderItem { get; private set; }
    }
}
