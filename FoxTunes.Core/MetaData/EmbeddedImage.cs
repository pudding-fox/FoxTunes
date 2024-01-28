using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

namespace FoxTunes
{
    [Serializable]
    public class EmbeddedImage
    {
        private static readonly BinaryFormatter Formatter = new BinaryFormatter();

        public EmbeddedImage(string fileName, string mimeType, string imageType, string description)
        {
            this.FileName = FileName;
            this.MimeType = mimeType;
            this.ImageType = imageType;
            this.Description = description;
        }

        public string FileName { get; private set; }

        public string MimeType { get; private set; }

        public string ImageType { get; private set; }

        public string Description { get; private set; }

        public async Task<string> Encode()
        {
            using (var stream = new MemoryStream())
            {
                Formatter.Serialize(stream, this);
                await stream.FlushAsync();
                return Convert.ToBase64String(stream.ToArray());
            }
        }

        public static Task<EmbeddedImage> Decode(string values)
        {
            using (var stream = new MemoryStream(Convert.FromBase64String(values)))
            {
                return Task.FromResult((EmbeddedImage)Formatter.Deserialize(stream));
            }
        }
    }
}
