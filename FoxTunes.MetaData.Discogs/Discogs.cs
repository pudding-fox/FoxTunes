using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TinyJson;

namespace FoxTunes
{
    public class Discogs : BaseComponent
    {
        public const string BASE_URL = "https://api.discogs.com";

        public const string KEY = "XdvMyQSHURdKVmPQJrXv";

        public const string SECRET = "VphFeVoRgCzPewlHHWstAheIKSBduLBR";

        public const int MAX_REQUESTS = 2;

        public Discogs(string baseUrl = BASE_URL, string key = KEY, string secret = SECRET, int maxRequests = MAX_REQUESTS)
        {
            this.BaseUrl = string.IsNullOrEmpty(baseUrl) ? BASE_URL : baseUrl;
            this.Key = string.IsNullOrEmpty(key) ? KEY : key;
            this.Secret = string.IsNullOrEmpty(secret) ? SECRET : secret;
            if (maxRequests < 0 || maxRequests > 10)
            {
                this.RateLimiter = new RateLimiter(1000 / MAX_REQUESTS);
            }
            else
            {
                this.RateLimiter = new RateLimiter(1000 / maxRequests);
            }
        }

        public string BaseUrl { get; private set; }

        public string Key { get; private set; }

        public string Secret { get; private set; }

        public RateLimiter RateLimiter { get; private set; }

        public async Task<IEnumerable<Release>> GetReleases(string artist, string album)
        {
            var url = string.Format(
                "{0}/database/search?type=master&artist={1}&release_title={2}",
                this.BaseUrl,
                Uri.EscapeDataString(artist),
                Uri.EscapeDataString(album)
            );
            Logger.Write(this, LogLevel.Debug, "Querying the API: {0}", url);
            var request = this.CreateRequest(url);
            using (var response = (HttpWebResponse)request.GetResponse())
            {
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    Logger.Write(this, LogLevel.Warn, "Status code does not indicate success.");
                    return Enumerable.Empty<Release>();
                }
                return await this.GetReleases(response.GetResponseStream()).ConfigureAwait(false);
            }
        }

        protected virtual async Task<IEnumerable<Release>> GetReleases(Stream stream)
        {
            const string RESULTS = "results";
            using (var reader = new StreamReader(stream))
            {
                var json = await reader.ReadToEndAsync().ConfigureAwait(false);
                var data = json.FromJson<Dictionary<string, object>>();
                if (data != null)
                {
                    var results = default(object);
                    if (data.TryGetValue(RESULTS, out results) && results is IList<object> resultsList)
                    {
                        return Release.FromResults(resultsList);
                    }
                }
            }
            return Enumerable.Empty<Release>();
        }

