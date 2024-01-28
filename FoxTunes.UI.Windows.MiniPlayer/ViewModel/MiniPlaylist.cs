using FoxTunes.Interfaces;
using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace FoxTunes.ViewModel
{
    public class MiniPlaylist : PlaylistBase, IValueConverter
    {
        protected override string EMPTY
        {
            get
            {
                return Strings.MiniPlaylist_Empty;
            }
        }

        public Playlist CurrentPlaylist { get; private set; }

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

        protected override Playlist GetPlaylist()
        {
            var playlist = this.PlaylistManager.CurrentPlaylist ?? this.PlaylistManager.SelectedPlaylist;
            return playlist;
        }

        protected override void InitializeComponent(ICore core)
        {
            base.InitializeComponent(core);
            this.ScriptingContext = this.ScriptingRuntime.CreateContext();
            this.PlaylistManager.CurrentPlaylistChanged += this.OnCurrentPlaylistChanged;
            this.PlaylistManager.SelectedPlaylistChanged += this.OnSelectedPlaylistChanged;
            this.PlaylistManager.CurrentItemChanged += this.OnCurrentItemChanged;
            this.Configuration = core.Components.Configuration;
            this.Configuration.GetElement<TextConfigurationElement>(
                MiniPlayerBehaviourConfiguration.SECTION,
                MiniPlayerBehaviourConfiguration.PLAYLIST_SCRIPT_ELEMENT
            ).ConnectValue(async value => await this.SetScript(value).ConfigureAwait(false));
        }

        protected virtual void OnCurrentPlaylistChanged(object sender, EventArgs e)
        {
            var task = this.RefreshIfRequired();
        }

        protected virtual void OnSelectedPlaylistChanged(object sender, EventArgs e)
        {
            var task = this.RefreshIfRequired();
        }

        protected virtual void OnCurrentItemChanged(object sender, EventArgs e)
        {
            //Critical: Don't block in this event handler, it causes a deadlock.
            this.Dispatch(this.RefreshSelectedItem);
        }

        protected virtual Task RefreshIfRequired()
        {
            var playlist = this.PlaylistManager.CurrentPlaylist ?? this.PlaylistManager.SelectedPlaylist;
            if (object.ReferenceEquals(this.CurrentPlaylist, playlist))
            {
#if NET40
                return TaskEx.FromResult(false);
#else
                return Task.CompletedTask;
#endif
            }
            return this.Refresh();
        }

        public override async Task Refresh()
        {
            this.CurrentPlaylist = this.PlaylistManager.CurrentPlaylist ?? this.PlaylistManager.SelectedPlaylist;
            await base.Refresh().ConfigureAwait(false);
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
                this.PlaylistManager.CurrentPlaylistChanged -= this.OnCurrentPlaylistChanged;
                this.PlaylistManager.SelectedPlaylistChanged -= this.OnSelectedPlaylistChanged;
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
