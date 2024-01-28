using FoxTunes.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace FoxTunes
{
    public abstract class InputManager : BaseManager, IInputManager
    {
        public InputManager()
        {
            this.Registrations = new ConcurrentDictionary<Tuple<int, int>, Action>();
        }

        public ConcurrentDictionary<Tuple<int, int>, Action> Registrations { get; private set; }

        public bool _Enabled { get; private set; }

        public bool Enabled
        {
            get
            {
                return this._Enabled;
            }
            set
            {
                this._Enabled = value;
                this.OnEnabledChanged();
            }
        }

        protected virtual void OnEnabledChanged()
        {
            //Nothing to do.
        }

        public IBackgroundTaskRunner BackgroundTaskRunner { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.BackgroundTaskRunner = core.Components.BackgroundTaskRunner;
            this.Configuration = core.Components.Configuration;
            this.Configuration.GetElement<BooleanConfigurationElement>(
                InputManagerConfiguration.KEYBOARD_SHORTCUTS_SECTION,
                InputManagerConfiguration.ENABLED_ELEMENT
            ).ConnectValue<bool>(value => this.Enabled = value);
            base.InitializeComponent(core);
        }

        public void AddInputHook(int modifiers, int keys, Action action)
        {
            this.Registrations.AddOrUpdate(
                new Tuple<int, int>(modifiers, keys),
                action,
                (key, value) =>
                {
                    throw new NotImplementedException();
                }
            );
        }

        public void RemoveInputHook(int modifiers, int keys)
        {
            this.Registrations.TryRemove(
                new Tuple<int, int>(modifiers, keys)
            );
        }

        public IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return InputManagerConfiguration.GetConfigurationSections();
        }
    }
}
