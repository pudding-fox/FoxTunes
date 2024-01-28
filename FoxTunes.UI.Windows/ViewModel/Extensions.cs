using FoxTunes.Interfaces;
using System.Windows.Input;

namespace FoxTunes.ViewModel
{
    public static partial class Extensions
    {
        public static void AddInputHook(this IInputManager inputManager, KeyboardInputType input, int keys, ICommand command)
        {
            inputManager.AddInputHook(input, keys, () =>
            {
                if (!command.CanExecute(null))
                {
                    return;
                }
                command.Execute(null);
            });
        }
    }
}
