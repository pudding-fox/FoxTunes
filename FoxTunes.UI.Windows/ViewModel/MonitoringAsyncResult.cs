using FoxTunes.Interfaces;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;

namespace FoxTunes.ViewModel
{
    public class MonitoringAsyncResult<T> : ViewModelBase, IAsyncResult<T> where T : class
    {
        private MonitoringAsyncResult()
        {

        }

        public MonitoringAsyncResult(IObservable source, Func<Task<T>> factory) : this()
        {
            this.Source = source;
            this.Source.PropertyChanged += this.OnSourceChanged;
            this.Factory = factory;
            this.Dispatch(this.Run);
        }

        public MonitoringAsyncResult(IObservable source, T value, Func<Task<T>> factory) : this()
        {
            this.Source = source;
            this.Source.PropertyChanged += this.OnSourceChanged;
            this.Value = value;
            this.Factory = factory;
            this.Dispatch(this.Run);
        }

        public IObservable Source { get; private set; }

        public Func<Task<T>> Factory { get; private set; }

        protected virtual void OnSourceChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.PropertyName))
            {
                return;
            }
            this.Dispatch(this.Run);
        }

        private T _Value { get; set; }

        public T Value
        {
            get
            {
                return this._Value;
            }
            set
            {
                if (object.ReferenceEquals(this.Value, value))
                {
                    return;
                }
                this._Value = value;
                this.OnValueChanged();
            }
        }

        protected virtual void OnValueChanged()
        {
            if (this.ValueChanged != null)
            {
                this.ValueChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Value");
        }

        public event EventHandler ValueChanged;

        public async Task Run()
        {
            var task = this.Factory();
            var value = await task.ConfigureAwait(false);
            if (value == null)
            {
                return;
            }
            this.Value = value;
        }

        protected override void OnDisposing()
        {
            if (this.Source != null)
            {
                this.Source.PropertyChanged -= this.OnSourceChanged;
            }
            base.OnDisposing();
        }

        protected override Freezable CreateInstanceCore()
        {
            return new MonitoringAsyncResult<T>();
        }
    }
}
