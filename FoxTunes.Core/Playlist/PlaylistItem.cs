using FoxTunes.Interfaces;
using System;
using System.Collections.ObjectModel;

namespace FoxTunes
{
    public class PlaylistItem : PersistableComponent, IMetaDataSource
    {
        public PlaylistItem()
        {
            this.MetaDatas = new ObservableCollection<MetaDataItem>();
            this.Properties = new ObservableCollection<PropertyItem>();
            this.Images = new ObservableCollection<ImageItem>();
        }

        public PlaylistItem(int sequence, string fileName, IMetaDataSource metaData) : this()
        {
            this.Sequence = sequence;
            this.FileName = fileName;
            this.MetaDatas = metaData.MetaDatas;
            this.Properties = metaData.Properties;
            this.Images = metaData.Images;
        }

        private int _Sequence { get; set; }

        public int Sequence
        {
            get
            {
                return this._Sequence;
            }
            set
            {
                this._Sequence = value;
                this.OnSequenceChanged();
            }
        }

        protected virtual void OnSequenceChanged()
        {
            if (this.SequenceChanged != null)
            {
                this.SequenceChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Sequence");
        }

        public event EventHandler SequenceChanged = delegate { };

        public string FileName { get; set; }

        public ObservableCollection<MetaDataItem> MetaDatas { get; private set; }

        public ObservableCollection<PropertyItem> Properties { get; private set; }

        public ObservableCollection<ImageItem> Images { get; private set; }

        public bool Equals(PlaylistItem other)
        {
            if (other == null)
            {
                return false;
            }
            if (object.ReferenceEquals(this, other))
            {
                return true;
            }
            return this.Id == other.Id && string.Equals(this.FileName, other.FileName, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as PlaylistItem);
        }

        public override int GetHashCode()
        {
            return this.FileName.GetHashCode();
        }

        public static bool operator ==(PlaylistItem a, PlaylistItem b)
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

        public static bool operator !=(PlaylistItem a, PlaylistItem b)
        {
            return !(a == b);
        }
    }
}
