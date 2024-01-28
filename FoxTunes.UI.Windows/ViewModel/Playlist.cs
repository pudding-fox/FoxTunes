using FoxTunes.Integration;
using FoxTunes.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace FoxTunes.ViewModel
{
    public class Playlist : ViewModelBase, IValueConverter
    {
        public Playlist()
        {
            this.SelectedItems = new ObservableCollection<PlaylistItem>();
        }

        public IForegroundTaskRunner ForegroundTaskRunner { get; private set; }

        public IScriptingRuntime ScriptingRuntime { get; private set; }

        public IScriptingContext ScriptingContext { get; private set; }

        public IDataManager DataManager { get; private set; }

        public IPlaybackManager PlaybackManager { get; private set; }

        public IPlaylistManager PlaylistManager { get; private set; }

        public ISignalEmitter SignalEmitter { get; private set; }

        public IList SelectedItems { get; set; }

        private bool _InsertActive { get; set; }

        public bool InsertActive
        {
            get
            {
                return this._InsertActive;
            }
            set
            {
                this._InsertActive = value;
                this.OnInsertActiveChanged();
            }
        }

        protected virtual void OnInsertActiveChanged()
        {
            if (this.InsertActiveChanged != null)
            {
                this.InsertActiveChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("InsertActive");
        }

        public event EventHandler InsertActiveChanged = delegate { };

        private int _InsertIndex { get; set; }

        public int InsertIndex
        {
            get
            {
                return this._InsertIndex;
            }
            set
            {
                this._InsertIndex = value;
                this.OnInsertIndexChanged();
            }
        }

        protected virtual void OnInsertIndexChanged()
        {
            if (this.InsertIndexChanged != null)
            {
                this.InsertIndexChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("InsertIndex");
        }

        public event EventHandler InsertIndexChanged = delegate { };

        private int _InsertOffset { get; set; }

        public int InsertOffset
        {
            get
            {
                return this._InsertOffset;
            }
            set
            {
                this._InsertOffset = value;
                this.OnInsertOffsetChanged();
            }
        }

        protected virtual void OnInsertOffsetChanged()
        {
            if (this.InsertOffsetChanged != null)
            {
                this.InsertOffsetChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("InsertOffset");
        }

        public event EventHandler InsertOffsetChanged = delegate { };

        public ObservableCollection<GridViewColumn> GridColumns
        {
            get
            {
                var columns = new ObservableCollection<GridViewColumn>();
                if (this.DataManager != null)
                {
                    foreach (var column in this.DataManager.ReadContext.Sets.PlaylistColumn)
                    {
                        columns.Add(new GridViewColumn()
                        {
                            Header = column.Name,
                            DisplayMemberBinding = new Binding()
                            {
                                Converter = this,
                                ConverterParameter = column.DisplayScript
                            },
                            Width = column.Width.HasValue ? column.Width.Value : double.NaN
                        });
                    }
                }
                return columns;
            }
        }

        protected virtual void OnGridColumnsChanged()
        {
            if (this.GridColumnsChanged != null)
            {
                this.GridColumnsChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("GridColumns");
        }

        public event EventHandler GridColumnsChanged = delegate { };

        public void Reload()
        {
            this.OnGridColumnsChanged();
        }

        private void EnsureScriptingContext()
        {
            if (this.ScriptingContext != null)
            {
                return;
            }
            this.ScriptingContext = this.ScriptingRuntime.CreateContext();
        }

        protected override void OnCoreChanged()
        {
            this.ForegroundTaskRunner = this.Core.Components.ForegroundTaskRunner;
            this.ScriptingRuntime = this.Core.Components.ScriptingRuntime;
            this.DataManager = this.Core.Managers.Data;
            this.PlaylistManager = this.Core.Managers.Playlist;
            this.PlaybackManager = this.Core.Managers.Playback;
            //TODO: This is a hack in order to make the playlist's "is playing" field update.
            this.PlaybackManager.CurrentStreamChanged += (sender, e) => this.OnGridColumnsChanged();
            this.SignalEmitter = this.Core.Components.SignalEmitter;
            this.SignalEmitter.Signal += this.OnSignal;
            this.OnGridColumnsChanged();
            base.OnCoreChanged();
        }

        protected virtual void OnSignal(object sender, ISignal signal)
        {
            switch (signal.Name)
            {
                case CommonSignals.PlaylistColumnsUpdated:
                    this.ForegroundTaskRunner.Run(this.Reload);
                    break;
            }
        }

        public ICommand PlaySelectedItemCommand
        {
            get
            {
                return new AsyncCommand(() =>
                {
                    var playlistItem = this.SelectedItems[0] as PlaylistItem;
                    return this.PlaylistManager.Play(playlistItem);
                },
                () => this.PlaybackManager != null && this.SelectedItems.Count > 0);
            }
        }

        public ICommand LocateCommand
        {
            get
            {
                return new Command(() =>
                {
                    var item = this.SelectedItems[0] as PlaylistItem;
                    Explorer.Select(item.FileName);
                },
                () => this.SelectedItems.Count > 0);
            }
        }

        public ICommand ClearCommand
        {
            get
            {
                return new AsyncCommand(() => this.Core.Managers.Playlist.Clear());
            }
        }

        public ICommand SettingsCommand
        {
            get
            {
                return new Command(() => this.SettingsVisible = true);
            }
        }

        public ICommand DragEnterCommand
        {
            get
            {
                return new Command<DragEventArgs>(this.OnDragEnter);
            }
        }

        protected virtual void OnDragEnter(DragEventArgs e)
        {
            var effects = DragDropEffects.None;
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                effects = DragDropEffects.Copy;
            }
            if (e.Data.GetDataPresent(typeof(LibraryHierarchyNode)))
            {
                effects = DragDropEffects.Copy;
            }
            e.Effects = effects;
        }

        public ICommand DropCommand
        {
            get
            {
                return new AsyncCommand<DragEventArgs>(this.OnDrop);
            }
        }

        protected virtual Task OnDrop(DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var paths = e.Data.GetData(DataFormats.FileDrop) as IEnumerable<string>;
                return this.AddToPlaylist(paths);
            }
            if (e.Data.GetDataPresent(typeof(LibraryHierarchyNode)))
            {
                var libraryHierarchyNode = e.Data.GetData(typeof(LibraryHierarchyNode)) as LibraryHierarchyNode;
                return this.AddToPlaylist(libraryHierarchyNode);
            }
            return Task.CompletedTask;
        }

        private Task AddToPlaylist(IEnumerable<string> paths)
        {
            var sequence = default(int);
            if (this.TryGetInsertSequence(out sequence))
            {
                return this.Core.Managers.Playlist.Insert(sequence, paths);
            }
            else
            {
                return this.Core.Managers.Playlist.Add(paths);
            }
        }

        private Task AddToPlaylist(LibraryHierarchyNode libraryHierarchyNode)
        {
            var sequence = default(int);
            if (this.TryGetInsertSequence(out sequence))
            {
                return this.Core.Managers.Playlist.Insert(sequence, libraryHierarchyNode);
            }
            else
            {
                return this.Core.Managers.Playlist.Add(libraryHierarchyNode);
            }
        }

        protected virtual bool TryGetInsertSequence(out int sequence)
        {
            if (!this.InsertActive)
            {
                sequence = 0;
                return false;
            }
            sequence = this.InsertIndex + this.InsertOffset;
            return true;
        }

        #region IValueConverter

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var playlistItem = value as PlaylistItem;
            if (playlistItem == null)
            {
                return null;
            }
            var script = parameter as string;
            if (string.IsNullOrEmpty(script))
            {
                return null;
            }
            return this.ExecuteScript(playlistItem, script);
        }

        private string ExecuteScript(PlaylistItem playlistItem, string script)
        {
            this.EnsureScriptingContext();
            var runner = new PlaylistItemScriptRunner(this.PlaybackManager, this.ScriptingContext, playlistItem, script);
            runner.Prepare();
            return global::System.Convert.ToString(runner.Run());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion

        private bool _SettingsVisible { get; set; }

        public bool SettingsVisible
        {
            get
            {
                return this._SettingsVisible;
            }
            set
            {
                this._SettingsVisible = value;
                this.OnSettingsVisibleChanged();
            }
        }

        protected virtual void OnSettingsVisibleChanged()
        {
            if (this.SettingsVisibleChanged != null)
            {
                this.SettingsVisibleChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("SettingsVisible");
        }

        public event EventHandler SettingsVisibleChanged = delegate { };

        protected override Freezable CreateInstanceCore()
        {
            return new Playlist();
        }
    }
}
