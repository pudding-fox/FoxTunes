using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FoxTunes.ViewModel
{
    public class UIComponentSelector : ViewModelBase, IValueConverter
    {
        public static readonly UIComponentFactory Factory = ComponentRegistry.Instance.GetComponent<UIComponentFactory>();

        public IEnumerable<UIComponent> Components
        {
            get
            {
                return LayoutManager.Instance.Components;
            }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is UIComponent component)
            {
                return Factory.CreateConfiguration(component);
            }
            if (value is UIComponentConfiguration configuration)
            {
                return configuration.Component;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is UIComponent component)
            {
                return Factory.CreateConfiguration(component);
            }
            if (value is UIComponentConfiguration configuration)
            {
                return configuration.Component;
            }
            return value;
        }

        protected override Freezable CreateInstanceCore()
        {
            return new UIComponentSelector();
        }
    }
}
