using System;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace FoxTunes
{
    public static partial class ListViewExtensions
    {
        private static readonly ConditionalWeakTable<ListView, DropInsertBehaviour> DropInsertBehaviours = new ConditionalWeakTable<ListView, DropInsertBehaviour>();

        public static readonly DependencyProperty DropInsertProperty = DependencyProperty.RegisterAttached(
            "DropInsert",
            typeof(bool),
            typeof(ListViewExtensions),
            new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnDropInsertPropertyChanged))
        );

        public static bool GetDropInsert(ListView source)
        {
            return (bool)source.GetValue(DropInsertProperty);
        }

        public static void SetDropInsert(ListView source, bool value)
        {
            source.SetValue(DropInsertProperty, value);
        }

        private static void OnDropInsertPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var listView = sender as ListView;
            if (listView == null)
            {
                return;
            }
            if (GetDropInsert(listView))
            {
                var behaviour = default(DropInsertBehaviour);
                if (!DropInsertBehaviours.TryGetValue(listView, out behaviour))
                {
                    DropInsertBehaviours.Add(listView, new DropInsertBehaviour(listView));
                }
            }
            else
            {
                DropInsertBehaviours.Remove(listView);
            }
        }

        public static readonly DependencyProperty DropInsertActiveProperty = DependencyProperty.RegisterAttached(
            "DropInsertActive",
            typeof(bool),
            typeof(ListViewExtensions),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
        );

        public static bool GetDropInsertActive(ListView source)
        {
            return (bool)source.GetValue(DropInsertActiveProperty);
        }

        public static void SetDropInsertActive(ListView source, bool value)
        {
            source.SetValue(DropInsertActiveProperty, value);
        }

        public static readonly DependencyProperty DropInsertPenProperty = DependencyProperty.RegisterAttached(
            "DropInsertPen",
            typeof(Pen),
            typeof(ListViewExtensions)
        );

        public static Pen GetDropInsertPen(ListView source)
        {
            return (Pen)source.GetValue(DropInsertPenProperty);
        }

        public static void SetDropInsertPen(ListView source, Pen value)
        {
            source.SetValue(DropInsertPenProperty, value);
        }

        public static readonly DependencyProperty DropInsertItemProperty = DependencyProperty.RegisterAttached(
            "DropInsertItem",
            typeof(ListViewItem),
            typeof(ListViewExtensions),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
        );

        public static ListViewItem GetDropInsertItem(ListView source)
        {
            return (ListViewItem)source.GetValue(DropInsertItemProperty);
        }

        public static void SetDropInsertItem(ListView source, ListViewItem value)
        {
            source.SetValue(DropInsertItemProperty, value);
        }

        public static readonly DependencyProperty DropInsertValueProperty = DependencyProperty.RegisterAttached(
            "DropInsertValue",
            typeof(object),
            typeof(ListViewExtensions),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
        );

        public static object GetDropInsertValue(ListView source)
        {
            return source.GetValue(DropInsertValueProperty);
        }

        public static void SetDropInsertValue(ListView source, object value)
        {
            source.SetValue(DropInsertValueProperty, value);
        }

        public static readonly DependencyProperty DropInsertOffsetProperty = DependencyProperty.RegisterAttached(
            "DropInsertOffset",
            typeof(int),
            typeof(ListViewExtensions),
            new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
        );

        public static int GetDropInsertOffset(ListView source)
        {
            return (int)source.GetValue(DropInsertOffsetProperty);
        }

        public static void SetDropInsertOffset(ListView source, int value)
        {
            source.SetValue(DropInsertOffsetProperty, value);
        }

        private class DropInsertBehaviour : UIBehaviour
        {
            private DropInsertBehaviour()
            {
                this.Callback = new DelayedCallback(this.RemoveCore, TimeSpan.FromMilliseconds(100));
            }

            public DropInsertBehaviour(ListView listView) : this()
            {
                this.ListView = listView;
                this.ListView.DragEnter += this.ListView_DragEnter;
                this.ListView.DragOver += this.ListView_DragOver;
                this.ListView.Drop += this.ListView_Drop;
                this.ListView.DragLeave += this.ListView_DragLeave;
                this.Adorner = new DropInsertAdorner(listView);
            }

            public DelayedCallback Callback { get; private set; }

            public ListView ListView { get; private set; }

            public DropInsertAdorner Adorner { get; private set; }

            protected virtual void ListView_DragEnter(object sender, DragEventArgs e)
            {
                this.Add();
            }

            protected virtual void ListView_DragOver(object sender, DragEventArgs e)
            {
                this.UpdateDropInsertIndex(e.GetPosition(this.ListView));
            }

            protected virtual void ListView_Drop(object sender, DragEventArgs e)
            {
                this.Remove();
            }

            protected virtual void ListView_DragLeave(object sender, DragEventArgs e)
            {
                var position = e.GetPosition(this.ListView);
                if (position.X < 0 || position.Y < 0)
                {
                    this.Remove();
                }
            }

            protected virtual void Add()
            {
                this.Callback.Disable();
                if (this.Adorner.Parent == null)
                {
                    var layer = AdornerLayer.GetAdornerLayer(this.ListView);
                    layer.IsHitTestVisible = false;
                    layer.Add(this.Adorner);
                }
                SetDropInsertActive(this.ListView, true);
                SetDropInsertItem(this.ListView, null);
                SetDropInsertOffset(this.ListView, 0);
            }

            protected virtual void Remove()
            {
                this.Callback.Enable();
            }

            protected virtual void RemoveCore()
            {
                var task = Windows.Invoke(() =>
                {
                    if (this.Adorner.Parent != null)
                    {
                        var layer = AdornerLayer.GetAdornerLayer(this.ListView);
                        layer.Remove(this.Adorner);
                    }
                    SetDropInsertActive(this.ListView, false);
                    SetDropInsertItem(this.ListView, null);
                    SetDropInsertOffset(this.ListView, 0);
                });
            }

            protected virtual void UpdateDropInsertIndex(Point point)
            {
                var result = VisualTreeHelper.HitTest(this.ListView, point);
                if (result.VisualHit == null)
                {
                    return;
                }
                var listViewItem = result.VisualHit.FindAncestor<ListViewItem>();
                if (listViewItem != null)
                {
                    var offset = this.ListView.TranslatePoint(point, listViewItem).Y < (listViewItem.ActualHeight / 2) ? 0 : 1;
                    SetDropInsertItem(this.ListView, listViewItem);
                    SetDropInsertValue(this.ListView, listViewItem.DataContext);
                    SetDropInsertOffset(this.ListView, offset);
                }
                this.Adorner.InvalidateVisual();
            }

            protected override void OnDisposing()
            {
                this.ListView.DragEnter -= this.ListView_DragEnter;
                this.ListView.DragOver -= this.ListView_DragOver;
                this.ListView.Drop -= this.ListView_Drop;
                this.ListView.DragLeave -= this.ListView_DragLeave;
                base.OnDisposing();
            }
        }

        private class DropInsertAdorner : Adorner
        {
            public DropInsertAdorner(ListView listView) : base(listView)
            {
                this.ListView = listView;
            }

            public ListView ListView { get; private set; }

            protected override void OnRender(DrawingContext context)
            {
                var item = GetDropInsertItem(this.ListView);
                var listViewItem = GetDropInsertItem(this.ListView);
                if (listViewItem != null && listViewItem.IsDescendantOf(this.ListView))
                {
                    this.RenderInsertionMarker(context, listViewItem);
                }
                base.OnRender(context);
            }

            protected virtual void RenderInsertionMarker(DrawingContext context, ListViewItem listViewItem)
            {
                var scrollViewer = listViewItem.FindAncestor<ScrollViewer>();
                //Translate the top left corner of the item to the position on the list view.
                var point = listViewItem.TransformToAncestor(this.ListView).Transform(new Point());
                var offset = GetDropInsertOffset(this.ListView);
                if (offset > 0)
                {
                    //If the offset is non zero we're inserting *after* the index.
                    //In this case we will draw the marker at the bottom of the item.
                    point.Y += listViewItem.ActualHeight + listViewItem.Margin.Bottom;
                }
                var pen = GetDropInsertPen(this.ListView);
                //The horizontal line.
                context.DrawLine(
                    pen,
                    point,
                    new Point(point.X + scrollViewer.ViewportWidth, point.Y)
                );
                //The left vertical nubbin.
                context.DrawLine(
                    pen,
                    new Point(point.X, (point.Y - 5) - pen.Thickness),
                    new Point(point.X, point.Y + 10)
                );
                //The right vertical nubbin.
                context.DrawLine(
                    pen,
                    new Point(point.X + scrollViewer.ViewportWidth, (point.Y - 5) - pen.Thickness),
                    new Point(point.X + scrollViewer.ViewportWidth, point.Y + 10)
                );
            }
        }
    }
}
