using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace FoxTunes
{
    public abstract class PopulatorBase : BaseComponent, IReportsProgress, IDisposable
    {
        public const int LONG_INTERVAL = 1000;

        public const int NORMAL_INTERVAL = 100;

        public const int FAST_INTERVAL = 10;

        public static readonly object SyncRoot = new object();

        private PopulatorBase()
        {
            this.Semaphore = new SemaphoreSlim(1, 1);
        }

        public PopulatorBase(bool reportProgress) : this()
        {
            this.ReportProgress = reportProgress;
            if (this.ReportProgress)
            {
                lock (SyncRoot)
                {
                    this.Timer = new global::System.Timers.Timer();
                    this.Timer.Interval = NORMAL_INTERVAL;
                    this.Timer.AutoReset = false;
                    this.Timer.Elapsed += this.OnElapsed;
                }
                this.CountMetric = new Metric(10);
                this.SecondsMetric = new Metric(10);
            }
        }

        protected virtual void OnElapsed(object sender, ElapsedEventArgs e)
        {
            lock (SyncRoot)
            {
                if (this.Timer == null)
                {
                    return;
                }
                this.Timer.Start();
            }
        }

        public SemaphoreSlim Semaphore { get; private set; }

        public bool ReportProgress { get; private set; }

        public global::System.Timers.Timer Timer { get; private set; }

        public Metric CountMetric { get; private set; }

        public Metric SecondsMetric { get; private set; }

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

        public event AsyncEventHandler NameChanged;

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

        public event AsyncEventHandler DescriptionChanged;

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

        public event AsyncEventHandler PositionChanged;

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

        public event AsyncEventHandler CountChanged;

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

        public event AsyncEventHandler IsIndeterminateChanged;

        public virtual ParallelOptions ParallelOptions
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
            lock (SyncRoot)
            {
                if (this.Timer != null)
                {
                    this.Timer.Stop();
                    this.Timer.Elapsed -= this.OnElapsed;
                    this.Timer.Dispose();
                    this.Timer = null;
                }
            }
        }

        ~PopulatorBase()
        {
            Logger.Write(this, LogLevel.Error, "Component was not disposed: {0}", this.GetType().Name);
            try
            {
                this.Dispose(true);
            }
            catch
            {
                //Nothing can be done, never throw on GC thread.
            }
        }

        public string GetEta(int count)
        {
            var seconds = this.SecondsMetric.Next(
                (this.Count - this.Position) / this.CountMetric.Average(count)
            );
            var time = TimeSpan.FromSeconds(seconds);
            var elements = new List<string>();
            if (time.Hours > 0)
            {
                elements.Add(string.Format("{0} hours", time.Hours));
            }
            if (time.Minutes > 0)
            {
                elements.Add(string.Format("{0} minutes", time.Minutes));
            }
            if (time.Seconds > 0)
            {
                elements.Add(string.Format("{0} seconds", time.Seconds));
            }
            return string.Join(", ", elements);
        }
    }
}
