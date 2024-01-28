using FoxTunes.Interfaces;
using FoxTunes.ViewModel;
using System;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Data;

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
            BindingHelper.Create(
                gridViewColumn,
                GridViewColumn.WidthProperty,
                typeof(GridViewColumn),
                column,
                "Width",
                ColumnWidthConverter.Instance,
                (sender, e) => this.OnWidthChanged(column)
            );
            BindingHelper.Create(
                gridViewColumn,
                GridViewColumnExtensions.PositionProperty,
                typeof(GridViewColumn),
                column,
                "Sequence",
                (sender, e) => this.OnPositionChanged(column)
            );
            return gridViewColumn;
        }

        protected virtual void OnWidthChanged(PlaylistColumn column)
        {
            if (this.WidthChanged == null)
            {
                return;
            }
            this.WidthChanged(this, column);
        }

        public event EventHandler<PlaylistColumn> WidthChanged = delegate { };

        protected virtual void OnPositionChanged(PlaylistColumn column)
        {
            if (this.PositionChanged == null)
            {
                return;
            }
            this.PositionChanged(this, column);
        }

        public event EventHandler<PlaylistColumn> PositionChanged = delegate { };

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