        public Task<byte[]> GetData(string url)
        {
            Logger.Write(this, LogLevel.Debug, "Querying the API: {0}", url);
            var request = this.CreateRequest(url);
            using (var response = (HttpWebResponse)request.GetResponse())
            {
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    Logger.Write(this, LogLevel.Warn, "Status code does not indicate success.");
#if NET40
                    return TaskEx.FromResult(default(byte[]));
#else
                    return Task.FromResult(default(byte[]));
#endif
                }
                return this.GetData(response.GetResponseStream());
            }
        }

        protected virtual Task<byte[]> GetData(Stream stream)
        {
            using (var reader = new BinaryReader(stream))
            {
                using (var temp = new MemoryStream())
                {
                    var buffer = new byte[10240];
                    var count = default(int);
                read:
                    count = reader.Read(buffer, 0, buffer.Length);
                    if (count == 0)
                    {
#if NET40
                        return TaskEx.FromResult(temp.ToArray());
#else
                        return Task.FromResult(temp.ToArray());
#endif
                    }
                    temp.Write(buffer, 0, count);
                    goto read;
                }
            }
        }

        protected virtual HttpWebRequest CreateRequest(string url)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            this.RateLimiter.Exec(() =>
            {
                request.UserAgent = string.Format("{0}/{1} +{2}", Publication.Product, Publication.Version, Publication.HomePage);
                if (this.Key != null && this.Secret != null)
                {
                    request.Headers[HttpRequestHeader.Authorization] = string.Format("Discogs key={0}, secret={1}", this.Key, this.Secret);
                }
            });
            return request;
        }

        private static readonly Regex IMAGE_SIZE = new Regex(@"(\d{3,4})x(\d{3,4})");

        public static int ImageSize(string url)
        {
            var match = IMAGE_SIZE.Match(url);
            if (match.Success)
            {
                var a = match.Groups[1].Value;
                var b = match.Groups[2].Value;
                var c = default(int);
                var d = default(int);
                int.TryParse(a, out c);
                int.TryParse(b, out d);
                return c * d;
            }
            else
            {
                return 0;
            }
        }

        public class ReleaseLookup
        {
            const int ERROR_CAPACITY = 10;

            private ReleaseLookup()
            {
                this.Id = Guid.NewGuid();
                this._Errors = new List<string>(ERROR_CAPACITY);
            }

            public ReleaseLookup(string artist, string album, bool isCompilation, IFileData[] fileDatas) : this()
            {
                this.Artist = artist;
                this.Album = album;
                this.IsCompilation = isCompilation;
                this.FileDatas = fileDatas;
            }

            public Guid Id { get; private set; }

            public string Artist { get; private set; }

            public string Album { get; private set; }

            public bool IsCompilation { get; private set; }

            public IFileData[] FileDatas { get; private set; }

            public Discogs.Release Release { get; set; }

            private IList<string> _Errors { get; set; }

            public IEnumerable<string> Errors
            {
                get
                {
                    return this._Errors;
                }
            }

            public ReleaseLookupStatus Status { get; set; }

            public void AddError(string error)
            {
                this._Errors.Add(error);
                if (this._Errors.Count > ERROR_CAPACITY)
                {
                    this._Errors.RemoveAt(0);
                }
            }

            public static IEnumerable<ReleaseLookup> FromFileDatas(IEnumerable<IFileData> fileDatas)
            {
                return fileDatas.GroupBy(fileData =>
                {
                    var metaData = default(IDictionary<string, string>);
                    lock (fileData.MetaDatas)
                    {
                        metaData = fileData.MetaDatas.ToDictionary(
                            metaDataItem => metaDataItem.Name,
                            metaDataItem => metaDataItem.Value,
                            StringComparer.OrdinalIgnoreCase
                        );
                    }
                    var artist = default(string);
                    var album = metaData.GetValueOrDefault(CommonMetaData.Album);
                    var isCompilation = string.Equals(
                        metaData.GetValueOrDefault(CustomMetaData.VariousArtists),
                        bool.TrueString,
                        StringComparison.OrdinalIgnoreCase
                    );
                    if (isCompilation)
                    {
                        artist = Strings.Discogs_CompilationArtist;
                    }
                    else
                    {
                        artist = metaData.GetValueOrDefault(CommonMetaData.Artist);
                    }
                    return new
                    {
                        Artist = artist,
                        Album = album,
                        IsCompilation = isCompilation
                    };
                }).Select(group => new ReleaseLookup(group.Key.Artist, group.Key.Album, group.Key.IsCompilation, group.ToArray()));
            }
        }

        public enum ReleaseLookupStatus : byte
        {
            None,
            Processing,
            Complete,
            Cancelled,
            Failed
        }

        public class Release
        {
            private Release(IDictionary<string, object> data)
            {
                const string ID = "id";
                const string URI = "uri";
                const string TITLE = "title";
                const string THUMB = "thumb";
                const string COVER_IMAGE = "cover_image";
                this.Id = Convert.ToString(data.GetValueOrDefault(ID));
                this.Url = Convert.ToString(data.GetValueOrDefault(URI));
                this.Title = Convert.ToString(data.GetValueOrDefault(TITLE));
                this.ThumbUrl = Convert.ToString(data.GetValueOrDefault(THUMB));
                this.CoverUrl = Convert.ToString(data.GetValueOrDefault(COVER_IMAGE));
            }

            public string Id { get; private set; }

            public string Url { get; private set; }

            public string Title { get; private set; }

            public string ThumbUrl { get; private set; }

            public int ThumbSize
            {
                get
                {
                    return ImageSize(this.ThumbUrl);
                }
            }

            public string CoverUrl { get; private set; }

            public int CoverSize
            {
                get
                {
                    return ImageSize(this.CoverUrl);
                }
            }

            public float Similarity(string artist, string album)
            {
                var title = string.Format("{0} - {1}", artist, album);
                var similarity = this.Title.Similarity(title, true);
                return similarity;
            }

            public static IEnumerable<Release> FromResults(IList<object> results)
            {
                foreach (var result in results)
                {
                    if (result is IDictionary<string, object> data)
                    {
                        yield return new Release(data);
                    }
                }
            }
        }
    }
}
