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

        public event EventHandler SelectedItemChanged = delegate { };

        private string _DisplayScript { get; set; }

        public string DisplayScript
        {
            get
            {
                return this._DisplayScript;
            }
        }

        public Task SetDisplayScript(string value)
        {
            this._DisplayScript = value;
            return this.OnDisplayScriptChanged();
        }

        protected virtual async Task OnDisplayScriptChanged()
        {
            await this.Refresh();
            if (this.DisplayScriptChanged != null)
            {
                this.DisplayScriptChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("DisplayScript");
        }

        public event EventHandler DisplayScriptChanged = delegate { };

        public override void InitializeComponent(ICore core)
        {
            base.InitializeComponent(core);
            this.ScriptingContext = this.ScriptingRuntime.CreateContext();
            this.PlaylistManager.CurrentItemChanged += this.OnCurrentItemChanged;
            this.Configuration = core.Components.Configuration;
            this.Configuration.GetElement<TextConfigurationElement>(
                MiniPlayerBehaviourConfiguration.SECTION,
                MiniPlayerBehaviourConfiguration.PLAYLIST_SCRIPT_ELEMENT
            ).ConnectValue<string>(async value => await this.SetDisplayScript(value));
            var task = this.Refresh();
        }

        protected virtual async void OnCurrentItemChanged(object sender, AsyncEventArgs e)
        {
            using (e.Defer())
            {
                await this.Refresh();
            }
        }

        protected virtual Task Refresh()
        {
            return Windows.Invoke(() => this.SelectedItem = this.PlaylistManager.CurrentItem);
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
            var playlistItemScriptRunner = new PlaylistItemScriptRunner(this.ScriptingContext, playlistItem, this.DisplayScript);
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
