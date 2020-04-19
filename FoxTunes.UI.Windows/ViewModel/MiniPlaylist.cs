using FoxTunes.Interfaces;
using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace FoxTunes.ViewModel
{
    public class MiniPlaylist : PlaylistBase, IValueConverter, IDisposable
    {
        public IScriptingContext ScriptingContext { get; private set; }

        public IConfiguration Configuration { get; private set; }

        private PlaylistItem _SelectedItem { get; set; }

        public PlaylistItem SelectedItem
        {
            get
            {
                return this._SelectedItem;
            }
            set
            {
                this._SelectedItem = value;
                this.OnSelectedItemChanged();
            }
        }

        protected virtual void OnSelectedItemChanged()
        {
            if (this.SelectedItemChanged != null)
            {
                this.SelectedItemChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("SelectedItem");
        }

        public event EventHandler SelectedItemChanged;

        private string _Script { get; set; }

        public string Script
        {
            get
            {
                return this._Script;
            }
        }

        public Task SetScript(string value)
        {
            this._Script = value;
            return this.OnScriptChanged();
        }

        protected virtual async Task OnScriptChanged()
        {
            await this.Refresh().ConfigureAwait(false);
            if (this.ScriptChanged != null)
            {
                this.ScriptChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Script");
        }

        public event EventHandler ScriptChanged;

        public override void InitializeComponent(ICore core)
        {
            base.InitializeComponent(core);
            this.ScriptingContext = this.ScriptingRuntime.CreateContext();
            this.PlaylistManager.CurrentItemChanged += this.OnCurrentItemChanged;
            this.Configuration = core.Components.Configuration;
            this.Configuration.GetElement<TextConfigurationElement>(
                MiniPlayerBehaviourConfiguration.SECTION,
                MiniPlayerBehaviourConfiguration.PLAYLIST_SCRIPT_ELEMENT
            ).ConnectValue(async value => await this.SetScript(value).ConfigureAwait(false));
        }

        protected virtual async void OnCurrentItemChanged(object sender, AsyncEventArgs e)
        {
            using (e.Defer())
            {
                await this.RefreshSelectedItem().ConfigureAwait(false);
            }
        }

        public virtual async Task Refresh()
        {
            await this.RefreshItems().ConfigureAwait(false);
            await this.RefreshSelectedItem().ConfigureAwait(false);
        }

        public virtual Task RefreshSelectedItem()
        {
            if (this.PlaybackManager == null)
            {
#if NET40
                return TaskEx.FromResult(false);
#else
                return Task.CompletedTask;
#endif
            }
            var outputStream = this.PlaybackManager.CurrentStream;
            return Windows.Invoke(() =>
            {
                if (outputStream != null)
                {
                    this.SelectedItem = outputStream.PlaylistItem;
                }
                else
                {
                    this.SelectedItem = null;
                }
            });
        }

        public override string StatusMessage
        {
            get
            {
                if (this.PlaylistBrowser != null)
                {
                    switch (this.PlaylistBrowser.State)
                    {
                        case PlaylistBrowserState.Loading:
                            return "Loading...";
                    }
                }
                if (this.PlaylistManager != null)
                {
                    if (!this.PlaylistManager.CanNavigate)
                    {
                        return "Drop files anywhere.";
                    }
                }
                return null;
            }
        }

        public ICommand PlaySelectedItemCommand
        {
            get
            {
                return CommandFactory.Instance.CreateCommand(
                    () =>
                    {
                        return this.PlaylistManager.Play(this.SelectedItem);
                    },
                    () => this.PlaylistManager != null && this.SelectedItem != null
                );
            }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var playlistItem = value as PlaylistItem;
            if (playlistItem == null)
            {
                return null;
            }
            var playlistItemScriptRunner = new PlaylistItemScriptRunner(this.ScriptingContext, playlistItem, this.Script);
            playlistItemScriptRunner.Prepare();
            return playlistItemScriptRunner.Run();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        protected override void OnDisposing()
        {
            if (this.PlaylistManager != null)
            {
                this.PlaylistManager.CurrentItemChanged -= this.OnCurrentItemChanged;
            }
            if (this.ScriptingContext != null)
            {
                this.ScriptingContext.Dispose();
            }
            base.OnDisposing();
        }

        protected override Freezable CreateInstanceCore()
        {
            return new MiniPlaylist();
        }
    }
}
