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
            this.PlaylistColumns = new ObservableCollection<PlaylistColumn>();
        }

        public IScriptingRuntime ScriptingRuntime { get; private set; }

        public IScriptingContext ScriptingContext { get; private set; }

        public IPlaybackManager PlaybackManager { get; private set; }

        public IPlaylistManager PlaylistManager { get; private set; }

        public IList SelectedItems { get; set; }

        public ObservableCollection<PlaylistColumn> PlaylistColumns { get; set; }

        public ObservableCollection<GridViewColumn> GridColumns
        {
            get
            {
                var columns = new ObservableCollection<GridViewColumn>();
                foreach (var column in this.PlaylistColumns)
                {
                    columns.Add(new GridViewColumn()
                    {
                        Header = column.Header,
                        DisplayMemberBinding = new Binding()
                        {
                            Converter = this,
                            ConverterParameter = column.Script
                        }
                    });
                }
                return columns;
            }
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
            this.ScriptingRuntime = this.Core.Components.ScriptingRuntime;
            this.PlaylistManager = this.Core.Managers.Playlist;
            this.PlaybackManager = this.Core.Managers.Playback;
            //TODO: This is a hack in order to make the playlist's "is playing" field update.
            this.PlaybackManager.CurrentStreamChanged += (sender, e) => this.OnPropertyChanged("GridColumns");
            base.OnCoreChanged();
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

        protected override Freezable CreateInstanceCore()
        {
            return new Playlist();
        }
    }
}
