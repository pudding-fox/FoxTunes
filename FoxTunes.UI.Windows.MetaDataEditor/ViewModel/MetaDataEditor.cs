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

        public MetaDataEditor()
        {
            this.Tags = new ObservableCollection<MetaDataEntry>();
            this.Images = new ObservableCollection<MetaDataEntry>();
        }

        public bool HasItems
        {
            get
            {
                return this.Tags.Any() || this.Images.Any();
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

        private int _Count { get; set; }

        public int Count
        {
            get
            {
                return this._Count;
            }
            set
            {
                this._Count = value;
                this.OnCountChanged();
            }
        }

        protected virtual void OnCountChanged()
        {
            if (this.CountChanged != null)
            {
                this.CountChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Count");
        }

        public event EventHandler CountChanged;

        public IPlaylistManager PlaylistManager { get; private set; }

        public ILibraryManager LibraryManager { get; private set; }

        public IMetaDataManager MetaDataManager { get; private set; }

        public IHierarchyManager HierarchyManager { get; private set; }

        public ILibraryHierarchyBrowser LibraryHierarchyBrowser { get; private set; }

        public ISignalEmitter SignalEmitter { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.PlaylistManager = core.Managers.Playlist;
            this.LibraryManager = core.Managers.Library;
            this.MetaDataManager = core.Managers.MetaData;
            this.HierarchyManager = core.Managers.Hierarchy;
            this.LibraryHierarchyBrowser = core.Components.LibraryHierarchyBrowser;
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
                        switch (invocation.Id)
                        {
                            case MetaDataEditorBehaviour.EDIT_METADATA:
                                switch (invocation.Category)
                                {
                                    case InvocationComponent.CATEGORY_LIBRARY:
                                        return this.EditLibrary();
                                    case InvocationComponent.CATEGORY_PLAYLIST:
                                        return this.EditPlaylist();
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

        public Task EditLibrary()
        {
            if (this.LibraryManager == null || this.LibraryManager.SelectedItem == null)
            {
#if NET40
                return TaskEx.FromResult(false);
#else
                return Task.CompletedTask;
#endif
            }
            //TODO: Warning: Buffering a potentially large sequence. It might be better to run the query multiple times.
            var libraryItems = this.LibraryHierarchyBrowser.GetItems(
                this.LibraryManager.SelectedItem,
                true
            ).ToArray();
            return this.SetItems(
                this.GetItems(libraryItems),
                libraryItems.Length
            );
        }

        public Task EditPlaylist()
        {
            if (this.PlaylistManager == null || this.PlaylistManager.SelectedItems == null)
            {
#if NET40
                return TaskEx.FromResult(false);
#else
                return Task.CompletedTask;
#endif
            }
            //TODO: If the playlist contains duplicate tracks, will all be refreshed properly?
            var playlistItems = this.PlaylistManager.SelectedItems
                .GroupBy(playlistItem => playlistItem.FileName)
                .Select(group => group.First())
                .ToArray();
            if (!playlistItems.Any())
            {
#if NET40
                return TaskEx.FromResult(false);
#else
                return Task.CompletedTask;
#endif
            }
            return this.SetItems(
                this.GetItems(playlistItems),
                playlistItems.Length
            );
        }

        protected IDictionary<MetaDataItemType, IEnumerable<MetaDataEntry>> GetItems(IEnumerable<IFileData> sources)
        {
            var result = new Dictionary<MetaDataItemType, IEnumerable<MetaDataEntry>>();
            result[MetaDataItemType.Tag] = TAGS.Select(name => new MetaDataEntry(name, MetaDataItemType.Tag)).ToArray();
            result[MetaDataItemType.Image] = IMAGES.Select(name => new MetaDataEntry(name, MetaDataItemType.Image)).ToArray();
            foreach (var key in result.Keys)
            {
                foreach (var metaDataEntry in result[key])
                {
                    metaDataEntry.SetSources(sources);
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

        protected virtual Task SetItems(IDictionary<MetaDataItemType, IEnumerable<MetaDataEntry>> items, int count)
        {
            return Windows.Invoke(() =>
            {
                this.Tags = new ObservableCollection<MetaDataEntry>(items[MetaDataItemType.Tag]);
                this.Images = new ObservableCollection<MetaDataEntry>(items[MetaDataItemType.Image]);
                this.Count = count;
                this.OnHasItemsChanged();
            });
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
            var sources = new HashSet<IFileData>();
            if (this.Tags != null)
            {
                foreach (var tag in this.Tags)
                {
                    tag.Save();
                    sources.AddRange(tag.GetSources());
                }
            }
            if (this.Images != null)
            {
                foreach (var image in this.Images)
                {
                    image.Save();
                    sources.AddRange(image.GetSources());
                }
            }
            var libraryItems = sources.OfType<LibraryItem>().ToArray();
            if (libraryItems.Any())
            {
                await this.MetaDataManager.Save(libraryItems).ConfigureAwait(false);
            }
            var playlistItems = sources.OfType<PlaylistItem>().ToArray();
            if (playlistItems.Any())
            {
                await this.MetaDataManager.Save(playlistItems).ConfigureAwait(false);
            }
            await this.HierarchyManager.Clear(LibraryItemStatus.Import).ConfigureAwait(false);
            await this.HierarchyManager.Build(LibraryItemStatus.Import).ConfigureAwait(false);
            await this.LibraryManager.Set(LibraryItemStatus.None).ConfigureAwait(false);
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
            this.Tags = new ObservableCollection<MetaDataEntry>();
            this.Images = new ObservableCollection<MetaDataEntry>();
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
