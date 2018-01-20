using FoxTunes.Interfaces;
using System.Windows.Controls;

namespace FoxTunes.Utilities
{
    public class PlaylistGridViewColumnFactory
    {
        public PlaylistGridViewColumnFactory(IPlaybackManager playbackManager, IScriptingRuntime scriptingRuntime)
        {
            this.PlaybackManager = playbackManager;
            this.ScriptingRuntime = scriptingRuntime;
        }

        public IPlaybackManager PlaybackManager { get; private set; }

        public IScriptingRuntime ScriptingRuntime { get; private set; }

        public IScriptingContext ScriptingContext { get; private set; }

        public GridViewColumn Create(PlaylistColumn column)
        {
            this.EnsureScriptingContext();
            var gridViewColumn = default(GridViewColumn);
            if (column.IsDynamic)
            {
                gridViewColumn = new RefreshableGridViewColumn();
            }
            else
            {
                gridViewColumn = new GridViewColumn();
            }
            gridViewColumn.Header = column.Name;
            gridViewColumn.DisplayMemberBinding = new PlaylistScriptBinding(this.PlaybackManager, this.ScriptingContext, column.DisplayScript);
            gridViewColumn.Width = column.Width.HasValue ? column.Width.Value : double.NaN;
            return gridViewColumn;
        }

        public void Refresh(GridViewColumn column)
        {
            var refreshable = column as RefreshableGridViewColumn;
            if (refreshable != null)
            {
                refreshable.Refresh();
            }
        }

        protected virtual void EnsureScriptingContext()
        {
            if (this.ScriptingContext != null)
            {
                return;
            }
            this.ScriptingContext = this.ScriptingRuntime.CreateContext();
        }
    }
}
