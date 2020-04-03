using System.Runtime.CompilerServices;
using System.Windows;

namespace FoxTunes
{
    public static partial class FrameworkElementExtensions
    {
        private static readonly ConditionalWeakTable<FrameworkElement, StyleBehaviour> StyleBehaviours = new ConditionalWeakTable<FrameworkElement, StyleBehaviour>();

        public static readonly DependencyProperty StyleProperty = DependencyProperty.RegisterAttached(
            "Style",
            typeof(Style),
            typeof(FrameworkElementExtensions),
            new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnStylePropertyChanged))
        );

        public static Style GetStyle(FrameworkElement source)
        {
            return (Style)source.GetValue(StyleProperty);
        }

        public static void SetStyle(FrameworkElement source, Style value)
        {
            source.SetValue(StyleProperty, value);
        }

        private static void OnStylePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var FrameworkElement = sender as FrameworkElement;
            if (FrameworkElement == null)
            {
                return;
            }
            if (GetStyle(FrameworkElement) != null)
            {
                var behaviour = default(StyleBehaviour);
                if (!StyleBehaviours.TryGetValue(FrameworkElement, out behaviour))
                {
                    StyleBehaviours.Add(FrameworkElement, new StyleBehaviour(FrameworkElement));
                }
            }
            else
            {
                StyleBehaviours.Remove(FrameworkElement);
            }
        }

        private class StyleBehaviour : DynamicStyleBehaviour
        {
            public StyleBehaviour(FrameworkElement FrameworkElement)
            {
                this.FrameworkElement = FrameworkElement;
                this.Apply();
            }

            public FrameworkElement FrameworkElement { get; private set; }

            protected override void Apply()
            {
                var type = this.FrameworkElement.GetType();
                var basedOn = default(Style);
                do
                {
                    basedOn = (Style)this.FrameworkElement.TryFindResource(type);
                    if (basedOn != null)
                    {
                        break;
                    }
                    type = type.BaseType;
                } while (type != null);
                this.FrameworkElement.Style = this.CreateStyle(
                    GetStyle(this.FrameworkElement),
                    basedOn
                );
            }
        }
    }
}
