using FoxDb;
using FoxTunes.Interfaces;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace FoxTunes
{
    public static partial class FrameworkElementExtensions
    {
        private static ILogger Logger
        {
            get
            {
                return LogManager.Logger;
            }
        }

        private static readonly ConditionalWeakTable<FrameworkElement, IsVirtualizingBehaviour> IsVirtualizingBehaviours = new ConditionalWeakTable<FrameworkElement, IsVirtualizingBehaviour>();

        public static readonly DependencyProperty IsVirtualizingProperty = DependencyProperty.RegisterAttached(
            "IsVirtualizing",
            typeof(bool?),
            typeof(FrameworkElementExtensions),
            new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnIsVirtualizingPropertyChanged))
        );

        public static bool? GetIsVirtualizing(FrameworkElement source)
        {
            return (bool?)source.GetValue(IsVirtualizingProperty);
        }

        public static void SetIsVirtualizing(FrameworkElement source, bool? value)
        {
            source.SetValue(IsVirtualizingProperty, value);
        }

        private static void OnIsVirtualizingPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var frameworkElement = sender as FrameworkElement;
            if (frameworkElement == null)
            {
                return;
            }
            var behaviour = default(IsVirtualizingBehaviour);
            if (!IsVirtualizingBehaviours.TryGetValue(frameworkElement, out behaviour))
            {
                IsVirtualizingBehaviours.Add(frameworkElement, new IsVirtualizingBehaviour(frameworkElement));
            }
            else
            {
                Logger.Write(typeof(FrameworkElementExtensions), LogLevel.Warn, "Cannot modify virtualization settings.");
            }
        }

        private class IsVirtualizingBehaviour : UIBehaviour
        {
            //VirtualizationMode.Recycling causes issues, types in this list are OK.
            public static readonly Type[] RECYCLING_TYPES = new[]
            {
                typeof(TreeView)
            };

            public IsVirtualizingBehaviour(FrameworkElement frameworkElement)
            {
                this.FrameworkElement = frameworkElement;
                if (GetIsVirtualizing(this.FrameworkElement).GetValueOrDefault())
                {
                    VirtualizingStackPanel.SetIsVirtualizing(this.FrameworkElement, true);
                    if (RECYCLING_TYPES.Any(type => type.IsAssignableFrom(this.FrameworkElement.GetType())))
                    {
                        VirtualizingStackPanel.SetVirtualizationMode(this.FrameworkElement, VirtualizationMode.Recycling);
                    }
                    else
                    {
                        VirtualizingStackPanel.SetVirtualizationMode(this.FrameworkElement, VirtualizationMode.Standard);
                    }
                }
                else
                {
                    VirtualizingStackPanel.SetIsVirtualizing(this.FrameworkElement, false);
                    VirtualizingStackPanel.SetVirtualizationMode(this.FrameworkElement, VirtualizationMode.Standard);
                }
            }

            public FrameworkElement FrameworkElement { get; private set; }
        }
    }
}
