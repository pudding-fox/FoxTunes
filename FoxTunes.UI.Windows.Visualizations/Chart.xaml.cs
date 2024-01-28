using System.Windows;
using System.Windows.Controls;

namespace FoxTunes
{
    /// <summary>
    /// Interaction logic for Chart.xaml
    /// </summary>
    public partial class Chart : UserControl
    {
        public static readonly DependencyProperty XAxisContentProperty = DependencyProperty.Register(
            "XAxisContent",
            typeof(object),
            typeof(Chart)
       );

        public static object GetXAxisContent(Chart source)
        {
            return source.GetValue(XAxisContentProperty);
        }

        public static void SetXAxisContent(Chart source, object value)
        {
            source.SetValue(XAxisContentProperty, value);
        }

        public static readonly DependencyProperty YAxisContentProperty = DependencyProperty.Register(
            "YAxisContent",
            typeof(object),
            typeof(Chart)
       );

        public static object GetYAxisContent(Chart source)
        {
            return source.GetValue(YAxisContentProperty);
        }

        public static void SetYAxisContent(Chart source, object value)
        {
            source.SetValue(YAxisContentProperty, value);
        }

        public static readonly DependencyProperty LegendContentProperty = DependencyProperty.Register(
            "LegendContent",
            typeof(object),
            typeof(Chart)
       );

        public static object GetLegendContent(Chart source)
        {
            return source.GetValue(LegendContentProperty);
        }

        public static void SetLegendContent(Chart source, object value)
        {
            source.SetValue(LegendContentProperty, value);
        }

        public Chart()
        {
            this.InitializeComponent();
        }


        public object XAxisContent
        {
            get
            {
                return this.GetValue(XAxisContentProperty);
            }
            set
            {
                this.SetValue(XAxisContentProperty, value);
            }
        }

        public object YAxisContent
        {
            get
            {
                return this.GetValue(YAxisContentProperty);
            }
            set
            {
                this.SetValue(YAxisContentProperty, value);
            }
        }

        public object LegendContent
        {
            get
            {
                return this.GetValue(LegendContentProperty);
            }
            set
            {
                this.SetValue(LegendContentProperty, value);
            }
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            var xAxis = this.Template.FindName("XAxis", this) as UIElement;
            var yAxis = this.Template.FindName("YAxis", this) as UIElement;
            var legend = this.Template.FindName("Legend", this) as UIElement;
            if (xAxis != null && yAxis != null && legend != null)
            {
                if (!double.IsNaN(sizeInfo.NewSize.Height) && sizeInfo.NewSize.Height >= 300)
                {
                    xAxis.Visibility = Visibility.Visible;
                }
                else
                {
                    xAxis.Visibility = Visibility.Collapsed;
                }
                if (!double.IsNaN(sizeInfo.NewSize.Width) && sizeInfo.NewSize.Width >= 300)
                {
                    yAxis.Visibility = Visibility.Visible;
                }
                else
                {
                    yAxis.Visibility = Visibility.Collapsed;
                }
                if (xAxis.Visibility == Visibility.Visible && yAxis.Visibility == Visibility.Visible)
                {
                    legend.Visibility = Visibility.Visible;
                }
                else
                {
                    legend.Visibility = Visibility.Collapsed;
                }
            }
            base.OnRenderSizeChanged(sizeInfo);
        }
    }
}