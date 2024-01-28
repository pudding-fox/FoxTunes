using FoxTunes.Interfaces;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FoxTunes.ViewModel
{
    public class AsyncCommand : ICommand
    {
        public AsyncCommand(IBackgroundTaskRunner backgroundTaskRunner, Func<Task> func)
        {
            this.BackgroundTaskRunner = backgroundTaskRunner;
            this.Func = func;
        }

        public AsyncCommand(IBackgroundTaskRunner backgroundTaskRunner, Func<Task> func, Func<bool> predicate) : this(backgroundTaskRunner, func)
        {
            this.Predicate = predicate;
        }

        public AsyncCommand(IBackgroundTaskRunner backgroundTaskRunner, Func<Task> func, Func<Task<bool>> predicate) : this(backgroundTaskRunner, func)
        {
            this.AsyncPredicate = predicate;
        }

        public IBackgroundTaskRunner BackgroundTaskRunner { get; private set; }

        public Func<Task> Func { get; private set; }

        public Func<bool> Predicate { get; private set; }

        public Func<Task<bool>> AsyncPredicate { get; private set; }

        public bool? CanExecute { get; private set; }

        bool ICommand.CanExecute(object parameter)
        {
            if (this.Predicate != null)
            {
                return this.Predicate();
            }
            else if (this.AsyncPredicate != null)
            {
                if (this.BackgroundTaskRunner != null)
                {
                    //TODO: Bad .Wait()
                    this.BackgroundTaskRunner.Run(() => this.AsyncPredicate().ContinueWith(async task =>
                    {
                        if (this.CanExecute.HasValue && this.CanExecute.Value == task.Result)
                        {
                            return;
                        }
                        this.CanExecute = task.Result;
                        await InvalidateRequerySuggested();
                    })).Wait();
                }
                if (this.CanExecute.HasValue)
                {
                    return this.CanExecute.Value;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        protected virtual void OnCanExecuteChanged()
        {
            if (this._CanExecuteChanged == null)
            {
                return;
            }
            this._CanExecuteChanged(this, EventArgs.Empty);
        }

        public event EventHandler _CanExecuteChanged = delegate { };

        public event EventHandler CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
                this._CanExecuteChanged += value;
            }

            remove
            {
                CommandManager.RequerySuggested -= value;
                this._CanExecuteChanged -= value;
            }
        }

        public void Execute(object parameter)
        {
            if (this.Func == null)
            {
                return;
            }
            this.Func().ContinueWith(async task => await InvalidateRequerySuggested());
        }

        public static Task InvalidateRequerySuggested()
        {
            var foregroundTaskRunner = ComponentRegistry.Instance.GetComponent<IForegroundTaskRunner>();
            if (foregroundTaskRunner != null)
            {
                return foregroundTaskRunner.RunAsync(() => CommandManager.InvalidateRequerySuggested());
            }
            else
            {
                CommandManager.InvalidateRequerySuggested();
                return Task.CompletedTask;
            }
        }

        public static readonly ICommand Disabled = new Command(() => { /*Nothing to do.*/ }, () => false);
    }

    public class AsyncCommand<T> : ICommand
    {
        public AsyncCommand(IBackgroundTaskRunner backgroundTaskRunner, Func<T, Task> func)
        {
            this.BackgroundTaskRunner = backgroundTaskRunner;
            this.Func = func;
        }

        public AsyncCommand(IBackgroundTaskRunner backgroundTaskRunner, Func<T, Task> func, Func<T, bool> predicate) : this(backgroundTaskRunner, func)
        {
            this.Predicate = predicate;
        }

        public AsyncCommand(IBackgroundTaskRunner backgroundTaskRunner, Func<T, Task> func, Func<T, Task<bool>> predicate) : this(backgroundTaskRunner, func)
        {
            this.AsyncPredicate = predicate;
        }

        public IBackgroundTaskRunner BackgroundTaskRunner { get; private set; }

        public Func<T, Task> Func { get; private set; }

        public Func<T, bool> Predicate { get; private set; }

        public Func<T, Task<bool>> AsyncPredicate { get; private set; }

        public bool? CanExecute { get; private set; }

        bool ICommand.CanExecute(object parameter)
        {
            if (this.Predicate != null)
            {
                return this.Predicate((T)parameter);
            }
            else if (this.AsyncPredicate != null)
            {
                if (this.BackgroundTaskRunner != null)
                {
                    //TODO: Bad .Wait()
                    this.BackgroundTaskRunner.Run(() => this.AsyncPredicate((T)parameter).ContinueWith(async task =>
                    {
                        if (this.CanExecute.HasValue && this.CanExecute.Value == task.Result)
                        {
                            return;
                        }
                        this.CanExecute = task.Result;
                        await Command.InvalidateRequerySuggested();
                    })).Wait();
                }
                if (this.CanExecute.HasValue)
                {
                    return this.CanExecute.Value;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        protected virtual void OnCanExecuteChanged()
        {
            if (this._CanExecuteChanged == null)
            {
                return;
            }
            this._CanExecuteChanged(this, EventArgs.Empty);
        }

        public event EventHandler _CanExecuteChanged = delegate { };

        public event EventHandler CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
                this._CanExecuteChanged += value;
            }

            remove
            {
                CommandManager.RequerySuggested -= value;
                this._CanExecuteChanged -= value;
            }
        }

        public void Execute(object parameter)
        {
            if (this.Func == null)
            {
                return;
            }
            this.Func((T)parameter).ContinueWith(async task => await Command.InvalidateRequerySuggested());
        }
    }
}
