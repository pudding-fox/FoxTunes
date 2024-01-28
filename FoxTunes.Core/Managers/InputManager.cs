using FoxTunes.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace FoxTunes
{
    public class InputManager : StandardManager, IInputManager
    {
        const int WH_KEYBOARD_LL = 13;

        public ConcurrentDictionary<Tuple<KeyboardInputType, int>, Action> Registrations { get; private set; }

        public IntPtr Hook { get; private set; }

        public KeyboardProc Callback { get; private set; }

        public IBackgroundTaskRunner BackgroundTaskRunner { get; private set; }

        public InputManager()
        {
            this.Registrations = new ConcurrentDictionary<Tuple<KeyboardInputType, int>, Action>();
            this.Callback = (int code, IntPtr input, ref KeyboardInput keys) =>
            {
                if (code >= 0)
                {
                    switch ((KeyboardInputType)input)
                    {
                        case KeyboardInputType.KeyDown:
                        case KeyboardInputType.KeyUp:
                            //TODO: Bad awaited Task.
                            var keyCode = keys.KeyCode;
                            this.BackgroundTaskRunner.Run(() => this.OnInputEvent((KeyboardInputType)input, keyCode));
                            break;
                    }
                }
                return CallNextHookEx(this.Hook, code, input, ref keys);
            };
        }

        public override void InitializeComponent(ICore core)
        {
            this.BackgroundTaskRunner = core.Components.BackgroundTaskRunner;
            this.Hook = SetHook(this.Callback);
            if (this.Hook != IntPtr.Zero)
            {
                Logger.Write(this, LogLevel.Debug, "Added keyboard hook, global keyboard shortcuts are enabled.");
            }
            else
            {
                Logger.Write(this, LogLevel.Warn, "Failed to add keyboard hook, global keyboard shortcuts are disabled.");
            }
            base.InitializeComponent(core);
        }

        protected virtual void OnInputEvent(KeyboardInputType input, int keys)
        {
            var registration = default(Action);
            if (!this.Registrations.TryGetValue(new Tuple<KeyboardInputType, int>(input, keys), out registration))
            {
                return;
            }
            Logger.Write(this, LogLevel.Debug, "Executing global keyboard shortcut: {0} => {1}", Enum.GetName(typeof(KeyboardInputType), input), keys);
            registration();
        }

        public void AddInputHook(KeyboardInputType input, int keys, Action action)
        {
            this.Registrations.AddOrUpdate(
                new Tuple<KeyboardInputType, int>(input, keys),
                action,
                (key, value) =>
                {
                    throw new NotImplementedException();
                }
            );
        }

        protected override void OnDisposing()
        {
            if (this.Hook != IntPtr.Zero)
            {
                UnhookWindowsHookEx(this.Hook);
                this.Hook = IntPtr.Zero;
            }
            base.OnDisposing();
        }

        public delegate IntPtr KeyboardProc(int code, IntPtr input, ref KeyboardInput keys);

        public static IntPtr SetHook(KeyboardProc proc)
        {
            using (var process = Process.GetCurrentProcess())
            {
                using (var module = process.MainModule)
                {
                    return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(module.ModuleName), 0);
                }
            }
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SetWindowsHookEx(int id, KeyboardProc proc, IntPtr mod, uint thread);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool UnhookWindowsHookEx(IntPtr hook);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr CallNextHookEx(IntPtr hook, int code, IntPtr input, ref KeyboardInput keys);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr GetModuleHandle(string module);

        public struct KeyboardInput
        {
            public int KeyCode;

            public int ScanCode;

            public int Flags;

            public int Time;

            public int Info;
        }
    }
}