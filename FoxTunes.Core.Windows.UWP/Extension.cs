using Windows.Storage.Streams;

namespace FoxTunes
{
    public static partial class Extension
    {
        public static IRandomAccessStream ToRandomAccessStream(this byte[] bytes)
        {
            var stream = new InMemoryRandomAccessStream();
            using (var writer = new DataWriter(stream.GetOutputStreamAt(0)))
            {
                writer.WriteBytes(bytes);
            }
            return stream;
        }
    }
}
