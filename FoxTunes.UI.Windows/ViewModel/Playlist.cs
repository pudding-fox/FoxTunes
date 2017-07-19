using FoxTunes.Integration;
using FoxTunes.Interfaces;
using FoxTunes.Utilities;
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

        public IScriptingContext ScriptingContext { get; set; }

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
            this.ScriptingContext = this.Core.Components.ScriptingRuntime.CreateContext();
        }

        protected override void OnCoreChanged()
        {
            //TODO: This is a hack in order to make the playlist's "is playing" field update.
            this.Core.Managers.Playback.CurrentStreamChanged += (sender, e) => this.OnPropertyChanged("GridColumns");
            base.OnCoreChanged();
        }

        public ICommand PlaySelectedItemCommand
        {
            get
            {
                return new Command<IPlaybackManager>(playback =>
                {
                    var item = this.SelectedItems[0] as PlaylistItem;
                    playback.Load(item.FileName).Play();
                },
                playback => playback != null && this.SelectedItems.Count > 0);
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
                return new Command(() => this.Core.Managers.Playlist.Clear());
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
            this.EnsureScriptingContext();
            var runner = new PlaylistItemScriptRunner(this.ScriptingContext, playlistItem, script);
            runner.Prepare();
            return runner.Run();
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
