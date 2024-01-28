using FoxTunes.Interfaces;
using FoxTunes.ViewModel;
using System;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Data;

namespace FoxTunes
{
    public class PlaylistGridViewColumnFactory : IDisposable
    {
        protected static ILogger Logger
        {
            get
            {
                return LogManager.Logger;
            }
        }

        public PlaylistGridViewColumnFactory(IPlaybackManager playbackManager, IScriptingRuntime scriptingRuntime)
        {
            this.PlaybackManager = playbackManager;
            this.ScriptingRuntime = scriptingRuntime;
        }

        public bool Suspended { get; private set; }

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
            gridViewColumn.DisplayMemberBinding = new PlaylistScriptBinding()
            {
                PlaybackManager = this.PlaybackManager,
                ScriptingContext = this.ScriptingContext,
                Script = column.DisplayScript
            };
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
            if (this.Suspended || this.WidthChanged == null)
            {
                return;
            }
            this.WidthChanged(this, column);
        }

        public event PlaylistColumnEventHandler WidthChanged = delegate { };

        protected virtual void OnPositionChanged(PlaylistColumn column)
        {
            if (this.Suspended || this.PositionChanged == null)
            {
                return;
            }
            this.PositionChanged(this, column);
        }

        public event PlaylistColumnEventHandler PositionChanged = delegate { };

        public void Refresh(GridViewColumn column)
        {
            this.Suspended = true;
            try
            {
                var refreshable = column as RefreshableGridViewColumn;
                if (refreshable != null)
                {
                    refreshable.Refresh();
                }
                if (double.IsNaN(column.Width))
                {
                    column.Width = column.ActualWidth;
                    column.Width = double.NaN;
                }
            }
            finally
            {
                this.Suspended = false;
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

        ~PlaylistGridViewColumnFactory()
        {
            Logger.Write(this.GetType(), LogLevel.Error, "Component was not disposed: {0}", this.GetType().Name);
            this.Dispose(true);
        }
    }

    public delegate void PlaylistColumnEventHandler(object sender, PlaylistColumn playlistColumn);
}
