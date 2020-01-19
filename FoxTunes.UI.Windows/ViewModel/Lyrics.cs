using FoxTunes.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace FoxTunes.ViewModel
{
    public class Lyrics : ViewModelBase
    {
        public Lyrics()
        {

        }

        public IPlaylistManager PlaylistManager { get; private set; }

        public IConfiguration Configuration { get; private set; }

        private bool _HasData { get; set; }

        public bool HasData
        {
            get
            {
                return this._HasData;
            }
            set
            {
                this._HasData = value;
                this.OnHasDataChanged();
            }
        }

        protected virtual void OnHasDataChanged()
        {
            if (this.HasDataChanged != null)
            {
                this.HasDataChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("HasData");
        }

        public event EventHandler HasDataChanged;

        private string _Data { get; set; }

        public string Data
        {
            get
            {
                return this._Data;
            }
            set
            {
                this._Data = value;
                this.OnDataChanged();
            }
        }

        protected virtual void OnDataChanged()
        {
            if (this.DataChanged != null)
            {
                this.DataChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Data");
        }

        public event EventHandler DataChanged;

        public override void InitializeComponent(ICore core)
        {
            this.PlaylistManager = this.Core.Managers.Playlist;
            this.Configuration = this.Core.Components.Configuration;
            this.Configuration.GetElement<BooleanConfigurationElement>(
                MetaDataBehaviourConfiguration.SECTION,
                MetaDataBehaviourConfiguration.READ_LYRICS_TAGS
            ).ConnectValue(value =>
            {
                if (value)
                {
                    this.PlaylistManager.CurrentItemChanged += this.OnCurrentItemChanged;
                    var task = this.Refresh();
                }
                else
                {
                    this.PlaylistManager.CurrentItemChanged -= this.OnCurrentItemChanged;
                    if (this.HasData)
                    {
                        var task = Windows.Invoke(() =>
                        {
                            this.HasData = false;
                            this.Data = null;
                        });
                    }
                }
            });
            base.InitializeComponent(core);
        }

        protected virtual async void OnCurrentItemChanged(object sender, AsyncEventArgs e)
        {
            using (e.Defer())
            {
                await this.Refresh().ConfigureAwait(false);
            }
        }

        protected virtual Task Refresh()
        {
            var data = default(string);
            if (this.PlaylistManager.CurrentItem != null)
            {
                var metaDataItem = this.PlaylistManager.CurrentItem.MetaDatas.FirstOrDefault(
                    _metaDataItem => string.Equals(_metaDataItem.Name, CommonMetaData.Lyrics, StringComparison.OrdinalIgnoreCase)
                );
                if (metaDataItem != null)
                {
                    data = metaDataItem.Value;
                }
            }
            return Windows.Invoke(() =>
            {
                if (!string.IsNullOrEmpty(data))
                {
                    this.Data = data;
                    this.HasData = true;
                }
                else
                {
                    this.HasData = false;
                    this.Data = null;
                }
            });
        }

        protected override Freezable CreateInstanceCore()
        {
            return new Lyrics();
        }

        protected override void OnDisposing()
        {
            if (this.PlaylistManager != null)
            {
                this.PlaylistManager.CurrentItemChanged -= this.OnCurrentItemChanged;
            }
            base.OnDisposing();
        }
    }
}
