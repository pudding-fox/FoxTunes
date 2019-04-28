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

        public BassEncoderMonitor(IBassEncoder encoder, bool reportProgress) : base(reportProgress)
        {
            this.Encoder = encoder;
        }

        public IBassEncoder Encoder { get; private set; }

        public Task Encode(EncoderItem[] encoderItems)
        {
#if NET40
            var task = TaskEx.Run(() =>
#else
            var task = Task.Run(() =>
#endif
            {
                this.Encoder.Encode(encoderItems);
            });
#if NET40
            return TaskEx.WhenAll(task, this.Monitor(encoderItems, task));
#else
            return Task.WhenAll(task, this.Monitor(encoderItems, task));
#endif
        }

        protected virtual async Task Monitor(IEnumerable<EncoderItem> encoderItems, Task task)
        {
            await this.SetName("Converting files");
            while (!task.IsCompleted)
            {
                var position = 0;
                var count = 0;
                var builder = new StringBuilder();
                foreach (var encoderItem in encoderItems)
                {
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
                await this.SetPosition(position);
                await this.SetCount(count);
#if NET40
                await TaskEx.Delay(INTERVAL);
#else
                await Task.Delay(INTERVAL);
#endif
            }
            var exceptions = new List<Exception>();
            foreach (var encoderItem in encoderItems)
            {
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
                throw new AggregateException(exceptions);
            }
        }
    }
}
