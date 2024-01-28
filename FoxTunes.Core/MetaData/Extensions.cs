using FoxTunes.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes
{
    public static partial class Extensions
    {
        public static async Task<IEnumerable<EmbeddedImage>> GetEmbeddedImages(this IMetaDataSource source)
        {
            var embeddedImages = new List<EmbeddedImage>();
            var metaDataItems = source.MetaDatas.Where(metaDataItem => !string.IsNullOrEmpty(metaDataItem.FileValue));
            foreach (var metaDataItem in metaDataItems)
            {
                embeddedImages.Add(await EmbeddedImage.Decode(metaDataItem.FileValue));
            }
            return embeddedImages;
        }
    }
}
