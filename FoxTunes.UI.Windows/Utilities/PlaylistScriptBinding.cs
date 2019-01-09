using FoxTunes.Interfaces;
using System;
using System.Globalization;

namespace FoxTunes
{
    public class PlaylistScriptBinding : ScriptBinding
    {
        public PlaylistScriptBinding()
        {

        }

        public PlaylistScriptBinding(IPlaybackManager playbackManager, IScriptingContext scriptingContext, string script)
            : base(scriptingContext, script)
        {
            this.PlaybackManager = playbackManager;
        }

        private IPlaybackManager _PlaybackManager { get; set; }

        public IPlaybackManager PlaybackManager
        {
            get
            {
                return this._PlaybackManager;
            }
            set
            {
                this._PlaybackManager = value;
                this.OnPlaybackManagerChanged();
            }
        }

        protected virtual void OnPlaybackManagerChanged()
        {
            if (this.PlaybackManagerChanged != null)
            {
                this.PlaybackManagerChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("PlaybackManager");
        }

        public event EventHandler PlaybackManagerChanged = delegate { };

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var playlistItem = value as PlaylistItem;
            if (playlistItem == null)
            {
                return null;
            }
            if (string.IsNullOrEmpty(this.Script))
            {
                return null;
            }
            return this.ExecuteScript(playlistItem, this.Script);
        }

        private object ExecuteScript(PlaylistItem playlistItem, string script)
        {
            var runner = new PlaylistItemScriptRunner(this.PlaybackManager, this.ScriptingContext, playlistItem, script);
            runner.Prepare();
            return runner.Run();
        }
    }
}
