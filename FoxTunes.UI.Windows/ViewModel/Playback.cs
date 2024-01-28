using FoxTunes.Interfaces;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace FoxTunes.ViewModel
{
    public class Playback : ViewModelBase
    {
        public IBackgroundTaskRunner BackgroundTaskRunner { get; private set; }

        public IOutput Output { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public IPlaylistManager PlaylistManager { get; private set; }

        public IPlaybackManager PlaybackManager { get; private set; }

        public IInputManager InputManager { get; private set; }

        public ICommand PlayCommand
        {
            get
            {
                return new AsyncCommand(
                    this.BackgroundTaskRunner,
                    () =>
                    {
                        if (this.PlaybackManager.CurrentStream == null)
                        {
                            return this.PlaylistManager.Next();
                        }
                        else if (this.PlaybackManager.CurrentStream.IsPaused)
                        {
                            return this.PlaybackManager.CurrentStream.Resume();
                        }
                        else if (this.PlaybackManager.CurrentStream.IsStopped)
                        {
                            return this.PlaybackManager.CurrentStream.Play();
                        }
                        return Task.CompletedTask;
                    },
                    () => this.PlaybackManager != null && this.PlaylistManager != null && this.PlaylistManager.CanNavigate && (this.PlaybackManager.CurrentStream == null || (this.PlaybackManager.CurrentStream.IsPaused || this.PlaybackManager.CurrentStream.IsStopped))
                );
            }
        }

        public ICommand PauseCommand
        {
            get
            {
                return new AsyncCommand(
                    this.BackgroundTaskRunner,
                    () =>
                    {
                        if (this.PlaybackManager.CurrentStream.IsPaused)
                        {
                            return this.PlaybackManager.CurrentStream.Resume();
                        }
                        else if (this.PlaybackManager.CurrentStream.IsPlaying)
                        {
                            return this.PlaybackManager.CurrentStream.Pause();
                        }
                        return Task.CompletedTask;
                    },
                    () => this.PlaybackManager != null && this.PlaybackManager.CurrentStream != null && (this.PlaybackManager.CurrentStream.IsPlaying || this.PlaybackManager.CurrentStream.IsPaused)
                );
            }
        }

        public ICommand StopStreamCommand
        {
            get
            {
                return new AsyncCommand(
                    this.BackgroundTaskRunner,
                    () => this.PlaybackManager.CurrentStream.Stop(),
                    () => this.PlaybackManager != null && this.PlaybackManager.CurrentStream != null && this.PlaybackManager.CurrentStream.IsPlaying
                );
            }
        }

        public ICommand StopOutputCommand
        {
            get
            {
                return new AsyncCommand(
                    this.BackgroundTaskRunner,
                    () => this.PlaybackManager.Stop(),
                    () => this.PlaybackManager != null && this.Output != null && this.Output.IsStarted
                );
            }
        }

        public ICommand PreviousCommand
        {
            get
            {
                return new AsyncCommand(
                    this.BackgroundTaskRunner,
                    () => this.PlaylistManager.Previous(),
                    () => this.PlaylistManager != null && this.PlaylistManager.CanNavigate
                );
            }
        }

        public ICommand NextCommand
        {
            get
            {
                return new AsyncCommand(
                    this.BackgroundTaskRunner,
                    () => this.PlaylistManager.Next(),
                    () => this.PlaylistManager != null && this.PlaylistManager.CanNavigate
                );
            }
        }

        private string _PlayCommandBinding { get; set; }

        public string PlayCommandBinding
        {
            get
            {
                return this._PlayCommandBinding;
            }
            set
            {
                if (!string.IsNullOrEmpty(this.PlayCommandBinding))
                {
                    this.RemoveCommandBinding(this.PlayCommandBinding);
                }
                this._PlayCommandBinding = value;
                this.AddCommandBinding(this.PlayCommandBinding, () =>
                {
                    if (this.PauseCommand.CanExecute(null))
                    {
                        this.PauseCommand.Execute(null);
                    }
                    else if (this.PlayCommand.CanExecute(null))
                    {
                        this.PlayCommand.Execute(null);
                    }
                });
            }
        }

        private string _PreviousCommandBinding { get; set; }

        public string PreviousCommandBinding
        {
            get
            {
                return this._PreviousCommandBinding;
            }
            set
            {
                if (!string.IsNullOrEmpty(this.PreviousCommandBinding))
                {
                    this.RemoveCommandBinding(this.PreviousCommandBinding);
                }
                this._PreviousCommandBinding = value;
                this.AddCommandBinding(this.PreviousCommandBinding, this.PreviousCommand);
            }
        }

        private string _NextCommandBinding { get; set; }

        public string NextCommandBinding
        {
            get
            {
                return this._NextCommandBinding;
            }
            set
            {
                if (!string.IsNullOrEmpty(this.NextCommandBinding))
                {
                    this.RemoveCommandBinding(this.NextCommandBinding);
                }
                this._NextCommandBinding = value;
                this.AddCommandBinding(this.NextCommandBinding, this.NextCommand);
            }
        }

        private string _StopCommandBinding { get; set; }

        public string StopCommandBinding
        {
            get
            {
                return this._StopCommandBinding;
            }
            set
            {
                if (!string.IsNullOrEmpty(this.StopCommandBinding))
                {
                    this.RemoveCommandBinding(this.StopCommandBinding);
                }
                this._StopCommandBinding = value;
                this.AddCommandBinding(this.StopCommandBinding, this.StopOutputCommand);
            }
        }

        public override void InitializeComponent(ICore core)
        {
            this.BackgroundTaskRunner = this.Core.Components.BackgroundTaskRunner;
            this.Output = this.Core.Components.Output;
            this.PlaylistManager = this.Core.Managers.Playlist;
            this.PlaybackManager = this.Core.Managers.Playback;
            this.InputManager = ComponentRegistry.Instance.GetComponent<IInputManager>();
            this.Core.Components.Output.IsStartedChanged += (sender, e) => Command.InvalidateRequerySuggested();
            this.Configuration = this.Core.Components.Configuration;
            this.Configuration.GetElement<TextConfigurationElement>(
                InputManagerConfiguration.SECTION,
                InputManagerConfiguration.PLAY_ELEMENT
            ).ConnectValue<string>(value => this.PlayCommandBinding = value);
            this.Configuration.GetElement<TextConfigurationElement>(
                InputManagerConfiguration.SECTION,
                InputManagerConfiguration.PREVIOUS_ELEMENT
            ).ConnectValue<string>(value => this.PreviousCommandBinding = value);
            this.Configuration.GetElement<TextConfigurationElement>(
                InputManagerConfiguration.SECTION,
                InputManagerConfiguration.NEXT_ELEMENT
            ).ConnectValue<string>(value => this.NextCommandBinding = value);
            this.Configuration.GetElement<TextConfigurationElement>(
                InputManagerConfiguration.SECTION,
                InputManagerConfiguration.STOP_ELEMENT
            ).ConnectValue<string>(value => this.StopCommandBinding = value);
            this.OnCommandsChanged();
            base.InitializeComponent(core);
        }

        protected virtual void OnCommandsChanged()
        {
            this.OnPropertyChanged("PlayCommand");
            this.OnPropertyChanged("PauseCommand");
            this.OnPropertyChanged("StopStreamCommand");
            this.OnPropertyChanged("StopOutputCommand");
            this.OnPropertyChanged("PreviousCommand");
            this.OnPropertyChanged("NextCommand");
        }

        protected virtual void AddCommandBinding(string keys, ICommand command)
        {
            this.AddCommandBinding(keys, () =>
            {
                if (!command.CanExecute(null))
                {
                    return;
                }
                command.Execute(null);
            });
        }

        protected virtual void AddCommandBinding(string keys, Action action)
        {
            if (this.InputManager == null)
            {
                return;
            }
            this.InputManager.AddInputHook(keys, action);
        }

        protected virtual void RemoveCommandBinding(string keys)
        {
            if (this.InputManager == null)
            {
                return;
            }
            this.InputManager.RemoveInputHook(keys);
        }

        protected override Freezable CreateInstanceCore()
        {
            return new Playback();
        }
    }
}
