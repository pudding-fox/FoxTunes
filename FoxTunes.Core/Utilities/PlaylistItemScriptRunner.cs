using FoxTunes.Interfaces;
using System.Collections.Generic;
using System.Diagnostics;

namespace FoxTunes
{
    public class PlaylistItemScriptRunner
    {
        public PlaylistItemScriptRunner(IPlaybackManager playbackManager, IScriptingContext scriptingContext, PlaylistItem playlistItem, string script)
        {
            this.PlaybackManager = playbackManager;
            this.ScriptingContext = scriptingContext;
            this.PlaylistItem = playlistItem;
            this.Script = script;
        }

        public IPlaybackManager PlaybackManager { get; private set; }

        public IScriptingContext ScriptingContext { get; private set; }

        public PlaylistItem PlaylistItem { get; private set; }

        public string Script { get; private set; }

        public void Prepare()
        {
            var metaData = new Dictionary<string, object>();
            foreach (var item in this.PlaylistItem.MetaDatas)
            {
                var key = item.Name.ToLower();
                if (metaData.ContainsKey(key))
                {
                    //Not sure what to do. Doesn't happen often.
                    continue;
                }
                metaData.Add(key, item.Value);
            }

            var properties = new Dictionary<string, object>();
            foreach (var item in this.PlaylistItem.Properties)
            {
                properties.Add(item.Name.ToLower(), item.Value);
            }

            this.ScriptingContext.SetValue("playing", this.PlaybackManager.CurrentStream);
            this.ScriptingContext.SetValue("item", this.PlaylistItem);
            this.ScriptingContext.SetValue("tag", metaData);
            this.ScriptingContext.SetValue("stat", properties);
        }

        [DebuggerNonUserCode]
        public object Run()
        {
            const string RESULT = "__result";
            try
            {
                this.ScriptingContext.Run(string.Concat("var ", RESULT, " = ", this.Script, ";"));
            }
            catch (ScriptingException e)
            {
                return e.Message;
            }
            return this.ScriptingContext.GetValue(RESULT);
        }
    }
}
