using FoxTunes.Interfaces;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FoxTunes.ViewModel
{
    public abstract class CommandBase : ICommand
    {
        public string Tag { get; set; }

        public abstract bool CanExecute(object parameter);

        public abstract void Execute(object parameter);

        protected virtual void OnPhase(CommandPhase phase, string tag, object parameter)
        {
            if (Phase == null)
            {
                return;
            }
            Phase(this, new CommandPhaseEventArgs(phase, tag, parameter));
        }

        public static event CommandPhaseEventHandler Phase = delegate { };

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

        public static readonly ICommand Disabled = new Command(() => { /*Nothing to do.*/ }, () => false);
    }

    public delegate void CommandPhaseEventHandler(object sender, CommandPhaseEventArgs e);

    public class CommandPhaseEventArgs : EventArgs
    {
        public CommandPhaseEventArgs(CommandPhase phase, string tag, object parameter)
        {
            this.Phase = phase;
            this.Tag = tag;
            this.Parameter = parameter;
        }

        public CommandPhase Phase { get; private set; }

        public string Tag { get; private set; }

        public object Parameter { get; private set; }
    }


    [Flags]
    public enum CommandPhase : byte
    {
        None = 0,
        Before = 1,
        After = 2
    }
}
