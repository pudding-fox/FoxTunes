using FoxTunes.Interfaces;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace FoxTunes.ViewModel
{
    public class OutputStream : ViewModelBase
    {
        private OutputStream()
        {
            PlaybackStateNotifier.Notify += this.OnNotify;
        }

        public OutputStream(IOutputStream outputStream) : this()
        {
            this.InnerOutputStream = outputStream;
        }

        public IOutputStream InnerOutputStream { get; private set; }

        protected virtual void OnNotify(object sender, EventArgs e)
        {
            try
            {
                this.OnPositionChanged();
                this.OnPositionDescriptionChanged();
                this.OnRemainingChanged();
                this.OnRemainingDescriptionChanged();
                this.OnDescriptionChanged();
            }
            catch
            {
                //Nothing can be done, never throw on background thread.
            }
        }

        private long _Position { get; set; }

        public long Position
        {
            get
            {
                if (this.IsSeeking)
                {
                    return this._Position;
                }
                return this.InnerOutputStream.Position;
            }
            set
            {
                if (this.IsSeeking)
                {
                    this._Position = value;
                }
                else
                {
                    this.InnerOutputStream.Position = value;
                }
                this.OnPositionChanged();
            }
        }

        protected virtual void OnPositionChanged()
        {
            if (this.PositionChanged != null)
            {
                this.PositionChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Position");
        }

        public event EventHandler PositionChanged;

        public string PositionDescription
        {
            get
            {
                return this.InnerOutputStream.GetDuration(this.Position).ToString(@"mm\:ss");
            }
        }

        protected virtual void OnPositionDescriptionChanged()
        {
            if (this.PositionDescriptionChanged != null)
            {
                this.PositionDescriptionChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("PositionDescription");
        }

        public event EventHandler PositionDescriptionChanged;

        public long Remaining
        {
            get
            {
                return this.Length - this.Position;
            }
        }

        protected virtual void OnRemainingChanged()
        {
            if (this.RemainingChanged != null)
            {
                this.RemainingChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Remaining");
        }

        public event EventHandler RemainingChanged;

        public string RemainingDescription
        {
            get
            {
                return this.InnerOutputStream.GetDuration(this.Remaining).ToString(@"mm\:ss");
            }
        }

        protected virtual void OnRemainingDescriptionChanged()
        {
            if (this.RemainingDescriptionChanged != null)
            {
                this.RemainingDescriptionChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("RemainingDescription");
        }

        public event EventHandler RemainingDescriptionChanged;

        public long Length
        {
            get
            {
                return this.InnerOutputStream.Length;
            }
        }

        public string LengthDescription
        {
            get
            {
                return this.InnerOutputStream.GetDuration(this.Length).ToString(@"mm\:ss");
            }
        }

        public string Description
        {
            get
            {
                return string.Format("{0}/{1}", this.PositionDescription, this.LengthDescription);
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

        private bool _IsSeeking { get; set; }

        public bool IsSeeking
        {
            get
            {
                return this._IsSeeking;
            }
            set
            {
                this._IsSeeking = value;
                this.OnIsSeekingChanged();
            }
        }

        protected virtual void OnIsSeekingChanged()
        {
            if (this.IsSeekingChanged != null)
            {
                this.IsSeekingChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("IsSeeking");
        }

        public event EventHandler IsSeekingChanged;

        public Task BeginSeek()
        {
            var position = this.Position;
            this.IsSeeking = true;
            this.Position = position;
            return this.InnerOutputStream.BeginSeek();
        }

        public Task EndSeek()
        {
            var position = this.Position;
            this.IsSeeking = false;
            this.Position = position;
            return this.InnerOutputStream.EndSeek();
        }

        protected override void OnDisposing()
        {
            PlaybackStateNotifier.Notify -= this.OnNotify;
            base.OnDisposing();
        }

        protected override Freezable CreateInstanceCore()
        {
            return new OutputStream(null);
        }
    }
}
