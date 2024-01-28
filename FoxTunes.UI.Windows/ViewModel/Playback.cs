using FoxTunes.Interfaces;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace FoxTunes.ViewModel
{
    public class Playback : ViewModelBase
    {
        public IOutput Output { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public IPlaylistManager PlaylistManager { get; private set; }

        public IPlaybackManager PlaybackManager { get; private set; }

        public IInputManager InputManager { get; private set; }

        public ICommand PlayCommand
        {
            get
            {
                return CommandFactory.Instance.CreateCommand(
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
#if NET40
                        return TaskEx.FromResult(false);
#else
                        return Task.CompletedTask;
#endif
                    }
                );
            }
        }

        public ICommand PauseCommand
        {
            get
            {
                return CommandFactory.Instance.CreateCommand(
                    () =>
                    {
                        if (this.PlaybackManager.CurrentStream != null)
                        {
                            if (this.PlaybackManager.CurrentStream.IsPaused)
                            {
                                return this.PlaybackManager.CurrentStream.Resume();
                            }
                            else if (this.PlaybackManager.CurrentStream.IsPlaying)
                            {
                                return this.PlaybackManager.CurrentStream.Pause();
                            }
                        }
#if NET40
                        return TaskEx.FromResult(false);
#else
                        return Task.CompletedTask;
#endif
                    }
                );
            }
        }

        public ICommand StopStreamCommand
        {
            get
            {
                return CommandFactory.Instance.CreateCommand(
                    () =>
                    {
                        if (this.PlaybackManager.CurrentStream != null)
                        {
                            return this.PlaybackManager.CurrentStream.Stop();
                        }
#if NET40
                        return TaskEx.FromResult(false);
#else
                        return Task.CompletedTask;
#endif
                    }
                );
            }
        }

        public ICommand StopOutputCommand
        {
            get
            {
                return CommandFactory.Instance.CreateCommand(
                    () => this.PlaybackManager.Stop()
                );
            }
        }

        public ICommand PreviousCommand
        {
            get
            {
                return CommandFactory.Instance.CreateCommand(
                    () => this.PlaylistManager.Previous()
                );
            }
        }

        public ICommand NextCommand
        {
            get
            {
                return CommandFactory.Instance.CreateCommand(
                    () => this.PlaylistManager.Next()
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
                    if (this.PlaybackManager.CurrentStream != null)
                    {
                        if (this.PlaybackManager.CurrentStream.IsPaused)
                        {
                            var task = this.PlaybackManager.CurrentStream.Resume();
                        }
                        else if (this.PlaybackManager.CurrentStream.IsPlaying)
                        {
                            var task = this.PlaybackManager.CurrentStream.Pause();
                        }
                    }
                    else
                    {
                        var task = this.PlaylistManager.Next();
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
            this.Output = this.Core.Components.Output;
            this.PlaylistManager = this.Core.Managers.Playlist;
            this.PlaybackManager = this.Core.Managers.Playback;
            this.InputManager = ComponentRegistry.Instance.GetComponent<IInputManager>();
            this.Configuration = this.Core.Components.Configuration;
            if (this.Configuration.GetSection(InputManagerConfiguration.SECTION) != null)
            {
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
            }
            base.InitializeComponent(core);
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
            try
            {
                this.InputManager.AddInputHook(keys, action);
            }
            catch (Exception e)
            {
                this.OnError(string.Format("Failed to register input hook {0}: {1}", keys, e.Message), e);
            }
        }

        protected virtual void RemoveCommandBinding(string keys)
        {
            if (this.InputManager == null)
            {
                return;
            }
            try
            {
                this.InputManager.RemoveInputHook(keys);
            }
            catch
            {
                //Nothing can be done.
            }
        }

        protected override void OnDisposing()
        {
            if (!string.IsNullOrEmpty(this.PlayCommandBinding))
            {
                this.RemoveCommandBinding(this.PlayCommandBinding);
            }
            if (!string.IsNullOrEmpty(this.PreviousCommandBinding))
            {
                this.RemoveCommandBinding(this.PreviousCommandBinding);
            }
            if (!string.IsNullOrEmpty(this.NextCommandBinding))
            {
                this.RemoveCommandBinding(this.NextCommandBinding);
            }
            if (!string.IsNullOrEmpty(this.StopCommandBinding))
            {
                this.RemoveCommandBinding(this.StopCommandBinding);
            }
            base.OnDisposing();
        }

        protected override Freezable CreateInstanceCore()
        {
            return new Playback();
        }
    }
}
