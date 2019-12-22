using FoxTunes.Interfaces;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class ArtworkProvider : StandardComponent, IArtworkProvider
    {
        const int CACHE_SIZE = 5120;

        const int MAX_LENGTH = 1024000;

        const string DELIMITER = ",";

        public static readonly string[] EXTENSIONS = new[] { ".jpg", ".jpeg", ".png", ".bmp", ".bin" };

        public ArtworkProvider()
        {
            this.Store = new Cache(CACHE_SIZE);
        }

        public Cache Store { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public bool Enabled { get; private set; }

        public string[] Front { get; private set; }

        public string[] Back { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Configuration = core.Components.Configuration;
            this.Configuration.GetElement<BooleanConfigurationElement>(
                MetaDataBehaviourConfiguration.SECTION,
                MetaDataBehaviourConfiguration.READ_LOOSE_IMAGES
            ).ConnectValue(value => this.Enabled = value);
            this.Configuration.GetElement<TextConfigurationElement>(
               MetaDataBehaviourConfiguration.SECTION,
               MetaDataBehaviourConfiguration.LOOSE_IMAGES_FRONT
           ).ConnectValue(value => this.Front = this.Parse(value));
            this.Configuration.GetElement<TextConfigurationElement>(
               MetaDataBehaviourConfiguration.SECTION,
               MetaDataBehaviourConfiguration.LOOSE_IMAGES_BACK
           ).ConnectValue(value => this.Back = this.Parse(value));
            base.InitializeComponent(core);
        }

        protected virtual string[] Parse(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return new string[] { };
            }
            return value
                .Split(new[] { DELIMITER }, StringSplitOptions.RemoveEmptyEntries)
                .Select(element => element.Trim())
                .ToArray();
        }

        public async Task<MetaDataItem> Find(string path, ArtworkType type)
        {
            var metaDataItem = default(MetaDataItem);
            if (string.IsNullOrEmpty(Path.GetPathRoot(path)))
            {
                return metaDataItem;
            }
            var directoryName = Path.GetDirectoryName(path);
            if (this.Store.TryGetValue(directoryName, type, out metaDataItem))
            {
                return metaDataItem;
            }
            var names = default(string[]);
            switch (type)
            {
                case ArtworkType.FrontCover:
                    names = this.Front;
                    break;
                case ArtworkType.BackCover:
                    names = this.Back;
                    break;
                default:
                    throw new NotImplementedException();
            }
            var exception = default(Exception);
            try
            {
                foreach (var name in names)
                {
                    foreach (var fileName in FileSystemHelper.EnumerateFiles(directoryName, string.Format("{0}.*", name), FileSystemHelper.SearchOption.None))
                    {
                        var info = new FileInfo(fileName);
                        if (!EXTENSIONS.Contains(info.Extension, true))
                        {
                            continue;
                        }
                        if (info.Length <= MAX_LENGTH)
                        {
                            metaDataItem = new MetaDataItem(Enum.GetName(typeof(ArtworkType), type), MetaDataItemType.Image)
                            {
                                Value = fileName
                            };
                            this.Store.Add(directoryName, type, metaDataItem);
                            return metaDataItem;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                exception = e;
            }
            if (exception != null)
            {
                await this.OnError(exception);
            }
            return metaDataItem;
        }

        public Task<MetaDataItem> Find(PlaylistItem playlistItem, ArtworkType type)
        {
            var result = playlistItem.MetaDatas.FirstOrDefault(
                 metaDataItem =>
                     metaDataItem.Type == MetaDataItemType.Image &&
                     string.Equals(metaDataItem.Name, Enum.GetName(typeof(ArtworkType), type), StringComparison.OrdinalIgnoreCase) &&
                     File.Exists(metaDataItem.Value)
             );
#if NET40
            return TaskEx.FromResult(result);
#else
            return Task.FromResult(result);
#endif
        }

        public class Cache
        {
            public Cache(int capacity)
            {
                this.Store = new CappedDictionary<Key, MetaDataItem>(capacity);
            }

            public CappedDictionary<Key, MetaDataItem> Store { get; private set; }

            public void Add(string path, ArtworkType type, MetaDataItem metaDataItem)
            {
                var key = new Key(path, type);
                this.Store.Add(key, metaDataItem);
            }

            public bool TryGetValue(string path, ArtworkType type, out MetaDataItem metaDataItem)
            {
                var key = new Key(path, type);
                return this.Store.TryGetValue(key, out metaDataItem);
            }

            public class Key : IEquatable<Key>
            {
                public Key(string path, ArtworkType type)
                {
                    this.Path = path;
                    this.Type = type;
                }

                public string Path { get; private set; }

                public ArtworkType Type { get; private set; }

                public virtual bool Equals(Key other)
                {
                    if (other == null)
                    {
                        return false;
                    }
                    if (object.ReferenceEquals(this, other))
                    {
                        return true;
                    }
                    if (!string.Equals(this.Path, other.Path, StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }
                    if (this.Type != other.Type)
                    {
                        return false;
                    }
                    return true;
                }

                public override bool Equals(object obj)
                {
                    return this.Equals(obj as Key);
                }

                public override int GetHashCode()
                {
                    var hashCode = default(int);
                    unchecked
                    {
                        if (!string.IsNullOrEmpty(this.Path))
                        {
                            hashCode += this.Path.ToLower().GetHashCode();
                        }
                        hashCode += this.Type.GetHashCode();
                    }
                    return hashCode;
                }

                public static bool operator ==(Key a, Key b)
                {
                    if ((object)a == null && (object)b == null)
                    {
                        return true;
                    }
                    if ((object)a == null || (object)b == null)
                    {
                        return false;
                    }
                    if (object.ReferenceEquals((object)a, (object)b))
                    {
                        return true;
                    }
                    return a.Equals(b);
                }

                public static bool operator !=(Key a, Key b)
                {
                    return !(a == b);
                }
            }
        }
    }
}
