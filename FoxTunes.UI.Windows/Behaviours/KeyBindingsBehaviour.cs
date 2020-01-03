using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.UserInterface)]
    public class KeyBindingsBehaviour : StandardBehaviour, IDisposable, IConfigurableComponent
    {
        public global::FoxTunes.ViewModel.Playback _Playback { get; private set; }

        public global::FoxTunes.ViewModel.Settings _Settings { get; private set; }

        public global::FoxTunes.ViewModel.MiniPlayer _MiniPlayer { get; private set; }

        public IPlaybackManager PlaybackManager { get; private set; }

        public IPlaylistManager PlaylistManager { get; private set; }

        public ISignalEmitter SignalEmitter { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public TextConfigurationElement Play { get; private set; }

        public TextConfigurationElement Previous { get; private set; }

        public TextConfigurationElement Next { get; private set; }

        public TextConfigurationElement Stop { get; private set; }

        public TextConfigurationElement Settings { get; private set; }

        public TextConfigurationElement MiniPlayer { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this._Playback = new global::FoxTunes.ViewModel.Playback(true)
            {
                Core = core,
            };
            this._Settings = new global::FoxTunes.ViewModel.Settings()
            {
                Core = core
            };
            this._MiniPlayer = new global::FoxTunes.ViewModel.MiniPlayer()
            {
                Core = core
            };
            Windows.MainWindowCreated += this.OnWindowCreated;
            Windows.MiniWindowCreated += this.OnWindowCreated;
            this.PlaybackManager = core.Managers.Playback;
            this.PlaylistManager = core.Managers.Playlist;
            this.SignalEmitter = core.Components.SignalEmitter;
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
            this.MiniPlayer = this.Configuration.GetElement<TextConfigurationElement>(
                InputManagerConfiguration.SECTION,
                KeyBindingsBehaviourConfiguration.MINI_PLAYER_ELEMENT
            );
            foreach (var element in new[] { this.Play, this.Previous, this.Next, this.Stop, this.Settings, this.MiniPlayer })
            {
                if (element == null)
                {
                    continue;
                }
                element.ValueChanged += this.OnValueChanged;
            }
            this.Update();
            base.InitializeComponent(core);
        }

        protected virtual void OnValueChanged(object sender, EventArgs e)
        {
            this.Update();
        }

        protected virtual void OnWindowCreated(object sender, EventArgs e)
        {
            var window = sender as Window;
            if (window == null)
            {
                return;
            }
            this.Update(window);
        }

        protected virtual void Update()
        {
            if (Windows.IsMainWindowCreated)
            {
                this.Update(Windows.MainWindow);
            }
            if (Windows.IsMiniWindowCreated)
            {
                this.Update(Windows.MiniWindow);
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
            if (this.MiniPlayer != null)
            {
                this.AddCommandBinding(window, this.MiniPlayer.Value, this._MiniPlayer.ToggleCommand);
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
            }
            else
            {
                this.OnError(string.Format("Failed to register input hook {0}", keys));
            }
        }

        protected virtual void RemoveCommandBindings()
        {
            if (Windows.IsMainWindowCreated)
            {
                this.RemoveCommandBindings(Windows.MainWindow);
            }
            if (Windows.IsMiniWindowCreated)
            {
                this.RemoveCommandBindings(Windows.MiniWindow);
            }
        }

        protected virtual void RemoveCommandBindings(Window window)
        {
            window.CommandBindings.Clear();
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
            Windows.MainWindowCreated -= this.OnWindowCreated;
            Windows.MiniWindowCreated -= this.OnWindowCreated;
            foreach (var element in new[] { this.Play, this.Previous, this.Next, this.Stop, this.Settings, this.MiniPlayer })
            {
                if (element == null)
                {
                    continue;
                }
                element.ValueChanged -= this.OnValueChanged;
            }
            this.RemoveCommandBindings();
            this._Playback.Dispose();
            this._Settings.Dispose();
            this._MiniPlayer.Dispose();
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
