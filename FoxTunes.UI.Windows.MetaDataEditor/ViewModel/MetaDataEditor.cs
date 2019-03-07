using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace FoxTunes.ViewModel
{
    public class MetaDataEditor : ViewModelBase
    {
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
            CommonMetaData.Year
        };

        private static readonly string[] IMAGES = new[]
        {
            CommonImageTypes.FrontCover
        };

        private static readonly string[] PROPERTIES = new[]
        {
            CommonProperties.Duration,
            CommonProperties.AudioBitrate,
            CommonProperties.AudioChannels,
            CommonProperties.AudioSampleRate,
            CommonProperties.BitsPerSample
        };

        public MetaDataEditor()
        {
            this.PlaylistItems = Enumerable.Empty<PlaylistItem>();
            this.Tags = new ObservableCollection<MetaDataEntry>();
            this.Images = new ObservableCollection<MetaDataEntry>();
            this.Properties = new ObservableCollection<MetaDataEntry>();
        }

        public bool HasItems
        {
            get
            {
                return this.PlaylistItems != null && this.PlaylistItems.Any();
            }
        }

        protected virtual void OnHasItemsChanged()
        {
            if (this.HasItemsChanged != null)
            {
                this.HasItemsChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("HasItems");
        }

        public event EventHandler HasItemsChanged;

        private IEnumerable<PlaylistItem> PlaylistItems { get; set; }

        private ObservableCollection<MetaDataEntry> _Tags { get; set; }

        public ObservableCollection<MetaDataEntry> Tags
        {
            get
            {
                return this._Tags;
            }
            set
            {
                this._Tags = value;
                this.OnTagsChanged();
            }
        }

        protected virtual void OnTagsChanged()
        {
            if (this.TagsChanged != null)
            {
                this.TagsChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Tags");
        }

        public event EventHandler TagsChanged;

        private ObservableCollection<MetaDataEntry> _Images { get; set; }

        public ObservableCollection<MetaDataEntry> Images
        {
            get
            {
                return this._Images;
            }
            set
            {
                this._Images = value;
                this.OnImagesChanged();
            }
        }

        protected virtual void OnImagesChanged()
        {
            if (this.ImagesChanged != null)
            {
                this.ImagesChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Images");
        }

        public event EventHandler ImagesChanged;

        private ObservableCollection<MetaDataEntry> _Properties { get; set; }

        public ObservableCollection<MetaDataEntry> Properties
        {
            get
            {
                return this._Properties;
            }
            set
            {
                this.OnPropertiesChanging();
                this._Properties = value;
                this.OnPropertiesChanged();
            }
        }

        protected virtual void OnPropertiesChanging()
        {
            if (this.Properties != null)
            {
                foreach (var metaDataEntry in this.Properties)
                {
                    metaDataEntry.Dispose();
                }
            }
        }

        protected virtual void OnPropertiesChanged()
        {
            if (this.PropertiesChanged != null)
            {
                this.PropertiesChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Properties");
        }

        public event EventHandler PropertiesChanged;

        public IPlaylistManager PlaylistManager { get; private set; }

        public IMetaDataManager MetaDataManager { get; private set; }

        public IHierarchyManager HierarchyManager { get; private set; }

        public ISignalEmitter SignalEmitter { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.PlaylistManager = core.Managers.Playlist;
            this.MetaDataManager = core.Managers.MetaData;
            this.HierarchyManager = core.Managers.Hierarchy;
            this.SignalEmitter = core.Components.SignalEmitter;
            this.SignalEmitter.Signal += this.OnSignal;
            base.InitializeComponent(core);
        }

        protected virtual Task OnSignal(object sender, ISignal signal)
        {
            switch (signal.Name)
            {
                case CommonSignals.PluginInvocation:
                    var invocation = signal.State as IInvocationComponent;
                    if (invocation != null)
                    {
                        switch (invocation.Category)
                        {
                            case InvocationComponent.CATEGORY_PLAYLIST:
                                switch (invocation.Id)
                                {
                                    case MetaDataEditorBehaviour.EDIT_METADATA:
                                        return this.Refresh();
                                }
                                break;
                        }
                    }
                    break;
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        public Task Refresh()
        {
            if (this.PlaylistManager == null || this.PlaylistManager.SelectedItems == null)
            {
#if NET40
                return TaskEx.FromResult(false);
#else
                return Task.CompletedTask;
#endif
            }
            this.PlaylistItems = this.PlaylistManager.SelectedItems.ToArray();
            var items = this.GetItems();
            return Windows.Invoke(() =>
            {
                this.Tags = new ObservableCollection<MetaDataEntry>(items[MetaDataItemType.Tag]);
                this.Images = new ObservableCollection<MetaDataEntry>(items[MetaDataItemType.Image]);
                this.Properties = new ObservableCollection<MetaDataEntry>(items[MetaDataItemType.Property]);
                this.OnHasItemsChanged();
            });
        }

        protected IDictionary<MetaDataItemType, IEnumerable<MetaDataEntry>> GetItems()
        {
            var result = new Dictionary<MetaDataItemType, IEnumerable<MetaDataEntry>>();
            result[MetaDataItemType.Tag] = TAGS.Select(name => new MetaDataEntry(name, MetaDataItemType.Tag)).ToArray();
            result[MetaDataItemType.Image] = IMAGES.Select(name => new MetaDataEntry(name, MetaDataItemType.Image)).ToArray();
            result[MetaDataItemType.Property] = PROPERTIES.Select(name => new MetaDataEntry(name, MetaDataItemType.Property)).ToArray();
            foreach (var playlistItem in this.PlaylistItems)
            {
                foreach (var key in result.Keys)
                {
                    foreach (var metaDataEntry in result[key])
                    {
                        metaDataEntry.AddPlaylistItem(playlistItem);
                    }
                }
            }
            foreach (var key in result.Keys)
            {
                foreach (var metaDataEntry in result[key])
                {
                    //TODO: Can't access this.Core on another thread.
                    metaDataEntry.InitializeComponent(null);
                }
            }
            return result;
        }

        public ICommand SaveCommand
        {
            get
            {
                return CommandFactory.Instance.CreateCommand(this.Save);
            }
        }

        public async Task Save()
        {
            if (this.PlaylistItems == null)
            {
                return;
            }
            foreach (var tag in this.Tags)
            {
                tag.Save();
            }
            foreach (var image in this.Images)
            {
                image.Save();
            }
            await this.MetaDataManager.Save(this.PlaylistItems);
            await this.HierarchyManager.Build(LibraryItemStatus.Import);
            this.Cancel();
        }

        public ICommand CancelCommand
        {
            get
            {
                return new Command(this.Cancel);
            }
        }

        public void Cancel()
        {
            this.PlaylistItems = Enumerable.Empty<PlaylistItem>();
            this.OnHasItemsChanged();
        }

        protected override void OnDisposing()
        {
            base.OnDisposing();
        }

        protected override Freezable CreateInstanceCore()
        {
            return new MetaDataEditor();
        }
    }
}
