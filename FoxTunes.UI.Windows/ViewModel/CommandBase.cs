using FoxTunes.Interfaces;
using System;
using System.Windows.Input;

namespace FoxTunes.ViewModel
{
    public abstract class CommandBase : ICommand
    {
        protected static readonly IErrorEmitter ErrorEmitter = ComponentRegistry.Instance.GetComponent<IErrorEmitter>();

        protected static ILogger Logger
        {
            get
            {
                return LogManager.Logger;
            }
        }

        public string Tag { get; set; }

        public abstract bool CanExecute(object parameter);

        public abstract void Execute(object parameter);

        protected virtual void OnCanExecuteChanged()
        {
            if (this._CanExecuteChanged == null)
            {
                return;
            }
            this._CanExecuteChanged(this, EventArgs.Empty);
        }

        public event EventHandler _CanExecuteChanged;

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

        public static readonly ICommand Disabled = new Command(() => { /*Nothing to do.*/ }, () => false);
    }
}
