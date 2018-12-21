using System;

namespace FoxTunes.Interfaces
{
    public interface IInputManager : IStandardManager
    {
        void AddInputHook(KeyboardInputType input, int keys, Action action);
    }

    public enum KeyboardInputType : int
    {
        KeyDown = 0x0100,
        KeyUp = 0x0101
    }
}
