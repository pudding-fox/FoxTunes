using FoxTunes.Interfaces;
using ManagedBass;

namespace FoxTunes
{
    public class ChannelReader
    {
        const int BUFFER_SIZE = 10240;

        public ChannelReader(IBassStream stream)
        {
            this.Stream = stream;
        }

        public IBassStream Stream { get; private set; }

        public void CopyTo(ProcessWriter writer)
        {
            var length = default(int);
            var buffer = new byte[BUFFER_SIZE];
            while ((length = Bass.ChannelGetData(this.Stream.ChannelHandle, buffer, BUFFER_SIZE)) > 0)
            {
                writer.Write(buffer, length);
            }
            writer.Close();
        }
    }
}
