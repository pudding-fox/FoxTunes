using FoxTunes.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FoxTunes
{
    public abstract class PopulatorBase : BaseComponent, IReportsProgress, IDisposable
    {
        private PopulatorBase()
        {
#if NET40
            this.Semaphore = new AsyncSemaphore(1);
#else
            this.Semaphore = new SemaphoreSlim(1, 1);
#endif
        }

        public PopulatorBase(bool reportProgress) : this()
        {
            this.ReportProgress = reportProgress;
        }

#if NET40
        public AsyncSemaphore Semaphore { get; private set; }
#else
        public SemaphoreSlim Semaphore { get; private set; }
#endif

        public bool ReportProgress { get; private set; }

        private string _Name { get; set; }

        public string Name
        {
            get
            {
                return this._Name;
            }
        }

        protected Task SetName(string value)
        {
            this._Name = value;
            return this.OnNameChanged();
        }

        protected virtual async Task OnNameChanged()
        {
            if (this.NameChanged != null)
            {
                var e = new AsyncEventArgs();
                this.NameChanged(this, e);
                await e.Complete();
            }
            this.OnPropertyChanged("Name");
        }

        public event AsyncEventHandler NameChanged = delegate { };

        private string _Description { get; set; }

        public string Description
        {
            get
            {
                return this._Description;
            }
        }

        protected Task SetDescription(string value)
        {
            this._Description = value;
            return this.OnDescriptionChanged();
        }

        protected virtual async Task OnDescriptionChanged()
        {
            if (this.DescriptionChanged != null)
            {
                var e = new AsyncEventArgs();
                this.DescriptionChanged(this, e);
                await e.Complete();
            }
            this.OnPropertyChanged("Description");
        }

        public event AsyncEventHandler DescriptionChanged = delegate { };

        private int _Position { get; set; }

        public int Position
        {
            get
            {
                return this._Position;
            }
        }

        protected Task SetPosition(int value)
        {
            this._Position = value;
            return this.OnPositionChanged();
        }

        protected virtual async Task OnPositionChanged()
        {
            if (this.PositionChanged != null)
            {
                var e = new AsyncEventArgs();
                this.PositionChanged(this, e);
                await e.Complete();
            }
            this.OnPropertyChanged("Position");
        }

        public event AsyncEventHandler PositionChanged = delegate { };

        private int _Count { get; set; }

        public int Count
        {
            get
            {
                return this._Count;
            }
        }

        protected Task SetCount(int value)
        {
            this._Count = value;
            return this.OnCountChanged();
        }

        protected virtual async Task OnCountChanged()
        {
            if (this.CountChanged != null)
            {
                var e = new AsyncEventArgs();
                this.CountChanged(this, e);
                await e.Complete();
            }
            this.OnPropertyChanged("Count");
        }

        public event AsyncEventHandler CountChanged = delegate { };

        public bool IsIndeterminate
        {
            get
            {
                return false;
            }
        }

        protected virtual async Task OnIsIndeterminateChanged()
        {
            if (this.IsIndeterminateChanged != null)
            {
                var e = new AsyncEventArgs();
                this.IsIndeterminateChanged(this, e);
                await e.Complete();
            }
            this.OnPropertyChanged("IsIndeterminate");
        }

        public event AsyncEventHandler IsIndeterminateChanged = delegate { };

        public ParallelOptions ParallelOptions
        {
            get
            {
                return new ParallelOptions()
                {
                    MaxDegreeOfParallelism = Environment.ProcessorCount
                };
            }
        }

        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.IsDisposed || !disposing)
            {
                return;
            }
            this.OnDisposing();
            this.IsDisposed = true;
        }

        protected virtual void OnDisposing()
        {
            //Nothing to do.
        }

        ~PopulatorBase()
        {
            Logger.Write(this, LogLevel.Error, "Component was not disposed: {0}", this.GetType().Name);
            this.Dispose(true);
        }
    }
}
