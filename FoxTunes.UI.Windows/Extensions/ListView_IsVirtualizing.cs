using FoxTunes.Interfaces;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace FoxTunes
{
    public static partial class ListViewExtensions
    {
        private static ILogger Logger
        {
            get
            {
                return LogManager.Logger;
            }
        }

        private static readonly ConditionalWeakTable<ListView, IsVirtualizingBehaviour> IsVirtualizingBehaviours = new ConditionalWeakTable<ListView, IsVirtualizingBehaviour>();

        public static readonly DependencyProperty IsVirtualizingProperty = DependencyProperty.RegisterAttached(
            "IsVirtualizing",
            typeof(bool?),
            typeof(ListViewExtensions),
            new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnIsVirtualizingPropertyChanged))
        );

        public static bool? GetIsVirtualizing(ListView source)
        {
            return (bool?)source.GetValue(IsVirtualizingProperty);
        }

        public static void SetIsVirtualizing(ListView source, bool? value)
        {
            source.SetValue(IsVirtualizingProperty, value);
        }

        private static void OnIsVirtualizingPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var listView = sender as ListView;
            if (listView == null)
            {
                return;
            }
            var behaviour = default(IsVirtualizingBehaviour);
            if (!IsVirtualizingBehaviours.TryGetValue(listView, out behaviour))
            {
                IsVirtualizingBehaviours.Add(listView, new IsVirtualizingBehaviour(listView));
            }
            else
            {
                Logger.Write(typeof(ListViewExtensions), LogLevel.Warn, "Cannot modify virtualization settings.");
            }
        }

        private class IsVirtualizingBehaviour : UIBehaviour
        {
            public IsVirtualizingBehaviour(ListView listView)
            {
                this.ListView = listView;
                if (GetIsVirtualizing(this.ListView).GetValueOrDefault())
                {
                    VirtualizingStackPanel.SetIsVirtualizing(this.ListView, true);
                    VirtualizingStackPanel.SetVirtualizationMode(this.ListView, VirtualizationMode.Recycling);
                }
                else
                {
                    VirtualizingStackPanel.SetIsVirtualizing(this.ListView, false);
                    VirtualizingStackPanel.SetVirtualizationMode(this.ListView, VirtualizationMode.Standard);
                }
            }

            public ListView ListView { get; private set; }
        }
    }
}
