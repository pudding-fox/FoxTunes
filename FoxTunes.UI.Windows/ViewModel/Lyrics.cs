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

        public IPlaybackManager PlaybackManager { get; private set; }

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
            this.PlaybackManager = this.Core.Managers.Playback;
            this.Configuration = this.Core.Components.Configuration;
            this.Configuration.GetElement<BooleanConfigurationElement>(
                MetaDataBehaviourConfiguration.SECTION,
                MetaDataBehaviourConfiguration.READ_LYRICS_TAGS
            ).ConnectValue(value =>
            {
                if (value)
                {
                    this.PlaybackManager.CurrentStreamChanged += this.OnCurrentStreamChanged;
                    this.Dispatch(this.Refresh);
                }
                else
                {
                    this.PlaybackManager.CurrentStreamChanged -= this.OnCurrentStreamChanged;
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

        protected virtual void OnCurrentStreamChanged(object sender, AsyncEventArgs e)
        {
            this.Dispatch(this.Refresh);
        }

        protected virtual Task Refresh()
        {
            var data = default(string);
            var outputStream = this.PlaybackManager.CurrentStream;
            if (outputStream != null)
            {
                var metaDataItem = default(MetaDataItem);
                lock (outputStream.PlaylistItem.MetaDatas)
                {
                    metaDataItem = outputStream.PlaylistItem.MetaDatas.FirstOrDefault(
                        element => string.Equals(element.Name, CommonMetaData.Lyrics, StringComparison.OrdinalIgnoreCase)
                    );
                }
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
            if (this.PlaybackManager != null)
            {
                this.PlaybackManager.CurrentStreamChanged -= this.OnCurrentStreamChanged;
            }
            base.OnDisposing();
        }
    }
}
