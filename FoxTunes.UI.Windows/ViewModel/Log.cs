using FoxTunes.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Data;

namespace FoxTunes.ViewModel
{
    public class Log : ViewModelBase
    {
        const int MESSAGE_CAPACITY = 100;

        public Log()
        {
            this.Messages = new ObservableCollection<LogMessage>();
            this.FilteredMessages = new CollectionViewSource();
            this.FilteredMessages.Source = this.Messages;
            this.FilteredMessages.Filter += this.OnFilter;
        }

        public ILogEmitter LogEmitter { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public BooleanConfigurationElement Element { get; private set; }

        public ObservableCollection<LogMessage> Messages { get; private set; }

        public CollectionViewSource FilteredMessages { get; private set; }

        public bool LogVisible
        {
            get
            {
                return this.Element != null && this.Element.Value;
            }
            set
            {
                if (this.Element == null)
                {
                    return;
                }
                this.Element.Value = value;
                this.OnLogVisibleChanged();
            }
        }

        protected virtual void OnLogVisibleChanged()
        {
            if (this.LogVisibleChanged != null)
            {
                this.LogVisibleChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("LogVisible");
        }

        public event EventHandler LogVisibleChanged = delegate { };

        private string _Filter { get; set; }

        public string Filter
        {
            get
            {
                return this._Filter;
            }
            set
            {
                this._Filter = value;
                this.OnFilterChanged();
            }
        }

        protected virtual void OnFilterChanged()
        {
            if (this.FilterChanged != null)
            {
                this.FilterChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Filter");
            this.FilteredMessages.View.Refresh();
        }

        public event EventHandler FilterChanged = delegate { };

        protected virtual void OnFilter(object sender, FilterEventArgs e)
        {
            if (string.IsNullOrEmpty(this.Filter))
            {
                e.Accepted = true;
                return;
            }
            var message = e.Item as LogMessage;
            e.Accepted =
                message.Name.Contains(this.Filter, true) ||
                Enum.GetName(typeof(LogLevel), message.Level).Contains(this.Filter, true) ||
                message.Message.Contains(this.Filter, true);
        }

        protected override void OnCoreChanged()
        {
            this.LogEmitter = this.Core.Components.LogEmitter;
            this.LogEmitter.LogMessage += this.OnLogEmitterLogMessage;
            this.Configuration = this.Core.Components.Configuration;
            this.Element = this.Configuration.GetElement<BooleanConfigurationElement>(
                WindowsUserInterfaceConfiguration.SYSTEM_SECTION,
                WindowsUserInterfaceConfiguration.LOGGING_ELEMENT
            );
            this.Element.ValueChanged += this.OnElementValueChanged;
            this.OnLogVisibleChanged();
            base.OnCoreChanged();
        }

        protected virtual void OnElementValueChanged(object sender, EventArgs e)
        {
            this.OnLogVisibleChanged();
        }

        protected virtual void OnLogEmitterLogMessage(object sender, LogMessageEventArgs e)
        {
            this.Messages.Insert(0, e.LogMessage);
            this.EnsureCapacity();
        }

        private void EnsureCapacity()
        {
            while (this.Messages.Count > MESSAGE_CAPACITY)
            {
                this.Messages.RemoveAt(this.Messages.Count - 1);
            }
        }

        protected override Freezable CreateInstanceCore()
        {
            return new Log();
        }
    }
}
