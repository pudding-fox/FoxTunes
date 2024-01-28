using FoxTunes.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FoxTunes
{
    public abstract class BackgroundTask : BaseComponent, IBackgroundTask
    {
        static BackgroundTask()
        {
            Instances = new List<WeakReference>();
            Semaphores = new ConcurrentDictionary<string, SemaphoreSlim>();
        }

        private static List<WeakReference> Instances { get; set; }

        private static ConcurrentDictionary<string, SemaphoreSlim> Semaphores { get; set; }

        public static IEnumerable<IBackgroundTask> Active
        {
            get
            {
                return Instances
                    .Where(instance => instance.IsAlive)
                    .Select(instance => (IBackgroundTask)instance.Target)
                    .Where(backgroundTask => !(backgroundTask.IsCompleted || backgroundTask.IsFaulted))
                    .ToArray();
            }
        }

        protected static void OnActiveChanged()
        {
            if (ActiveChanged == null)
            {
                return;
            }
            ActiveChanged(null, EventArgs.Empty);
        }

        public static event EventHandler ActiveChanged;

        protected BackgroundTask(string id)
        {
            this.Id = id;
        }

        public string Id { get; private set; }

        public virtual bool Visible
        {
            get
            {
                return false;
            }
        }

        public virtual bool Cancellable
        {
            get
            {
                return false;
            }
        }

        public bool IsCancellationRequested { get; protected set; }

        protected virtual void OnCancellationRequested()
        {
            if (this.CancellationRequested == null)
            {
                return;
            }
            this.CancellationRequested(this, EventArgs.Empty);
        }

        public event EventHandler CancellationRequested;

        public virtual int Concurrency
        {
            get
            {
                return 1;
            }
        }

        public SemaphoreSlim Semaphore
        {
            get
            {
                return Semaphores.GetOrAdd(this.Id, key => new SemaphoreSlim(this.Concurrency, this.Concurrency));
            }
        }

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
            await this.OnIsIndeterminateChanged();
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
            await this.OnIsIndeterminateChanged();
        }

        public event AsyncEventHandler CountChanged;

        public bool IsIndeterminate
        {
            get
            {
                return this.Position == 0 && this.Count == 0;
            }
        }

        protected async Task SetIsIndeterminate(bool value)
        {
            if (value)
            {
                await this.SetPosition(0);
                await this.SetCount(0);
            }
            else
            {
                //Nothing to do.
            }
            await this.OnIsIndeterminateChanged();
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

        public override void InitializeComponent(ICore core)
        {
            Instances.Add(new WeakReference(this));
            OnActiveChanged();
            base.InitializeComponent(core);
        }

        public virtual async Task Run()
        {
            Logger.Write(this, LogLevel.Debug, "Running background task.");
            await this.OnStarted();
#if NET40
            Semaphore.Wait();
#else
            await Semaphore.WaitAsync();
#endif
            try
            {
                try
                {
                    await this.OnRun();
                }
                finally
                {
                    this.Semaphore.Release();
                }
                Logger.Write(this, LogLevel.Debug, "Background task succeeded.");
                await this.OnCompleted();
                return;
            }
            catch (AggregateException e)
            {
                foreach (var innerException in e.InnerExceptions)
                {
                    Logger.Write(this, LogLevel.Error, "Background task failed: {0}", innerException.Message);
                }
                this.Exception = e;
            }
            catch (Exception e)
            {
                Logger.Write(this, LogLevel.Error, "Background task failed: {0}", e.Message);
                this.Exception = e;
            }
            await this.OnFaulted();
        }

        protected abstract Task OnRun();

        protected virtual Task OnStarted()
        {
            this.IsStarted = true;
            if (this.Started == null)
            {
#if NET40
                return TaskEx.FromResult(false);
#else
                return Task.CompletedTask;
#endif
            }
            var e = new AsyncEventArgs();
            this.Started(this, e);
            return e.Complete();
        }

        public event AsyncEventHandler Started;

        public bool IsStarted { get; private set; }

        protected virtual Task OnCompleted()
        {
            this.IsCompleted = true;
            if (this.Completed == null)
            {
#if NET40
                return TaskEx.FromResult(false);
#else
                return Task.CompletedTask;
#endif
            }
            var e = new AsyncEventArgs();
            this.Completed(this, e);
            return e.Complete();
        }

        public event AsyncEventHandler Completed;

        public bool IsCompleted { get; private set; }

        private Exception _Exception { get; set; }

        public Exception Exception
        {
            get
            {
                return this._Exception;
            }
            protected set
            {
                this._Exception = value;
                this.OnExceptionChanged();
            }
        }

        protected virtual void OnExceptionChanged()
        {
            if (this.ExceptionChanged != null)
            {
                this.ExceptionChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Exception");
        }

        public event EventHandler ExceptionChanged;

        protected virtual Task OnFaulted()
        {
            this.IsFaulted = true;
            if (this.Faulted == null)
            {
#if NET40
                return TaskEx.FromResult(false);
#else
                return Task.CompletedTask;
#endif
            }
            var e = new AsyncEventArgs();
            this.Faulted(this, e);
            return e.Complete();
        }

        public event AsyncEventHandler Faulted;

        public bool IsFaulted { get; private set; }

        public virtual void Cancel()
        {
            if (!this.Cancellable)
            {
                throw new NotImplementedException();
            }
            this.IsCancellationRequested = true;
            this.OnCancellationRequested();
        }

        protected virtual async Task WithPopulator(PopulatorBase populator, Func<Task> func)
        {
            var nameChanged = new AsyncEventHandler(async (sender, e) => { using (e.Defer()) await this.SetName(populator.Name); });
            var descriptionChanged = new AsyncEventHandler(async (sender, e) => { using (e.Defer()) await this.SetDescription(populator.Description); });
            var positionChanged = new AsyncEventHandler(async (sender, e) => { using (e.Defer()) await this.SetPosition(populator.Position); });
            var countChanged = new AsyncEventHandler(async (sender, e) => { using (e.Defer()) await this.SetCount(populator.Count); });
            var isIndeterminateChanged = new AsyncEventHandler(async (sender, e) => { using (e.Defer()) await this.SetIsIndeterminate(populator.IsIndeterminate); });
            populator.NameChanged += nameChanged;
            populator.DescriptionChanged += descriptionChanged;
            populator.PositionChanged += positionChanged;
            populator.CountChanged += countChanged;
            populator.IsIndeterminateChanged += isIndeterminateChanged;
            try
            {
                await func();
            }
            finally
            {
                populator.NameChanged -= nameChanged;
                populator.DescriptionChanged -= descriptionChanged;
                populator.PositionChanged -= positionChanged;
                populator.CountChanged -= countChanged;
                populator.IsIndeterminateChanged -= isIndeterminateChanged;
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
            var instances = Instances.ToArray();
            foreach (var instance in instances)
            {
                if (!instance.IsAlive)
                {
                    Instances.Remove(instance);
                }
                else if (object.ReferenceEquals(this, instance.Target))
                {
                    Instances.Remove(instance);
                }
            }
            OnActiveChanged();
        }

        ~BackgroundTask()
        {
            Logger.Write(this, LogLevel.Error, "Component was not disposed: {0}", this.GetType().Name);
            this.Dispose(true);
        }

        public static async Task<bool> Shutdown(TimeSpan interval, TimeSpan timeout)
        {
            OnShutdownStarted();
            do
            {
                var instances = Active.ToArray();
                if (instances.Length == 0)
                {
                    break;
                }
                foreach (var instance in instances)
                {
                    if (!instance.Cancellable || instance.IsCancellationRequested)
                    {
                        continue;
                    }
                    instance.Cancel();
                }
#if NET40
                await TaskEx.Delay(interval);
#else
                await Task.Delay(interval);
#endif
                timeout -= interval;
                if (timeout <= TimeSpan.Zero)
                {
                    return false;
                }
            } while (true);
            OnShutdownCompleted();
            return true;
        }

        private static void OnShutdownStarted()
        {
            if (ShutdownStarted == null)
            {
                return;
            }
            ShutdownStarted(null, EventArgs.Empty);
        }

        public static event EventHandler ShutdownStarted;

        private static void OnShutdownCompleted()
        {
            if (ShutdownCompleted == null)
            {
                return;
            }
            ShutdownCompleted(null, EventArgs.Empty);
        }

        public static event EventHandler ShutdownCompleted;
    }
}
