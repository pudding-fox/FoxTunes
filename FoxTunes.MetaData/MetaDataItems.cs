using FoxTunes.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoxTunes
{
    public abstract class MetaDataItems : BaseComponent, IMetaDataItems
    {
        protected MetaDataItems()
        {
            this.Items = new ObservableCollection<IMetaDataItem>();
            this.Items.CollectionChanged += (sender, e) => this.CollectionChanged(this, e);
        }

        private ObservableCollection<IMetaDataItem> Items { get; set; }

        public void Add(IMetaDataItem item)
        {
            this.Items.Add(item);
        }

        public void Clear()
        {
            this.Items.Clear();
        }

        public bool Contains(IMetaDataItem item)
        {
            return this.Items.Contains(item);
        }

        public void CopyTo(IMetaDataItem[] array, int arrayIndex)
        {
            this.Items.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get
            {
                return this.Items.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public bool Remove(IMetaDataItem item)
        {
            return this.Items.Remove(item);
        }

        public int IndexOf(IMetaDataItem item)
        {
            return this.Items.IndexOf(item);
        }

        public IMetaDataItem this[int index]
        {
            get
            {
                return this.Items[index];
            }
        }

        public IMetaDataItem this[string name]
        {
            get
            {
                return this.Items.FirstOrDefault(item => string.Equals(item.Name, name, StringComparison.OrdinalIgnoreCase));
            }
        }

        public IEnumerator<IMetaDataItem> GetEnumerator()
        {
            return this.Items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.Items.GetEnumerator();
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged = delegate { };
    }
}
