using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace FoxTunes.ViewModel
{
    public class Playlist : ViewModelBase, IValueConverter
    {
        public Playlist()
        {
            this.PlaylistColumns = new ObservableCollection<PlaylistColumn>();
        }

        public IScriptingContext ScriptingContext { get; set; }

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

        protected override void OnCoreChanged()
        {
            this.Core.Managers.Playback.CurrentStreamChanged += (sender, e) => this.OnPropertyChanged("GridColumns");
            base.OnCoreChanged();
        }

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
            var runner = new PlaylistItemScriptRunner(this, playlistItem, script);
            runner.Prepare();
            return runner.Run();
        }

        private void EnsureScriptingContext()
        {
            if (this.ScriptingContext != null)
            {
                return;
            }
            this.ScriptingContext = this.Core.Components.ScriptingRuntime.CreateContext();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        protected override Freezable CreateInstanceCore()
        {
            return new Playlist();
        }

        private class PlaylistItemScriptRunner
        {
            public PlaylistItemScriptRunner(Playlist playlist, PlaylistItem playlistItem, string script)
            {
                this.Playlist = playlist;
                this.PlaylistItem = playlistItem;
                this.Script = script;
            }

            public Playlist Playlist{ get; private set; }

            public PlaylistItem PlaylistItem { get; private set; }

            public string Script { get; private set; }

            public void Prepare()
            {
                var metaData = new Dictionary<string, object>();
                foreach (var item in this.PlaylistItem.MetaDatas)
                {
                    metaData.Add(item.Name.ToLower(), item.Value);
                }

                var properties = new Dictionary<string, object>();
                foreach (var item in this.PlaylistItem.Properties)
                {
                    properties.Add(item.Name.ToLower(), item.Value);
                }
                this.Playlist.ScriptingContext.SetValue("item", this.PlaylistItem);
                this.Playlist.ScriptingContext.SetValue("playing", this.Playlist.Core.Components.Playlist.SelectedItem);
                this.Playlist.ScriptingContext.SetValue("tag", metaData);
                this.Playlist.ScriptingContext.SetValue("stat", properties);
            }

            public object Run()
            {
                const string RESULT = "__result";
                try
                {
                    this.Playlist.ScriptingContext.Run(string.Concat("var ", RESULT, " = ", this.Script, ";"));
                }
                catch (Exception e)
                {
                    return e;
                }
                return this.Playlist.ScriptingContext.GetValue(RESULT);
            }
        }
    }
}
