using FoxTunes.Integration;
using FoxTunes.Interfaces;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Globalization;
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

        public IScriptingRuntime ScriptingRuntime { get; private set; }

        public IScriptingContext ScriptingContext { get; private set; }

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
                if (this.Core != null)
                {
                    foreach (var column in this.Core.Components.Playlist.PlaylistColumnQuery)
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
            this.OnGridColumnsChanged();
            this.ScriptingRuntime = this.Core.Components.ScriptingRuntime;
            this.PlaylistManager = this.Core.Managers.Playlist;
            this.PlaybackManager = this.Core.Managers.Playback;
            //TODO: This is a hack in order to make the playlist's "is playing" field update.
            this.PlaybackManager.CurrentStreamChanged += (sender, e) => this.OnGridColumnsChanged();
            this.SignalEmitter = this.Core.Components.SignalEmitter;
            this.SignalEmitter.Signal += this.OnSignal;
            base.OnCoreChanged();
        }

        protected virtual void OnSignal(object sender, ISignal signal)
        {
            if (string.Equals(signal.Name, CommonSignals.PlaylistColumnsUpdated))
            {
                this.OnGridColumnsChanged();
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
            var runner = new PlaylistItemScriptRunner(this.ScriptingContext, playlistItem, script);
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
