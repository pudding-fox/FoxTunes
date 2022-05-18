using FoxTunes.Interfaces;
using System;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FoxTunes.ViewModel
{
    public class PlaybackDetails : ViewModelBase
    {
        private string _Description { get; set; }

        public string Description
        {
            get
            {
                return this._Description;
            }
            set
            {
                this._Description = value;
                this.OnDescriptionChanged();
            }
        }

        protected virtual void OnDescriptionChanged()
        {
            if (this.DescriptionChanged != null)
            {
                this.DescriptionChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Description");
        }

        public event EventHandler DescriptionChanged;

        public IPlaybackManager PlaybackManager { get; private set; }

        public IOutput Output { get; private set; }

        protected override void InitializeComponent(ICore core)
        {
            this.PlaybackManager = core.Managers.Playback;
            this.PlaybackManager.CurrentStreamChanged += this.OnCurrentStreamChanged;
            this.Output = core.Components.Output;
            this.Dispatch(this.Refresh);
            base.InitializeComponent(core);
        }

        protected virtual void OnCurrentStreamChanged(object sender, EventArgs e)
        {
            //Critical: Don't block in this event handler, it causes a deadlock.
            this.Dispatch(this.Refresh);
        }

        protected virtual Task Refresh()
        {
            var builder = new StringBuilder();
            builder.AppendLine(string.Format("{0} {1}", Publication.Product, Publication.Version));
            if (this.Output != null)
            {
                builder.Append(this.Output.Description);
            }
            else
            {
                builder.Append(Strings.PlaybackDetails_NoOutput);
            }
            return Windows.Invoke(() =>
            {
                this.Description = builder.ToString();
            });
        }

        protected override Freezable CreateInstanceCore()
        {
            return new PlaybackDetails();
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
