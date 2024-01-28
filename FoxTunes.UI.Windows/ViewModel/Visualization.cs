using FoxTunes.Interfaces;
using System;
using System.Windows;

namespace FoxTunes.ViewModel
{
    public class Visualization : ViewModelBase
    {
        public IOutput Output { get; private set; }

        private string _StatusMessage { get; set; }

        public string StatusMessage
        {
            get
            {
                return this._StatusMessage;
            }
            set
            {
                this._StatusMessage = value;
                this.OnStatusMessageChanged();
            }
        }

        protected virtual void OnStatusMessageChanged()
        {
            if (this.StatusMessageChanged != null)
            {
                this.StatusMessageChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("StatusMessage");
        }

        public event EventHandler StatusMessageChanged;

        private string _StatusMessageDetail { get; set; }

        public string StatusMessageDetail
        {
            get
            {
                return this._StatusMessageDetail;
            }
            set
            {
                this._StatusMessageDetail = value;
                this.OnStatusMessageDetailChanged();
            }
        }

        protected virtual void OnStatusMessageDetailChanged()
        {
            if (this.StatusMessageDetailChanged != null)
            {
                this.StatusMessageDetailChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("StatusMessageDetail");
        }

        public event EventHandler StatusMessageDetailChanged;

        private bool _HasStatusMessage { get; set; }

        public bool HasStatusMessage
        {
            get
            {
                return this._HasStatusMessage;
            }
            set
            {
                this._HasStatusMessage = value;
                this.OnHasStatusMessageChanged();
            }
        }

        protected virtual void OnHasStatusMessageChanged()
        {
            if (this.HasStatusMessageChanged != null)
            {
                this.HasStatusMessageChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("HasStatusMessage");
        }

        public event EventHandler HasStatusMessageChanged;

        protected override void InitializeComponent(ICore core)
        {
            this.Output = core.Components.Output;
            this.Output.CanGetDataChanged += this.OnCanGetDataChanged;
            this.Update();
            base.InitializeComponent(core);
        }

        protected virtual void OnCanGetDataChanged(object sender, EventArgs e)
        {
            var task = Windows.Invoke(this.Update);
        }

        protected virtual void Update()
        {
            if (this.Output.IsStarted && !this.Output.CanGetData)
            {
                this.StatusMessage = Strings.Visualization_Unavailable;
                this.StatusMessageDetail = Strings.Visualization_UnavailableDetail;
                this.HasStatusMessage = true;
            }
            else
            {
                this.StatusMessage = null;
                this.StatusMessageDetail = null;
                this.HasStatusMessage = false;
            }
        }

        protected override Freezable CreateInstanceCore()
        {
            return new Visualization();
        }

        protected override void OnDisposing()
        {
            if (this.Output != null)
            {
                this.Output.IsStartedChanged -= this.OnCanGetDataChanged;
            }
            base.OnDisposing();
        }
    }
}
