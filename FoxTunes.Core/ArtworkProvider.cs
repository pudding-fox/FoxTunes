using FoxTunes.Interfaces;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class ArtworkProvider : StandardComponent, IArtworkProvider
    {
        const int MAX_LENGTH = 1024000;

        const string DELIMITER = ",";

        public static readonly string[] EXTENSIONS = new[] { ".jpg", ".jpeg", ".png", ".bmp", ".bin" };

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
            if (!string.IsNullOrEmpty(Path.GetPathRoot(path)))
            {
                var exception = default(Exception);
                try
                {
                    var directoryName = Path.GetDirectoryName(path);
                    foreach (var name in names)
                    {
                        foreach (var fileName in FileSystemHelper.EnumerateFiles(directoryName, string.Format("{0}.*", name)))
                        {
                            var info = new FileInfo(fileName);
                            if (!EXTENSIONS.Contains(info.Extension, true))
                            {
                                continue;
                            }
                            if (info.Length <= MAX_LENGTH)
                            {
                                return new MetaDataItem(Enum.GetName(typeof(ArtworkType), type), MetaDataItemType.Image)
                                {
                                    Value = fileName
                                };
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
            }
            return default(MetaDataItem);
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
    }
}
