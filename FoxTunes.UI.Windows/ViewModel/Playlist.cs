using FoxTunes.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var item = value as PlaylistItem;
            if (item == null)
            {
                return null;
            }
            var script = parameter as string;
            if (string.IsNullOrEmpty(script))
            {
                return null;
            }
            this.EnsureScriptingContext();
            this.ScriptingContext.SetValue("playing", value);
            const string RESULT = "__result";
            try
            {
                foreach (var metaData in item.MetaData)
                {
                    this.ScriptingContext.SetValue(metaData.Name.ToLower(), metaData.Value);
                }
                this.ScriptingContext.Run(string.Concat("var ", RESULT, " = ", script, ";"));
            }
            finally
            {
                foreach (var metaData in item.MetaData)
                {
                    this.ScriptingContext.SetValue(metaData.Name.ToLower(), null);
                }
            }
            return this.ScriptingContext.GetValue(RESULT);
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
    }
}
