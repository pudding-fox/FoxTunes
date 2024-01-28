using FoxTunes.Interfaces;
using FoxTunes.ViewModel;
using System;
using System.Windows.Controls;

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

        public static PlaylistColumnManager PlaylistColumnManager = ComponentRegistry.Instance.GetComponent<PlaylistColumnManager>();

        public PlaylistGridViewColumnFactory(IPlaybackManager playbackManager, IScriptingRuntime scriptingRuntime)
        {
            this.PlaybackManager = playbackManager;
            this.ScriptingRuntime = scriptingRuntime;
        }

        public bool Suspended { get; private set; }

        public IPlaybackManager PlaybackManager { get; private set; }

        public IScriptingRuntime ScriptingRuntime { get; private set; }

        public IScriptingContext ScriptingContext { get; private set; }

        public PlaylistGridViewColumn Create(PlaylistColumn column)
        {
            this.EnsureScriptingContext();
            var gridViewColumn = new PlaylistGridViewColumn(column);
            switch (column.Type)
            {
                case PlaylistColumnType.Script:
                    gridViewColumn.DisplayMemberBinding = new PlaylistScriptBinding()
                    {
                        PlaybackManager = this.PlaybackManager,
                        ScriptingContext = this.ScriptingContext,
                        Script = column.Script
                    };
                    break;
                case PlaylistColumnType.Plugin:
                    var provider = PlaylistColumnManager.GetProvider(column.Plugin);
                    if (provider != null)
                    {
                        gridViewColumn.CellTemplate = provider.CellTemplate;
                    }
                    break;
            }
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

        public event PlaylistColumnEventHandler WidthChanged;

        protected virtual void OnPositionChanged(PlaylistColumn column)
        {
            if (this.Suspended || this.PositionChanged == null)
            {
                return;
            }
            this.PositionChanged(this, column);
        }

        public event PlaylistColumnEventHandler PositionChanged;

        public void Refresh(PlaylistGridViewColumn column)
        {
            this.Suspended = true;
            try
            {
                column.Refresh();
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

    public delegate void PlaylistColumnEventHandler(object sender, PlaylistColumn playlistColumn);
}
