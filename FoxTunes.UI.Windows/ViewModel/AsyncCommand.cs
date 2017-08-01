using FoxTunes.Interfaces;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FoxTunes.ViewModel
{
    public class AsyncCommand : ICommand
    {
        public AsyncCommand(Func<Task> func)
            : this(func, null)
        {
        }

        public AsyncCommand(Func<Task> func, Func<bool> predicate)
        {
            this.Func = func;
            this.Predicate = predicate;

        }

        public Func<Task> Func { get; private set; }

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
            if (this.Func == null)
            {
                return;
            }
            this.Func().ContinueWith(_ =>
            {
                ComponentRegistry.Instance.GetComponent<IForegroundTaskRunner>().Run(() => CommandManager.InvalidateRequerySuggested());
            });
        }

        public static readonly ICommand Disabled = new Command(() => { /*Nothing to do.*/ }, () => false);
    }
}
