using FoxTunes.Interfaces;
using System;
using System.Collections.ObjectModel;

namespace FoxTunes
{
    public class PlaylistItem : PersistableComponent, IMetaDataSource, IFileData
    {
        public PlaylistItem()
        {

        }

        public ICore Core { get; private set; }

        public IDatabase Database { get; private set; }

        public int Sequence { get; set; }

        public string DirectoryName { get; set; }

        public string FileName { get; set; }

        public PlaylistItemStatus Status { get; set; }

        private ObservableCollection<MetaDataItem> _MetaDatas { get; set; }

        public ObservableCollection<MetaDataItem> MetaDatas
        {
            get
            {
                if (this._MetaDatas == null)
                {
                    this.LoadMetaDatas();
                }
                return this._MetaDatas;
            }
            set
            {
                this._MetaDatas = value;
                this.OnMetaDatasChanged();
            }
        }

        protected virtual void OnMetaDatasChanged()
        {
            if (this.MetaDatasChanged != null)
            {
                this.MetaDatasChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("MetaDatas");
        }

        public event EventHandler MetaDatasChanged = delegate { };

        public override void InitializeComponent(ICore core)
        {
            this.Core = core;
            this.Database = core.Components.Database;
            base.InitializeComponent(core);
        }

        public void LoadMetaDatas()
        {
            this.MetaDatas = new ObservableCollection<MetaDataItem>(MetaDataInfo.GetMetaData(this.Core, this.Database, this, MetaDataItemType.All));
        }

        public override bool Equals(IPersistableComponent other)
        {
            if (other is PlaylistItem)
            {
                return base.Equals(other) && string.Equals(this.FileName, (other as PlaylistItem).FileName, StringComparison.OrdinalIgnoreCase);
            }
            return base.Equals(other);
        }

    }

    public enum PlaylistItemStatus : byte
    {
        None = 0,
        Import = 1,
        Update = 2
    }
}
