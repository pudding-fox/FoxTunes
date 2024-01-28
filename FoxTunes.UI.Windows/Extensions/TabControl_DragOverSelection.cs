using FoxTunes.Interfaces;
using System;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace FoxTunes
{
    public partial class TabControlExtensions
    {
        private static readonly ConditionalWeakTable<TabControl, DragOverSelectionBehaviour> DragOverSelectionBehaviours = new ConditionalWeakTable<TabControl, DragOverSelectionBehaviour>();

        public static readonly DependencyProperty DragOverSelectionProperty = DependencyProperty.RegisterAttached(
            "DragOverSelection",
            typeof(bool),
            typeof(TabControlExtensions),
            new UIPropertyMetadata(false, OnDragOverSelectionChanged)
        );

        private static void OnDragOverSelectionChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var tabControl = sender as TabControl;
            if (tabControl == null)
            {
                return;
            }
            if (GetDragOverSelection(tabControl))
            {
                var behaviour = default(DragOverSelectionBehaviour);
                if (!DragOverSelectionBehaviours.TryGetValue(tabControl, out behaviour))
                {
                    DragOverSelectionBehaviours.Add(tabControl, new DragOverSelectionBehaviour(tabControl));
                }
            }
            else
            {
                var behaviour = default(DragOverSelectionBehaviour);
                if (DragOverSelectionBehaviours.TryGetValue(tabControl, out behaviour))
                {
                    DragOverSelectionBehaviours.Remove(tabControl);
                    behaviour.Dispose();
                }
            }
        }

        public static bool GetDragOverSelection(TabControl source)
        {
            return (bool)source.GetValue(DragOverSelectionProperty);
        }

        public static void SetDragOverSelection(TabControl source, bool value)
        {
            source.SetValue(DragOverSelectionProperty, value);
        }

        private class DragOverSelectionBehaviour : UIBehaviour
        {
            public static readonly TimeSpan TIMEOUT = TimeSpan.FromMilliseconds(500);

            private DragOverSelectionBehaviour()
            {
                this.Debouncer = new Debouncer(TIMEOUT);
            }

            public DragOverSelectionBehaviour(TabControl tabControl) : this()
            {
                this.TabControl = tabControl;
                this.TabControl.DragOver += this.OnDragOver;
            }

            public Debouncer Debouncer { get; private set; }

            public TabControl TabControl { get; private set; }

            public TabItem TabItem { get; private set; }

            protected virtual void OnDragOver(object sender, DragEventArgs e)
            {
                var element = e.OriginalSource as FrameworkElement;
                if (element == null)
                {
                    return;
                }
                var tabItem = element.FindAncestor<TabItem>();
                if (tabItem == null)
                {
                    Logger.Write(typeof(DragOverSelectionBehaviour), LogLevel.Trace, "No tab under cursor, cancelling selection.");
                    this.Debouncer.Cancel(this.UpdateSelection);
                }
                else if (!object.ReferenceEquals(this.TabItem, tabItem))
                {
                    Logger.Write(typeof(DragOverSelectionBehaviour), LogLevel.Trace, "Tab appeared \"{0}\" under cursor, will select in {1}ms.", tabItem.Header, TIMEOUT.TotalMilliseconds);
                    this.Debouncer.Exec(this.UpdateSelection);
                }
                this.TabItem = tabItem;
            }

            protected virtual void UpdateSelection()
            {
                var task = Windows.Invoke(() =>
                {
                    if (this.TabItem == null)
                    {
                        return;
                    }
                    Logger.Write(typeof(DragOverSelectionBehaviour), LogLevel.Trace, "Selecting tab \"{0}\".", this.TabItem.Header);
                    this.TabItem.IsSelected = true;
                });
            }

            protected override void OnDisposing()
            {
                if (this.Debouncer != null)
                {
                    this.Debouncer.Dispose();
                }
                if (this.TabControl != null)
                {
                    this.TabControl.DragOver -= this.OnDragOver;
                }
                base.OnDisposing();
            }
        }
    }
}
