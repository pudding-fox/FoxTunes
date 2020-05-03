using System;
using System.Threading.Tasks;
using System.Windows;

namespace FoxTunes.ViewModel
{
    public class AsyncResult<T> : ViewModelBase
    {
        private AsyncResult()
        {

        }

        private AsyncResult(T value)
        {
            this.Value = value;
        }

        public AsyncResult(Task<T> task) : this()
        {
            this.Task = task;
            this.Dispatch(this.Run);
        }

        public Task<T> Task { get; private set; }

        private T _Value { get; set; }

        public T Value
        {
            get
            {
                return this._Value;
            }
            set
            {
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
            this.Value = await this.Task.ConfigureAwait(false);
        }

        protected override Freezable CreateInstanceCore()
        {
            return new AsyncResult<T>();
        }

        public static AsyncResult<T> FromValue(T value)
        {
            return new AsyncResult<T>(value);
        }
    }
}
