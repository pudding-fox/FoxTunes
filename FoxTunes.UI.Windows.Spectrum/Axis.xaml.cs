using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FoxTunes
{
    /// <summary>
    /// Interaction logic for Axis.xaml
    /// </summary>
    public partial class Axis : UserControl
    {
        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(
            "Orientation",
            typeof(Orientation),
            typeof(Axis),
            new PropertyMetadata(Orientation.Horizontal, new PropertyChangedCallback(OnOrientationChanged))
        );

        public static Orientation GetOrientation(Axis source)
        {
            return (Orientation)source.GetValue(OrientationProperty);
        }

        public static void SetOrientation(Axis source, Orientation value)
        {
            source.SetValue(OrientationProperty, value);
        }

        public static void OnOrientationChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var axis = sender as Axis;
            if (axis == null)
            {
                return;
            }
            axis.OnOrientationChanged();
        }

        public static readonly DependencyProperty LabelProviderProperty = DependencyProperty.Register(
            "LabelProvider",
            typeof(AxisLabelProvider),
            typeof(Axis),
            new PropertyMetadata(null, new PropertyChangedCallback(OnLabelProviderChanged))
        );

        public static AxisLabelProvider GetLabelProvider(Axis source)
        {
            return (AxisLabelProvider)source.GetValue(LabelProviderProperty);
        }

        public static void SetLabelProvider(Axis source, AxisLabelProvider value)
        {
            source.SetValue(LabelProviderProperty, value);
        }

        public static void OnLabelProviderChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var axis = sender as Axis;
            if (axis == null)
            {
                return;
            }
            axis.OnLabelProviderChanged();
        }

        public Axis()
        {
            this.LabelProvider = new RangeAxisLabelProvider();
            this.InitializeComponent();
        }

        public Orientation Orientation
        {
            get
            {
                return GetOrientation(this);
            }
            set
            {
                SetOrientation(this, value);
            }
        }

        protected virtual void OnOrientationChanged()
        {
            if (this.OrientationChanged != null)
            {
                this.OrientationChanged(this, EventArgs.Empty);
            }
        }

        public event EventHandler OrientationChanged;

        public AxisLabelProvider LabelProvider
        {
            get
            {
                return GetLabelProvider(this);
            }
            set
            {
                SetLabelProvider(this, value);
            }
        }

        protected virtual void OnLabelProviderChanged()
        {
            if (this.LabelProvider != null)
            {
                this.LabelProvider.Invalidated += this.OnInvalidated;
            }
            if (this.LabelProviderChanged != null)
            {
                this.LabelProviderChanged(this, EventArgs.Empty);
            }
        }

        public event EventHandler LabelProviderChanged;

        protected virtual void OnInvalidated(object sender, EventArgs e)
        {
            this.InvalidateVisual();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (this.LabelProvider == null)
            {
                return;
            }
            var culture = CultureInfo.CurrentCulture;
            var typeFace = new Typeface(this.FontFamily, this.FontStyle, this.FontWeight, this.FontStretch);
            var labels = this.LabelProvider.GetLabels();
            var formatted = labels.Select(
                label => new FormattedText(label, culture, this.FlowDirection, typeFace, this.FontSize, this.Foreground)
            ).ToArray();
            switch (this.Orientation)
            {
                case Orientation.Horizontal:
                    this.OnRenderHorizontal(formatted, drawingContext);
                    break;
                case Orientation.Vertical:
                    this.OnRenderVertical(formatted, drawingContext);
                    break;
            }
        }

        protected virtual void OnRenderHorizontal(FormattedText[] labels, DrawingContext drawingContext)
        {
            var position = 0;
            var width = Convert.ToInt32(this.ActualWidth);
            var y = Convert.ToInt32(this.ActualHeight / 2);
            for (int step = width / labels.Length, x = step / 2; x < width && position < labels.Length; x += step, position++)
            {
                var label = labels[position];
                var origin = new Point(x - (label.Width / 2), y - (label.Height / 2));
                drawingContext.DrawText(label, origin);
            }
        }

        protected virtual void OnRenderVertical(FormattedText[] labels, DrawingContext drawingContext)
        {
            var position = labels.Length - 1;
            var height = Convert.ToInt32(this.ActualHeight);
            var x = Convert.ToInt32(this.ActualWidth / 2);
            for (int step = height / labels.Length, y = step / 2; y < height && position >= 0; y += step, position--)
            {
                var label = labels[position];
                var origin = new Point(x - (label.Width / 2), y - (label.Height / 2));
                drawingContext.DrawText(label, origin);
            }
        }
    }

    public abstract class AxisLabelProvider : DependencyObject
    {
        public AxisLabelProvider()
        {

        }

        public abstract IEnumerable<string> GetLabels();

        protected virtual void OnInvalidated()
        {
            if (this.Invalidated == null)
            {
                return;
            }
            this.Invalidated(this, EventArgs.Empty);
        }

        public event EventHandler Invalidated;
    }

    public class RangeAxisLabelProvider : AxisLabelProvider
    {
        public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(
            "Minimum",
            typeof(int),
            typeof(RangeAxisLabelProvider),
            new PropertyMetadata(0, new PropertyChangedCallback(OnMinimumChanged))
        );

        public static int GetMinimum(RangeAxisLabelProvider source)
        {
            return (int)source.GetValue(MinimumProperty);
        }

        public static void SetMinimum(RangeAxisLabelProvider source, int value)
        {
            source.SetValue(MinimumProperty, value);
        }

        public static void OnMinimumChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var provider = sender as RangeAxisLabelProvider;
            if (provider == null)
            {
                return;
            }
            provider.OnMinimumChanged();
        }

        public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(
            "Maximum",
            typeof(int),
            typeof(RangeAxisLabelProvider),
            new PropertyMetadata(10, new PropertyChangedCallback(OnMaximumChanged))
        );

        public static int GetMaximum(RangeAxisLabelProvider source)
        {
            return (int)source.GetValue(MaximumProperty);
        }

        public static void SetMaximum(RangeAxisLabelProvider source, int value)
        {
            source.SetValue(MaximumProperty, value);
        }

        public static void OnMaximumChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var provider = sender as RangeAxisLabelProvider;
            if (provider == null)
            {
                return;
            }
            provider.OnMaximumChanged();
        }

        public static readonly DependencyProperty IntervalProperty = DependencyProperty.Register(
            "Interval",
            typeof(int),
            typeof(RangeAxisLabelProvider),
            new PropertyMetadata(10, new PropertyChangedCallback(OnIntervalChanged))
        );

        public static int GetInterval(RangeAxisLabelProvider source)
        {
            return (int)source.GetValue(IntervalProperty);
        }

        public static void SetInterval(RangeAxisLabelProvider source, int value)
        {
            source.SetValue(IntervalProperty, value);
        }

        public static void OnIntervalChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var provider = sender as RangeAxisLabelProvider;
            if (provider == null)
            {
                return;
            }
            provider.OnIntervalChanged();
        }

        public RangeAxisLabelProvider()
        {

        }

        public int Minimum
        {
            get
            {
                return GetMinimum(this);
            }
            set
            {
                SetMinimum(this, value);
            }
        }

        protected virtual void OnMinimumChanged()
        {
            this.OnInvalidated();
            if (this.MinimumChanged != null)
            {
                this.MinimumChanged(this, EventArgs.Empty);
            }
        }

        public event EventHandler MinimumChanged;

        public int Maximum
        {
            get
            {
                return GetMaximum(this);
            }
            set
            {
                SetMaximum(this, value);
            }
        }

        protected virtual void OnMaximumChanged()
        {
            this.OnInvalidated();
            if (this.MaximumChanged != null)
            {
                this.MaximumChanged(this, EventArgs.Empty);
            }
        }

        public event EventHandler MaximumChanged;

        public int Interval
        {
            get
            {
                return GetInterval(this);
            }
            set
            {
                SetInterval(this, value);
            }
        }

        protected virtual void OnIntervalChanged()
        {
            this.OnInvalidated();
            if (this.IntervalChanged != null)
            {
                this.IntervalChanged(this, EventArgs.Empty);
            }
        }

        public event EventHandler IntervalChanged;

        public override IEnumerable<string> GetLabels()
        {
            for (var value = this.Minimum; value <= this.Maximum; value += this.Interval)
            {
                yield return Convert.ToString(value);
            }
        }
    }

    public class FixedAxisLabelProvider : AxisLabelProvider
    {
        public static readonly DependencyProperty ValuesProperty = DependencyProperty.Register(
            "Values",
            typeof(Int32Collection),
            typeof(FixedAxisLabelProvider),
            new PropertyMetadata(null, new PropertyChangedCallback(OnValuesChanged))
        );

        public static Int32Collection GetValues(FixedAxisLabelProvider source)
        {
            return (Int32Collection)source.GetValue(ValuesProperty);
        }

        public static void SetValues(FixedAxisLabelProvider source, Int32Collection value)
        {
            source.SetValue(ValuesProperty, value);
        }

        public static void OnValuesChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var provider = sender as FixedAxisLabelProvider;
            if (provider == null)
            {
                return;
            }
            provider.OnValuesChanged();
        }

        public FixedAxisLabelProvider()
        {

        }

        public Int32Collection Values
        {
            get
            {
                return GetValues(this);
            }
            set
            {
                SetValues(this, value);
            }
        }

        protected virtual void OnValuesChanged()
        {
            this.OnInvalidated();
            if (this.ValuesChanged != null)
            {
                this.ValuesChanged(this, EventArgs.Empty);
            }
        }

        public event EventHandler ValuesChanged;

        public override IEnumerable<string> GetLabels()
        {
            if (this.Values != null)
            {
                foreach (var value in this.Values)
                {
                    if (value < 1000)
                    {
                        yield return Convert.ToString(value);
                    }
                    else
                    {
                        yield return string.Format("{0:0.##}K", (float)value / 1000);
                    }
                }
            }
        }
    }
}