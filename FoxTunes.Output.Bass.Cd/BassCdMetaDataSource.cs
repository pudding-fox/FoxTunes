using FoxTunes.Interfaces;
using System.Collections.ObjectModel;

namespace FoxTunes
{
    public class BassCdMetaDataSource : BaseComponent, IMetaDataSource
    {
        private BassCdMetaDataSource()
        {
            this.MetaDatas = new ObservableCollection<MetaDataItem>();
        }

        public BassCdMetaDataSource(string fileName, IBassCdMetaDataSourceStrategy strategy) : this()
        {
            this.FileName = fileName;
            this.Strategy = strategy;
        }

        public string FileName { get; private set; }

        public IBassCdMetaDataSourceStrategy Strategy { get; private set; }

        public ObservableCollection<MetaDataItem> MetaDatas { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            var drive = default(int);
            var track = default(int);
            if (!BassCdStreamProvider.ParseUrl(this.FileName, out drive, out track))
            {
                //TODO: Warn.
            }
            else
            {
                this.MetaDatas.AddRange(this.Strategy.GetMetaDatas(track));
                this.MetaDatas.AddRange(this.Strategy.GetProperties(track));
            }
            base.InitializeComponent(core);
        }
    }
}
