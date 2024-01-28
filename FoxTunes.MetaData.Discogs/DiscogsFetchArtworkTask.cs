using FoxTunes.Interfaces;
using System;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class DiscogsFetchArtworkTask : DiscogsLookupTask
    {
        private static readonly string PREFIX = typeof(DiscogsBehaviour).Name;

        public static readonly string FRONT_COVER = Enum.GetName(typeof(ArtworkType), ArtworkType.FrontCover);

        public DiscogsFetchArtworkTask(Discogs.ReleaseLookup[] releaseLookups) : base(releaseLookups)
        {
            this.Name = Strings.LookupArtworkTask_Name;
        }

        protected override async Task<bool> OnLookupSuccess(Discogs.ReleaseLookup releaseLookup)
        {
            var value = await this.ImportImage(releaseLookup).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(value))
            {
                releaseLookup.MetaData[FRONT_COVER] = value;
                return true;
            }
            else
            {
                Logger.Write(this, LogLevel.Warn, "Failed to download artwork for album {0} - {1}: Releases don't contain images or they count not be downloaded.", releaseLookup.Artist, releaseLookup.Album);
                releaseLookup.AddError(Strings.DiscogsFetchArtworkTask_NotFound);
                return false;
            }
        }

        protected virtual Task<string> ImportImage(Discogs.ReleaseLookup releaseLookup)
        {
            var urls = new[]
            {
                releaseLookup.Release.CoverUrl,
                releaseLookup.Release.ThumbUrl
            };
            foreach (var url in urls)
            {
                if (!string.IsNullOrEmpty(url))
                {
                    try
                    {
                        return FileMetaDataStore.IfNotExistsAsync(PREFIX, url, async result =>
                        {
                            Logger.Write(this, LogLevel.Debug, "Downloading data from url: {0}", url);
                            var data = await this.Discogs.GetData(url).ConfigureAwait(false);
                            return await FileMetaDataStore.WriteAsync(PREFIX, url, data).ConfigureAwait(false);
                        });
                    }
                    catch (Exception e)
                    {
                        Logger.Write(this, LogLevel.Error, "Failed to download data from url \"{0}\": {1}", url, e.Message);
                        releaseLookup.AddError(e.Message);
                    }
                }
            }
#if NET40
            return TaskEx.FromResult(string.Empty);
#else
            return Task.FromResult(string.Empty);
#endif
        }
    }
}
