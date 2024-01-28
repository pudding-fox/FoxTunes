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

        public ChannelReader(ScannerItem scannerItem, IBassStream stream)
        {
            this.ScannerItem = scannerItem;
            this.Stream = stream;
        }

        public ScannerItem ScannerItem { get; private set; }

        public IBassStream Stream { get; private set; }

        protected virtual void Update()
        {
            var position = Bass.ChannelGetPosition(this.Stream.ChannelHandle, PositionFlags.Bytes);
            var length = Bass.ChannelGetLength(this.Stream.ChannelHandle, PositionFlags.Bytes);
            this.ScannerItem.Progress = (int)((position / (double)length) * ScannerItem.PROGRESS_COMPLETE);
        }
    }
}
