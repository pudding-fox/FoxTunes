using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;

namespace FoxTunes
{
    public class PlaylistItemScriptComparer : BaseComponent, IComparer<PlaylistItem>, IDisposable
    {
        public PlaylistItemScriptComparer(string script)
        {
            this.Script = script;
        }

        public string Script { get; private set; }

        public IScriptingRuntime ScriptingRuntime { get; private set; }

        public IScriptingContext ScriptingContext { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.ScriptingRuntime = core.Components.ScriptingRuntime;
            this.ScriptingContext = this.ScriptingRuntime.CreateContext();
            base.InitializeComponent(core);
        }

        public int Compare(PlaylistItem playlistItem1, PlaylistItem playlistItem2)
        {
            var value1 = default(string);
            var value2 = default(string);
            {
                var runner = new PlaylistItemScriptRunner(this.ScriptingContext, playlistItem1, this.Script);
                runner.Prepare();
                value1 = Convert.ToString(runner.Run());
            }
            {
                var runner = new PlaylistItemScriptRunner(this.ScriptingContext, playlistItem2, this.Script);
                runner.Prepare();
                value2 = Convert.ToString(runner.Run());
            }
            return string.Compare(value1, value2, StringComparison.OrdinalIgnoreCase);
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
            if (this.ScriptingContext != null)
            {
                this.ScriptingContext.Dispose();
            }
        }

        ~PlaylistItemScriptComparer()
        {
            Logger.Write(this.GetType(), LogLevel.Error, "Component was not disposed: {0}", this.GetType().Name);
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
