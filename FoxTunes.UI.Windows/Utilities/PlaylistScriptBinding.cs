using FoxTunes.Interfaces;
using System;
using System.Globalization;

namespace FoxTunes.Utilities
{
    public class PlaylistScriptBinding : ScriptBinding
    {
        public PlaylistScriptBinding(IPlaybackManager playbackManager, IScriptingContext scriptingContext, string script) : base(scriptingContext, script)
        {
            this.PlaybackManager = playbackManager;
        }

        public IPlaybackManager PlaybackManager { get; private set; }

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
