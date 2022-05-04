using FoxTunes.Interfaces;
using FoxTunes.ViewModel;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace FoxTunes
{
    [WindowsUserInterfaceDependency]
    public class KeyBindingsBehaviour : StandardBehaviour, IDisposable, IConfigurableComponent
    {
        public const string SEARCH = "5125ACDE-CC68-4DFE-82B0-F96A0ED303B6";

        public global::FoxTunes.ViewModel.Playback _Playback { get; private set; }

        public global::FoxTunes.ViewModel.Settings _Settings { get; private set; }

        public IPlaybackManager PlaybackManager { get; private set; }

        public IPlaylistManager PlaylistManager { get; private set; }

        public ISignalEmitter SignalEmitter { get; private set; }

        public IErrorEmitter ErrorEmitter { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public TextConfigurationElement Play { get; private set; }

        public TextConfigurationElement Previous { get; private set; }

        public TextConfigurationElement Next { get; private set; }

        public TextConfigurationElement Stop { get; private set; }

        public TextConfigurationElement Settings { get; private set; }

        public TextConfigurationElement Search { get; private set; }

        public ICommand SearchCommand
        {
            get
            {
                //TODO: This is a hack. The SearchBox control listens for this signal. Nothing else uses this pattern.
                return CommandFactory.Instance.CreateCommand(
                    () => this.SignalEmitter.Send(new Signal(this, CommonSignals.PluginInvocation, SEARCH))
                );
            }
        }

        public override void InitializeComponent(ICore core)
        {
            this._Playback = new global::FoxTunes.ViewModel.Playback(false);
            this._Settings = new global::FoxTunes.ViewModel.Settings();
            Windows.Registrations.AddCreated(
                Windows.Registrations.IdsByRole(UserInterfaceWindowRole.Main),
                this.OnWindowCreated
            );
            this.PlaybackManager = core.Managers.Playback;
            this.PlaylistManager = core.Managers.Playlist;
            this.SignalEmitter = core.Components.SignalEmitter;
            this.ErrorEmitter = core.Components.ErrorEmitter;
            this.Configuration = core.Components.Configuration;
            this.Play = this.Configuration.GetElement<TextConfigurationElement>(
                InputManagerConfiguration.SECTION,
                KeyBindingsBehaviourConfiguration.PLAY_ELEMENT
            );
            this.Previous = this.Configuration.GetElement<TextConfigurationElement>(
                InputManagerConfiguration.SECTION,
                KeyBindingsBehaviourConfiguration.PREVIOUS_ELEMENT
            );
            this.Next = this.Configuration.GetElement<TextConfigurationElement>(
                InputManagerConfiguration.SECTION,
                KeyBindingsBehaviourConfiguration.NEXT_ELEMENT
            );
            this.Stop = this.Configuration.GetElement<TextConfigurationElement>(
                InputManagerConfiguration.SECTION,
                KeyBindingsBehaviourConfiguration.STOP_ELEMENT
            );
            this.Settings = this.Configuration.GetElement<TextConfigurationElement>(
                InputManagerConfiguration.SECTION,
                KeyBindingsBehaviourConfiguration.SETTINGS_ELEMENT
            );
            this.Search = this.Configuration.GetElement<TextConfigurationElement>(
                InputManagerConfiguration.SECTION,
                KeyBindingsBehaviourConfiguration.SEARCH_ELEMENT
            );
            foreach (var element in new[] { this.Play, this.Previous, this.Next, this.Stop, this.Settings, this.Search })
            {
                if (element == null)
                {
                    continue;
                }
                element.ValueChanged += this.OnValueChanged;
            }
            base.InitializeComponent(core);
        }

        protected virtual void OnWindowCreated(object sender, EventArgs e)
        {
            if (sender is Window window)
            {
                this.Update(window);
            }
        }

        protected virtual void OnValueChanged(object sender, EventArgs e)
        {
            this.Update();
        }

        protected virtual void Update()
        {
            foreach (var window in Windows.Registrations.WindowsByIds(Windows.Registrations.IdsByRole(UserInterfaceWindowRole.Main)))
            {
                this.Update(window);
            }
        }

        protected virtual void Update(Window window)
        {
            this.RemoveCommandBindings(window);
            if (this.Play != null)
            {
                this.AddCommandBinding(window, this.Play.Value, this._Playback.PlayCommand);
            }
            if (this.Next != null)
            {
                this.AddCommandBinding(window, this.Next.Value, this._Playback.NextCommand);
            }
            if (this.Previous != null)
            {
                this.AddCommandBinding(window, this.Previous.Value, this._Playback.PreviousCommand);
            }
            if (this.Stop != null)
            {
                this.AddCommandBinding(window, this.Stop.Value, this._Playback.StopOutputCommand);
            }
            if (this.Settings != null)
            {
                this.AddCommandBinding(window, this.Settings.Value, this._Settings.ShowCommand);
            }
            if (this.Search != null)
            {
                this.AddCommandBinding(window, this.Search.Value, this.SearchCommand);
            }
        }

        protected virtual void AddCommandBinding(Window window, string keys, ICommand command)
        {
            var key = default(Key);
            var modifiers = default(ModifierKeys);
            if (keys.TryGetKeys(out modifiers, out key))
            {
                var gesture = new KeyGesture(key, modifiers);
                window.InputBindings.Add(new InputBinding(command, gesture));
                Logger.Write(this, LogLevel.Debug, "AddCommandBinding: {0}/{1} => {2}", window.GetType().Name, window.Title, keys);
            }
            else
            {
                this.ErrorEmitter.Send(string.Format("Failed to register input hook {0}", keys));
            }
        }

        protected virtual void RemoveCommandBindings()
        {
            foreach (var window in Windows.Registrations.WindowsByIds(Windows.Registrations.IdsByRole(UserInterfaceWindowRole.Main)))
            {
                this.RemoveCommandBindings(window);
            }
        }

        protected virtual void RemoveCommandBindings(Window window)
        {
            //TODO: We should only remove command bindings that *we* created.
            window.CommandBindings.Clear();
            Logger.Write(this, LogLevel.Debug, "RemoveCommandBindings: {0}/{1}", window.GetType().Name, window.Title);
        }

        public IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return KeyBindingsBehaviourConfiguration.GetConfigurationSections();
        }

        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.IsDisposed || !disposing)
            {
                return;
            }
            this.OnDisposing();
            this.IsDisposed = true;
        }

        protected virtual void OnDisposing()
        {
            Windows.Registrations.RemoveCreated(
                Windows.Registrations.IdsByRole(UserInterfaceWindowRole.Main),
                this.OnWindowCreated
            );
            foreach (var element in new[] { this.Play, this.Previous, this.Next, this.Stop, this.Settings, this.Search })
            {
                if (element == null)
                {
                    continue;
                }
                element.ValueChanged -= this.OnValueChanged;
            }
            this.RemoveCommandBindings();
            if (this._Playback != null)
            {
                this._Playback.Dispose();
            }
            if (this._Settings != null)
            {
                this._Settings.Dispose();
            }
        }

        ~KeyBindingsBehaviour()
        {
            Logger.Write(this, LogLevel.Error, "Component was not disposed: {0}", this.GetType().Name);
            try
            {
                this.Dispose(true);
            }
            catch
            {
                //Nothing can be done, never throw on GC thread.
            }
        }
    }
}
