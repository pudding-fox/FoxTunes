using FoxTunes.Interfaces;
using ManagedBass;

namespace FoxTunes
{
    public class ChannelReader
    {
        protected static ILogger Logger
        {
            get
            {
                return LogManager.Logger;
            }
        }

        const int BUFFER_SIZE = 102400;

        public ChannelReader(EncoderItem encoderItem, IBassStream stream)
        {
            this.EncoderItem = encoderItem;
            this.Stream = stream;
        }

        public EncoderItem EncoderItem { get; private set; }

        public IBassStream Stream { get; private set; }

        public void CopyTo(ProcessWriter writer, CancellationToken cancellationToken)
        {
            Logger.Write(this.GetType(), LogLevel.Debug, "Begin reading data from channel {0} with {1} byte buffer.", this.Stream.ChannelHandle, BUFFER_SIZE);
            var length = default(int);
            var buffer = new byte[BUFFER_SIZE];
            while ((length = Bass.ChannelGetData(this.Stream.ChannelHandle, buffer, BUFFER_SIZE)) > 0)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                writer.Write(buffer, length);
                this.Update();
            }
            Logger.Write(this.GetType(), LogLevel.Debug, "Finished reading data from channel {0}, closing process input.", this.Stream.ChannelHandle);
            writer.Close();
        }

        protected virtual void Update()
        {
            this.EncoderItem.Progress = (int)((this.Stream.Position / (double)this.Stream.Length) * EncoderItem.PROGRESS_COMPLETE);
        }
    }
}
