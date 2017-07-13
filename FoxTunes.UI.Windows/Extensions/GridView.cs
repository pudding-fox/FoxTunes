using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace FoxTunes.Extensions
{
    public static class GridViewExtensions
    {
        public static readonly DependencyProperty ColumnsSourceProperty = DependencyProperty.RegisterAttached(
            "ColumnsSource",
            typeof(ObservableCollection<GridViewColumn>),
            typeof(GridViewExtensions),
            new UIPropertyMetadata(null, new PropertyChangedCallback(OnColumnsSourcePropertyChanged))
        );

        public static ObservableCollection<GridViewColumn> GetColumnsSource(GridView source)
        {
            return (ObservableCollection<GridViewColumn>)source.GetValue(ColumnsSourceProperty);
        }

        public static void SetColumnsSource(GridView source, ObservableCollection<GridViewColumn> value)
        {
            source.SetValue(ColumnsSourceProperty, value);
        }

        private static void OnColumnsSourcePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var gridView = (sender as GridView);
            if (gridView == null)
            {
                return;
            }
            var columns = (e.NewValue as ObservableCollection<GridViewColumn>);
            if (columns == null)
            {
                return;
            }
            gridView.Columns.Clear();
            foreach (var column in columns)
            {
                gridView.Columns.Add(column);
            }
        }
    }
}
