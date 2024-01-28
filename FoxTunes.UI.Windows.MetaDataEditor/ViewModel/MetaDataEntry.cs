using FoxTunes.Interfaces;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace FoxTunes.ViewModel
{
    public class MetaDataEntry : ViewModelBase
    {
        public static readonly string MyPictures = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

        public static readonly string NoValue = "<No Value>";

        public static readonly string MultipleValues = "<Multiple Values>";

        public static readonly ThemeLoader ThemeLoader = ComponentRegistry.Instance.GetComponent<ThemeLoader>();

        private MetaDataEntry()
        {
            this.PlaylistItems = new List<PlaylistItem>();
            this.MetaDataItems = new Dictionary<PlaylistItem, MetaDataItem>();
        }

        public MetaDataEntry(string name, MetaDataItemType type) : this()
        {
            this.Name = name;
            this.Type = type;
        }

        private IList<PlaylistItem> PlaylistItems { get; set; }

        private IDictionary<PlaylistItem, MetaDataItem> MetaDataItems { get; set; }

        private string _Name { get; set; }

        public string Name
        {
            get
            {
                return this._Name;
            }
            set
            {
                this._Name = value;
                this.OnNameChanged();
            }
        }

        protected virtual void OnNameChanged()
        {
            if (this.NameChanged != null)
            {
                this.NameChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Name");
        }

        public event EventHandler NameChanged;

        private MetaDataItemType _Type { get; set; }

        public MetaDataItemType Type
        {
            get
            {
                return this._Type;
            }
            set
            {
                this._Type = value;
                this.OnTypeChanged();
            }
        }

        protected virtual void OnTypeChanged()
        {
            if (this.TypeChanged != null)
            {
                this.TypeChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Type");
        }

        public event EventHandler TypeChanged;

        public string OriginalValue { get; protected set; }

        private string _Value { get; set; }

        public string Value
        {
            get
            {
                return this._Value;
            }
            set
            {
                this._Value = value;
                this.OnValueChanged();
            }
        }

        protected virtual void OnValueChanged()
        {
            if (string.Equals(this.Value, NoValue, StringComparison.OrdinalIgnoreCase))
            {
                this.HasValue = false;
            }
            else
            {
                this.HasValue = true;
            }
            if (string.Equals(this.Value, MultipleValues, StringComparison.OrdinalIgnoreCase))
            {
                this.HasMultipleValues = true;
            }
            else
            {
                this.HasMultipleValues = false;
            }
            this.OnHasChangesChanged();
            if (this.ValueChanged != null)
            {
                this.ValueChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Value");
        }

        public event EventHandler ValueChanged;

        public bool HasChanges
        {
            get
            {
                return !string.Equals(this.Value, this.OriginalValue);
            }
        }

        protected virtual void OnHasChangesChanged()
        {
            if (this.HasChangesChanged != null)
            {
                this.HasChangesChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("HasChanges");
        }

        public event EventHandler HasChangesChanged;

        private ImageSource _ImageSource { get; set; }

        public ImageSource ImageSource
        {
            get
            {
                return this._ImageSource;
            }
            set
            {
                this._ImageSource = value;
                this.OnImageSourceChanged();
            }
        }

        protected virtual void OnImageSourceChanged()
        {
            if (this.ImageSourceChanged != null)
            {
                this.ImageSourceChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("ImageSource");
        }

        public event EventHandler ImageSourceChanged;

        public int DecodePixelWidth
        {
            get
            {
                return 0;
            }
        }

        public int DecodePixelHeight
        {
            get
            {
                return 0;
            }
        }

        private bool _HasValue { get; set; }

        public bool HasValue
        {
            get
            {
                return this._HasValue;
            }
            set
            {
                this._HasValue = value;
                this.OnHasValueChanged();
            }
        }

        protected virtual void OnHasValueChanged()
        {
            if (this.HasValueChanged != null)
            {
                this.HasValueChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("HasValue");
        }

        public event EventHandler HasValueChanged;

        private bool _HasMultipleValues { get; set; }

        public bool HasMultipleValues
        {
            get
            {
                return this._HasMultipleValues;
            }
            set
            {
                this._HasMultipleValues = value;
                this.OnHasMultipleValuesChanged();
            }
        }

        protected virtual void OnHasMultipleValuesChanged()
        {
            if (this.HasMultipleValuesChanged != null)
            {
                this.HasMultipleValuesChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("HasMultipleValues");
        }

        public event EventHandler HasMultipleValuesChanged;

        public override void InitializeComponent(ICore core)
        {
            foreach (var playlistItem in this.PlaylistItems)
            {
                var metaDataItem = default(MetaDataItem);
                if (!this.MetaDataItems.TryGetValue(playlistItem, out metaDataItem) || metaDataItem == null)
                {
                    if (!this.HasValue)
                    {
                        this.Value = NoValue;
                    }
                    else
                    {
                        this.Value = MultipleValues;
                        break;
                    }
                }
                else if (!this.HasValue)
                {
                    this.Value = metaDataItem.Value;
                }
                else if (!string.Equals(this.Value, metaDataItem.Value))
                {
                    this.Value = MultipleValues;
                    break;
                }
            }
            this.OriginalValue = this.Value;
            switch (this.Type)
            {
                case MetaDataItemType.Image:
                    this.RefreshImageSource();
                    break;
            }
            base.InitializeComponent(core);
        }

        protected virtual void RefreshImageSource()
        {
            if (!this.HasValue || this.HasMultipleValues)
            {
                //TODO: The <No Value>/<Multiple Values> text is hard to read when we use the placeholder image.
                //using (var stream = ThemeLoader.Theme.ArtworkPlaceholder)
                //{
                //    this.ImageSource = ImageLoader.Load(stream, this.DecodePixelWidth, this.DecodePixelHeight);
                //}
                this.ImageSource = null;
            }
            else
            {
                this.ImageSource = ImageLoader.Load((string)this.Value, this.DecodePixelWidth, this.DecodePixelHeight);
            }
        }

        public void AddPlaylistItem(PlaylistItem playlistItem)
        {
            this.AddPlaylistItem(playlistItem, playlistItem.MetaDatas.FirstOrDefault(metaDataItem => string.Equals(metaDataItem.Name, this.Name, StringComparison.OrdinalIgnoreCase)));
        }

        public void AddPlaylistItem(PlaylistItem playlistItem, MetaDataItem metaDataItem)
        {
            this.PlaylistItems.Add(playlistItem);
            if (metaDataItem != null)
            {
                this.MetaDataItems.Add(playlistItem, metaDataItem);
            }
        }

        public void Save()
        {
            if (!this.HasChanges || this.HasMultipleValues)
            {
                return;
            }
            foreach (var playlistItem in this.PlaylistItems)
            {
                var metaDataItem = default(MetaDataItem);
                if (!this.MetaDataItems.TryGetValue(playlistItem, out metaDataItem))
                {
                    metaDataItem = new MetaDataItem(this.Name, this.Type);
                    playlistItem.MetaDatas.Add(metaDataItem);
                    this.MetaDataItems.Add(playlistItem, metaDataItem);
                }
                if (this.HasValue)
                {
                    metaDataItem.Value = this.Value;
                }
                else
                {
                    metaDataItem.Value = null;
                }
            }
        }

        public ICommand BrowseCommand
        {
            get
            {
                return new Command(this.Browse, () => this.CanBrowse);
            }
        }

        public bool CanBrowse
        {
            get
            {
                return this.PlaylistItems.Count > 0;
            }
        }

        public void Browse()
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "Images (*.jpg, *.jpeg, *.png, *.bmp, *.bin) | *.jpg; *.jpeg; *.png; *.bmp; *.bin;";
            var directoryName = this.PlaylistItems.Select(playlistItem => playlistItem.DirectoryName).FirstOrDefault();
            if (string.IsNullOrEmpty(directoryName))
            {
                directoryName = MyPictures;
            }
            dialog.InitialDirectory = directoryName;
            if (dialog.ShowDialog() != true)
            {
                return;
            }
            this.Value = dialog.FileName;
            switch (this.Type)
            {
                case MetaDataItemType.Image:
                    this.RefreshImageSource();
                    break;
            }
        }

        public ICommand ClearCommand
        {
            get
            {
                return new Command(this.Clear, () => this.CanClear);
            }
        }

        public bool CanClear
        {
            get
            {
                return this.PlaylistItems.Count > 0 && this.HasValue;
            }
        }

        public void Clear()
        {
            this.Value = NoValue;
            switch (this.Type)
            {
                case MetaDataItemType.Image:
                    this.RefreshImageSource();
                    break;
            }
        }

        protected override Freezable CreateInstanceCore()
        {
            return new MetaDataEntry();
        }
    }
}
