using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace FoxTunes.Config
{
    /// <summary>
    /// Interaction logic for DoubleConfigurationElement.xaml
    /// </summary>
    public partial class DoubleConfigurationElement : UserControl
    {
        public DoubleConfigurationElement()
        {
            this.InitializeComponent();
        }

        protected virtual void OnDragCompleted(object sender, RoutedEventArgs e)
        {
            var expression = BindingOperations.GetBindingExpression(this.Slider, Slider.ValueProperty);
            if (expression != null)
            {
                expression.UpdateSource();
            }
        }
    }
}
