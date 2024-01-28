using System;

namespace FoxTunes.Interfaces
{
    public interface IInputManager : IStandardManager
    {
        bool Enabled { get; set; }

        void AddInputHook(int input, int modifiers, int keys, Action action);

        void RemoveInputHook(int input, int modifiers, int keys);
    }
}
