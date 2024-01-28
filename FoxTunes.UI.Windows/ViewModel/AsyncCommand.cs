using FoxTunes.Interfaces;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FoxTunes.ViewModel
{
    public class AsyncCommand : ICommand
    {
        public AsyncCommand(Func<Task> func)
        {
            this.Func = func;
        }

        public AsyncCommand(Func<Task> func, Func<bool> predicate) : this(func)
        {
            this.Predicate = predicate;
        }

        public AsyncCommand(Func<Task> func, Func<Task<bool>> predicate) : this(func)
        {
            this.AsyncPredicate = predicate;
        }

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
                this.AsyncPredicate().ContinueWith(_ =>
                {
                    if (this.CanExecute.HasValue && this.CanExecute.Value == _.Result)
                    {
                        return;
                    }
                    this.CanExecute = _.Result;
                    InvalidateRequerySuggested();
                });
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
            this.Func().ContinueWith(_ =>
            {
                InvalidateRequerySuggested();
            });
        }

        public static void InvalidateRequerySuggested()
        {
            ComponentRegistry.Instance.GetComponent<IForegroundTaskRunner>().Run(() => CommandManager.InvalidateRequerySuggested());
        }

        public static readonly ICommand Disabled = new Command(() => { /*Nothing to do.*/ }, () => false);
    }

    public class AsyncCommand<T> : ICommand
    {
        public AsyncCommand(Func<T, Task> func)
        {
            this.Func = func;
        }

        public AsyncCommand(Func<T, Task> func, Func<T, bool> predicate) : this(func)
        {
            this.Predicate = predicate;
        }

        public AsyncCommand(Func<T, Task> func, Func<T, Task<bool>> predicate) : this(func)
        {
            this.AsyncPredicate = predicate;
        }

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
                this.AsyncPredicate((T)parameter).ContinueWith(_ =>
                {
                    if (this.CanExecute.HasValue && this.CanExecute.Value == _.Result)
                    {
                        return;
                    }
                    this.CanExecute = _.Result;
                    Command.InvalidateRequerySuggested();
                });
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
            this.Func((T)parameter).ContinueWith(_ =>
            {
                Command.InvalidateRequerySuggested();
            });
        }
    }
}
