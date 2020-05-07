using System;
using System.Globalization;
using System.Windows.Data;

namespace FoxTunes.ViewModel
{
    public class LibraryBrowserImageConverter : IValueConverter
    {
        public static readonly LibraryBrowserTileBrushFactory Factory = ComponentRegistry.Instance.GetComponent<LibraryBrowserTileBrushFactory>();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is LibraryHierarchyNode libraryHierarchyNode))
            {
                return value;
            }
            return Factory.Create(libraryHierarchyNode);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
