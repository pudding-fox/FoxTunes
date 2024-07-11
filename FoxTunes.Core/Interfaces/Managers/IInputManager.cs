using System;

namespace FoxTunes.Interfaces
{
    public interface IInputManager : IConfigurableComponent
    {
        bool Enabled { get; set; }

        void AddInputHook(int modifiers, int keys, Action action);

        void RemoveInputHook(int modifiers, int keys);
    }
}
