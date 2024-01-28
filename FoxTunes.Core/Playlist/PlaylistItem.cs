using FoxDb;
using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class PlaylistItem : PersistableComponent, IMetaDataSource, IFileData
    {
        public PlaylistItem()
        {

        }

        public int Sequence { get; set; }

        public string DirectoryName { get; set; }

        public string FileName { get; set; }

        public PlaylistItemStatus Status { get; set; }

        [Relation(Flags = RelationFlags.AutoExpression | RelationFlags.EagerFetch | RelationFlags.ManyToMany)]
        public ObservableCollection<MetaDataItem> MetaDatas { get; set; }

        protected virtual void OnMetaDatasChanged()
        {
            if (this.MetaDatasChanged != null)
            {
                this.MetaDatasChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("MetaDatas");
        }

        public event EventHandler MetaDatasChanged = delegate { };

        public Task<IEnumerable<MetaDataItem>> GetMetaData(string fileName)
        {
#if NET40
            return TaskEx.FromResult<IEnumerable<MetaDataItem>>(this.MetaDatas);
#else
            return Task.FromResult<IEnumerable<MetaDataItem>>(this.MetaDatas);
#endif
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
        Update = 2,
        Remove = 3
    }
}
