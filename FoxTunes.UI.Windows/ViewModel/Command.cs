using FoxTunes.Interfaces;
using System;
using System.Windows.Input;

namespace FoxTunes.ViewModel
{
    public class Command : ICommand
    {
        public Command(Action action)
            : this(action, null)
        {
        }

        public Command(Action action, Func<bool> predicate)
        {
            this.Action = action;
            this.Predicate = predicate;

        }

        public Action Action { get; private set; }

        public Func<bool> Predicate { get; private set; }

        public bool CanExecute(object parameter)
        {
            if (this.Predicate == null)
            {
                return true;
            }
            return this.Predicate();
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
            if (!this.CanExecute(parameter))
            {
                throw new InvalidOperationException("Execution is not valid at this time.");
            }
            if (this.Action == null)
            {
                return;
            }
            this.Action();
        }

        public static void InvalidateRequerySuggested()
        {
            ComponentRegistry.Instance.GetComponent<IForegroundTaskRunner>().Run(() => CommandManager.InvalidateRequerySuggested());
        }

        public static readonly ICommand Disabled = new Command(() => { /*Nothing to do.*/ }, () => false);
    }

    public class Command<T> : ICommand
    {
        public Command(Action<T> action)
            : this(action, null)
        {
        }

        public Command(Action<T> action, Func<T, bool> predicate)
        {
            this.Action = action;
            this.Predicate = predicate;

        }

        public Action<T> Action { get; private set; }

        public Func<T, bool> Predicate { get; private set; }

        public bool CanExecute(object parameter)
        {
            if (this.Predicate == null)
            {
                return true;
            }
            return this.Predicate((T)parameter);
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
            if (!this.CanExecute(parameter))
            {
                throw new InvalidOperationException("Execution is not valid at this time.");
            }
            if (this.Action == null)
            {
                return;
            }
            this.Action((T)parameter);
        }
    }
}
