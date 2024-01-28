using FoxTunes.Interfaces;
using System;
using System.Threading.Tasks;

namespace FoxTunes.ViewModel
{
    public class AsyncCommand : CommandBase
    {
        public AsyncCommand(IBackgroundTaskRunner backgroundTaskRunner, Func<Task> func)
        {
            this.BackgroundTaskRunner = backgroundTaskRunner;
            this.Func = func;
        }

        public AsyncCommand(IBackgroundTaskRunner backgroundTaskRunner, Func<Task> func, Func<bool> predicate)
            : this(backgroundTaskRunner, func)
        {
            this.Predicate = predicate;
        }

        public AsyncCommand(IBackgroundTaskRunner backgroundTaskRunner, Func<Task> func, Func<Task<bool>> predicate)
            : this(backgroundTaskRunner, func)
        {
            this.AsyncPredicate = predicate;
        }

        public IBackgroundTaskRunner BackgroundTaskRunner { get; private set; }

        public Func<Task> Func { get; private set; }

        public Func<bool> Predicate { get; private set; }

        public Func<Task<bool>> AsyncPredicate { get; private set; }

        public bool? _CanExecute { get; private set; }

        public override bool CanExecute(object parameter)
        {
            if (this.Predicate != null)
            {
                return this.Predicate();
            }
            else if (this.AsyncPredicate != null)
            {
                if (this.BackgroundTaskRunner != null)
                {
                    this.OnCanExecute();
                }
                if (this._CanExecute.HasValue)
                {
                    return this._CanExecute.Value;
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

        protected virtual Task OnCanExecute()
        {
            return this.BackgroundTaskRunner.Run(async () =>
            {
                var canExecute = await this.AsyncPredicate();
                if (this._CanExecute.HasValue && this._CanExecute.Value == canExecute)
                {
                    return;
                }
                this._CanExecute = canExecute;
                await InvalidateRequerySuggested();
            });
        }

        public override async void Execute(object parameter)
        {
            if (this.Func == null)
            {
                return;
            }
            this.OnPhase(CommandPhase.Before, this.Tag, parameter);
            await this.BackgroundTaskRunner.Run(async () =>
            {
                await this.Func();
                this.OnPhase(CommandPhase.After, this.Tag, parameter);
                await InvalidateRequerySuggested();
            });
        }
    }

    public class AsyncCommand<T> : CommandBase
    {
        public AsyncCommand(IBackgroundTaskRunner backgroundTaskRunner, Func<T, Task> func)
        {
            this.BackgroundTaskRunner = backgroundTaskRunner;
            this.Func = func;
        }

        public AsyncCommand(IBackgroundTaskRunner backgroundTaskRunner, Func<T, Task> func, Func<T, bool> predicate)
            : this(backgroundTaskRunner, func)
        {
            this.Predicate = predicate;
        }

        public AsyncCommand(IBackgroundTaskRunner backgroundTaskRunner, Func<T, Task> func, Func<T, Task<bool>> predicate)
            : this(backgroundTaskRunner, func)
        {
            this.AsyncPredicate = predicate;
        }

        public IBackgroundTaskRunner BackgroundTaskRunner { get; private set; }

        public Func<T, Task> Func { get; private set; }

        public Func<T, bool> Predicate { get; private set; }

        public Func<T, Task<bool>> AsyncPredicate { get; private set; }

        public bool? _CanExecute { get; private set; }

        public override bool CanExecute(object parameter)
        {

            if (this.Predicate != null)
            {
                return this.Predicate((T)parameter);
            }
            else if (this.AsyncPredicate != null)
            {
                if (this.BackgroundTaskRunner != null)
                {
                    this.OnCanExecute(parameter);
                }
                if (this._CanExecute.HasValue)
                {
                    return this._CanExecute.Value;
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

        protected virtual Task OnCanExecute(object parameter)
        {
            return this.BackgroundTaskRunner.Run(async () =>
            {
                var canExecute = await this.AsyncPredicate((T)parameter);
                if (this._CanExecute.HasValue && this._CanExecute.Value == canExecute)
                {
                    return;
                }
                this._CanExecute = canExecute;
                await InvalidateRequerySuggested();
            });
        }

        public override async void Execute(object parameter)
        {
            if (this.Func == null)
            {
                return;
            }
            this.OnPhase(CommandPhase.Before, this.Tag, parameter);
            await this.BackgroundTaskRunner.Run(async () =>
            {
                await this.Func((T)parameter);
                this.OnPhase(CommandPhase.After, this.Tag, parameter);
                await InvalidateRequerySuggested();
            });
        }
    }
}
