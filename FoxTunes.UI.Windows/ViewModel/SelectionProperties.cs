using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FoxTunes.ViewModel
{
    public class SelectionProperties : ConfigurableViewModelBase
    {
        const int TIMEOUT = 100;

        const string DELIMITER = "; ";

        private static readonly string[] TAGS = new[]
        {
            CommonMetaData.Album,
            CommonMetaData.Artist,
            CommonMetaData.Composer,
            CommonMetaData.Conductor,
            CommonMetaData.Disc,
            CommonMetaData.DiscCount,
            CommonMetaData.Genre,
            CommonMetaData.Performer,
            CommonMetaData.Title,
            CommonMetaData.Track,
            CommonMetaData.TrackCount,
            CommonMetaData.Year,
            CommonMetaData.IsCompilation
        };

        private static readonly string[] PROPERTIES = new[]
        {
            CommonProperties.AudioBitrate,
            CommonProperties.AudioChannels,
            CommonProperties.AudioSampleRate,
            CommonProperties.BitsPerSample,
            CommonProperties.Duration
        };

        private static readonly string[] IMAGES = new[]
        {
            CommonImageTypes.FrontCover
        };

        public SelectionProperties() : base(false)
        {
            this.Debouncer = new Debouncer(TIMEOUT);
            this.FileDatas = new ObservableCollection<IFileData>();
            this.Tags = new ObservableCollection<Row>();
            this.Properties = new ObservableCollection<Row>();
            this.Images = new ObservableCollection<Row>();
            if (Core.Instance != null)
            {
                this.InitializeComponent(Core.Instance);
            }
        }

        public Debouncer Debouncer { get; private set; }

        public ObservableCollection<IFileData> FileDatas { get; private set; }

        public ObservableCollection<Row> Tags { get; private set; }

        public ObservableCollection<Row> Properties { get; private set; }

        public ObservableCollection<Row> Images { get; private set; }

        public ILibraryManager LibraryManager { get; private set; }

        public IPlaylistManager PlaylistManager { get; private set; }

        public ILibraryHierarchyBrowser LibraryHierarchyBrowser { get; private set; }

        private bool _ShowTags { get; set; }

        public bool ShowTags
        {
            get
            {
                return this._ShowTags;
            }
            set
            {
                this._ShowTags = value;
                this.OnShowTagsChanged();
            }
        }

        protected virtual void OnShowTagsChanged()
        {
            this.Debouncer.Exec(this.Refresh);
            if (this.ShowTagsChanged != null)
            {
                this.ShowTagsChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("ShowTags");
        }

        public event EventHandler ShowTagsChanged;

        private bool _ShowProperties { get; set; }

        public bool ShowProperties
        {
            get
            {
                return this._ShowProperties;
            }
            set
            {
                this._ShowProperties = value;
                this.OnShowPropertiesChanged();
            }
        }

        protected virtual void OnShowPropertiesChanged()
        {
            this.Debouncer.Exec(this.Refresh);
            if (this.ShowPropertiesChanged != null)
            {
                this.ShowPropertiesChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("ShowProperties");
        }

        public event EventHandler ShowPropertiesChanged;

        private bool _ShowImages { get; set; }

        public bool ShowImages
        {
            get
            {
                return this._ShowImages;
            }
            set
            {
                this._ShowImages = value;
                this.OnShowImagesChanged();
            }
        }

        protected virtual void OnShowImagesChanged()
        {
            this.Debouncer.Exec(this.Refresh);
            if (this.ShowImagesChanged != null)
            {
                this.ShowImagesChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("ShowImages");
        }

        public event EventHandler ShowImagesChanged;

        protected override void InitializeComponent(ICore core)
        {
            this.LibraryManager = core.Managers.Library;
            this.LibraryManager.SelectedItemChanged += this.OnSelectedItemChanged;
            this.PlaylistManager = core.Managers.Playlist;
            this.PlaylistManager.SelectedItemsChanged += this.OnSelectedItemsChanged;
            this.LibraryHierarchyBrowser = core.Components.LibraryHierarchyBrowser;
            this.Debouncer.Exec(this.Refresh);
            base.InitializeComponent(core);
        }

        protected override void OnConfigurationChanged()
        {
            if (this.Configuration != null)
            {
                this.Configuration.GetElement<BooleanConfigurationElement>(
                    SelectionPropertiesConfiguration.SECTION,
                    SelectionPropertiesConfiguration.SHOW_TAGS
                ).ConnectValue(value => this.ShowTags = value);
                this.Configuration.GetElement<BooleanConfigurationElement>(
                    SelectionPropertiesConfiguration.SECTION,
                    SelectionPropertiesConfiguration.SHOW_PROPERTIES
                ).ConnectValue(value => this.ShowProperties = value);
                this.Configuration.GetElement<BooleanConfigurationElement>(
                    SelectionPropertiesConfiguration.SECTION,
                    SelectionPropertiesConfiguration.SHOW_IMAGES
                ).ConnectValue(value => this.ShowImages = value);
            }
            base.OnConfigurationChanged();
        }

        protected virtual void OnSelectedItemChanged(object sender, EventArgs e)
        {
            if (this.LibraryManager.SelectedItem == null)
            {
                return;
            }
            var task = this.Refresh(this.LibraryManager.SelectedItem);
        }

        protected virtual void OnSelectedItemsChanged(object sender, EventArgs e)
        {
            if (this.PlaylistManager.SelectedItems == null || !this.PlaylistManager.SelectedItems.Any())
            {
                return;
            }
            var task = this.Refresh(this.PlaylistManager.SelectedItems);
        }

        protected virtual void Refresh()
        {
            var task = this.Refresh(this.FileDatas.ToArray());
        }

        protected virtual Task Refresh(LibraryHierarchyNode libraryHierarchyNode)
        {
            var libraryItems = this.LibraryHierarchyBrowser.GetItems(libraryHierarchyNode);
            if (libraryItems == null || !libraryItems.Any())
            {
#if NET40
                return TaskEx.FromResult(false);
#else
                return Task.CompletedTask;
#endif
            }
            return this.Refresh(libraryItems);
        }

        protected virtual Task Refresh(IEnumerable<IFileData> fileDatas)
        {
            var metaDatas = this.GetMetaDatas(fileDatas);
            var tags = this.GetTags(fileDatas, metaDatas);
            var properties = this.GetProperties(fileDatas, metaDatas);
            var images = this.GetImages(fileDatas, metaDatas);
            return Windows.Invoke(() =>
            {
                this.FileDatas.Clear();
                this.FileDatas.AddRange(fileDatas);
                this.Tags.Clear();
                this.Tags.AddRange(tags);
                this.Properties.Clear();
                this.Properties.AddRange(properties);
                this.Images.Clear();
                this.Images.AddRange(images);
            });
        }

        protected virtual IDictionary<IFileData, IDictionary<string, string>> GetMetaDatas(IEnumerable<IFileData> fileDatas)
        {
            return fileDatas.ToDictionary(
                fileData => fileData,
                fileData => this.GetMetaData(fileData)
            );
        }

        protected virtual IDictionary<string, string> GetMetaData(IFileData fileData)
        {
            if (fileData.MetaDatas == null)
            {
                return null;
            }
            lock (fileData.MetaDatas)
            {
                return fileData.MetaDatas.ToDictionary(
                    metaDataItem => metaDataItem.Name,
                    metaDataItem => metaDataItem.Value,
                    StringComparer.OrdinalIgnoreCase
                );
            }
        }

        protected virtual IEnumerable<Row> GetTags(IEnumerable<IFileData> fileDatas, IDictionary<IFileData, IDictionary<string, string>> metaDatas)
        {
            if (this.ShowTags)
            {
                foreach (var tag in TAGS)
                {
                    yield return this.GetRow(tag, fileDatas, metaDatas);
                }
            }
        }

        protected virtual IEnumerable<Row> GetProperties(IEnumerable<IFileData> fileDatas, IDictionary<IFileData, IDictionary<string, string>> metaDatas)
        {
            if (this.ShowProperties)
            {
                foreach (var property in PROPERTIES)
                {
                    yield return this.GetRow(property, fileDatas, metaDatas);
                }
            }
        }

        protected virtual IEnumerable<Row> GetImages(IEnumerable<IFileData> fileDatas, IDictionary<IFileData, IDictionary<string, string>> metaDatas)
        {
            if (this.ShowImages)
            {
                foreach (var image in IMAGES)
                {
                    yield return this.GetRow(image, fileDatas, metaDatas);
                }
            }
        }

        protected virtual Row GetRow(string name, IEnumerable<IFileData> fileDatas, IDictionary<IFileData, IDictionary<string, string>> metaDatas)
        {
            var result = new StringBuilder();
            var history = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var fileData in fileDatas)
            {
                var value = default(string);
                var metaData = default(IDictionary<string, string>);
                if (!metaDatas.TryGetValue(fileData, out metaData) || metaData == null)
                {
                    value = Strings.SelectionProperties_NoValue;
                }
                else
                {
                    if (!metaData.TryGetValue(name, out value))
                    {
                        value = Strings.SelectionProperties_NoValue;
                    }
                }
                if (!history.Add(value))
                {
                    continue;
                }
                if (result.Length > 0)
                {
                    result.Append(DELIMITER);
                }
                result.Append(value);
            }
            return new Row(name, result.ToString());
        }

        protected override Freezable CreateInstanceCore()
        {
            return new SelectionProperties();
        }

        protected override void OnDisposing()
        {
            if (this.LibraryManager != null)
            {
                this.LibraryManager.SelectedItemChanged -= this.OnSelectedItemChanged;
            }
            if (this.PlaylistManager != null)
            {
                this.PlaylistManager.SelectedItemsChanged -= this.OnSelectedItemsChanged;
            }
            base.OnDisposing();
        }

        public class Row
        {
            public Row(string name, string value)
            {
                this.Name = name;
                this.Value = value;
            }

            public string Name { get; private set; }

            public string Value { get; private set; }
        }
    }
}
