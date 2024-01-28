using FoxTunes.Interfaces;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace FoxTunes
{
    public static partial class ListBoxExtensions
    {
        private static ILogger Logger
        {
            get
            {
                return LogManager.Logger;
            }
        }

        private static readonly ConditionalWeakTable<ListBox, IsVirtualizingBehaviour> IsVirtualizingBehaviours = new ConditionalWeakTable<ListBox, IsVirtualizingBehaviour>();

        public static readonly DependencyProperty IsVirtualizingProperty = DependencyProperty.RegisterAttached(
            "IsVirtualizing",
            typeof(bool?),
            typeof(ListBoxExtensions),
            new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnIsVirtualizingPropertyChanged))
        );

        public static bool? GetIsVirtualizing(ListBox source)
        {
            return (bool?)source.GetValue(IsVirtualizingProperty);
        }

        public static void SetIsVirtualizing(ListBox source, bool? value)
        {
            source.SetValue(IsVirtualizingProperty, value);
        }

        private static void OnIsVirtualizingPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var listBox = sender as ListBox;
            if (listBox == null)
            {
                return;
            }
            var behaviour = default(IsVirtualizingBehaviour);
            if (!IsVirtualizingBehaviours.TryGetValue(listBox, out behaviour))
            {
                IsVirtualizingBehaviours.Add(listBox, new IsVirtualizingBehaviour(listBox));
            }
            else
            {
                Logger.Write(typeof(ListBoxExtensions), LogLevel.Warn, "Cannot modify virtualization settings.");
            }
        }

        private class IsVirtualizingBehaviour : UIBehaviour
        {
            public IsVirtualizingBehaviour(ListBox listBox)
            {
                this.ListBox = listBox;
                if (GetIsVirtualizing(this.ListBox).GetValueOrDefault())
                {
                    VirtualizingStackPanel.SetIsVirtualizing(this.ListBox, true);
                    VirtualizingStackPanel.SetVirtualizationMode(this.ListBox, VirtualizationMode.Recycling);
                }
                else
                {
                    VirtualizingStackPanel.SetIsVirtualizing(this.ListBox, false);
                    VirtualizingStackPanel.SetVirtualizationMode(this.ListBox, VirtualizationMode.Standard);
                }
            }

            public ListBox ListBox { get; private set; }
        }
    }
}
