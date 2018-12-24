using FoxTunes.Interfaces;
using System;
using System.Linq;
using System.Windows.Input;

namespace FoxTunes.ViewModel
{
    public static partial class Extensions
    {
        public static void AddInputHook(this IInputManager inputManager, string phrase, Action action)
        {
            var modifiers = default(int);
            var keys = default(int);
            if (!TryGetKey(phrase, out modifiers, out keys))
            {
                throw new NotImplementedException();
            }
            inputManager.AddInputHook(modifiers, keys, action);
        }

        public static void RemoveInputHook(this IInputManager inputManager, string phrase)
        {
            var modifiers = default(int);
            var keys = default(int);
            if (!TryGetKey(phrase, out modifiers, out keys))
            {
                throw new NotImplementedException();
            }
            inputManager.RemoveInputHook(modifiers, keys);
        }

        private static bool TryGetKey(string phrase, out int modifiers, out int keys)
        {
            modifiers = 0;
            var key = Key.None;
            var sequence = phrase.Split(new[] { "+" }, StringSplitOptions.RemoveEmptyEntries).Select(element => element.Trim());
            foreach (var element in sequence)
            {
                var modifier = default(ModifierKeys);
                if (Enum.TryParse<ModifierKeys>(element, true, out modifier))
                {
                    modifiers |= (int)modifier;
                }
                else if (Enum.TryParse<Key>(element, true, out key))
                {
                    //Nothing to do.
                }
                else
                {
                    keys = default(int);
                    return false;
                }
            }
            keys = KeyInterop.VirtualKeyFromKey(key);
            return true;
        }
    }
}
