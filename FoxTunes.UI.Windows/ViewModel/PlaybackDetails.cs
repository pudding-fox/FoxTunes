using FoxTunes.Interfaces;
using System;
using System.Text;
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
                if (string.Equals(this._Description, value, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }
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

        public IOutput Output { get; private set; }

        protected override void InitializeComponent(ICore core)
        {
            PlaybackStateNotifier.Notify += this.OnNotify;
            this.Output = core.Components.Output;
            base.InitializeComponent(core);
        }

        protected virtual void OnNotify(object sender, EventArgs e)
        {
            try
            {
                this.Refresh();
            }
            catch
            {
                //Nothing can be done, never throw on background thread.
            }
        }

        protected virtual void Refresh()
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
            this.Description = builder.ToString();
        }

        protected override Freezable CreateInstanceCore()
        {
            return new PlaybackDetails();
        }

        protected override void OnDisposing()
        {
            PlaybackStateNotifier.Notify -= this.OnNotify;
            base.OnDisposing();
        }
    }
}
