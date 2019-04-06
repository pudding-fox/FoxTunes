using System;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace FoxTunes
{
    public static partial class ListBoxExtensions
    {
        private static readonly ConditionalWeakTable<ListBox, DragDropReorderBehaviour> DragDropReorderBehaviours = new ConditionalWeakTable<ListBox, DragDropReorderBehaviour>();

        public static readonly DependencyProperty DragDropReorderProperty = DependencyProperty.RegisterAttached(
            "DragDropReorder",
            typeof(bool),
            typeof(ListBoxExtensions),
            new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnDragDropReorderChanged))
        );

        public static bool GetDragDropReorder(ListBox source)
        {
            return (bool)source.GetValue(DragDropReorderProperty);
        }

        public static void SetDragDropReorder(ListBox source, bool value)
        {
            source.SetValue(DragDropReorderProperty, value);
        }

        private static void OnDragDropReorderChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var listBox = sender as ListBox;
            if (listBox == null)
            {
                return;
            }
            if (GetDragDropReorder(listBox))
            {
                var behaviour = default(DragDropReorderBehaviour);
                if (!DragDropReorderBehaviours.TryGetValue(listBox, out behaviour))
                {
                    DragDropReorderBehaviours.Add(listBox, new DragDropReorderBehaviour(listBox));
                }
            }
            else
            {
                DragDropReorderBehaviours.Remove(listBox);
            }
        }

        public static readonly DependencyProperty DragDropReorderCommandProperty = DependencyProperty.RegisterAttached(
            "DragDropReorderCommand",
            typeof(ICommand),
            typeof(ListBoxExtensions)
        );

        public static ICommand GetDragDropReorderCommand(ListBox source)
        {
            return (ICommand)source.GetValue(DragDropReorderCommandProperty);
        }

        public static void SetDragDropReorderCommand(ListBox source, ICommand value)
        {
            source.SetValue(DragDropReorderCommandProperty, value);
        }

        private class DragDropReorderBehaviour : UIBehaviour
        {
            public DragDropReorderBehaviour(ListBox listBox)
            {
                this.ListBox = listBox;
                this.ListBox.AllowDrop = true;
                this.ListBox.PreviewMouseDown += this.OnMouseDown;
                this.ListBox.PreviewMouseUp += this.OnMouseUp;
                this.ListBox.MouseMove += this.OnMouseMove;
                this.ListBox.DragOver += this.OnDragOver;
            }

            public Point DragStartPosition { get; private set; }

            public bool DragInitialized { get; private set; }

            public ListBox ListBox { get; private set; }

            protected virtual bool ShouldInitializeDrag(object source, Point position)
            {
                if (this.DragStartPosition.Equals(default(Point)) || source is Thumb)
                {
                    return false;
                }
                if (Math.Abs(position.X - this.DragStartPosition.X) > (SystemParameters.MinimumHorizontalDragDistance * 2))
                {
                    return true;
                }
                if (Math.Abs(position.Y - this.DragStartPosition.Y) > (SystemParameters.MinimumVerticalDragDistance * 2))
                {
                    return true;
                }
                return false;
            }

            protected virtual void DoDragDrop()
            {
                this.DragInitialized = true;
                try
                {
                    DragDrop.DoDragDrop(
                        this.ListBox,
                        this.ListBox.SelectedValue,
                        DragDropEffects.Copy
                    );
                }
                finally
                {
                    this.DragInitialized = false;
                }
            }

            protected virtual void OnMouseDown(object sender, MouseButtonEventArgs e)
            {
                if (e.LeftButton != MouseButtonState.Pressed)
                {
                    return;
                }
                this.DragStartPosition = e.GetPosition(this.ListBox);
            }

            protected virtual void OnMouseUp(object sender, MouseButtonEventArgs e)
            {
                this.DragStartPosition = default(Point);
            }

            protected virtual void OnMouseMove(object sender, MouseEventArgs e)
            {
                if (e.LeftButton != MouseButtonState.Pressed || this.DragInitialized || this.ListBox.SelectedValue == null)
                {
                    return;
                }
                var position = e.GetPosition(this.ListBox);
                if (this.ShouldInitializeDrag(e.OriginalSource, position))
                {
                    this.DragStartPosition = default(Point);
                    this.DoDragDrop();
                }
            }

            protected virtual void OnDragOver(object sender, DragEventArgs e)
            {
                var effects = DragDropEffects.None;
                if (!this.DragInitialized)
                {
                    return;
                }
                var position = e.GetPosition(this.ListBox);
                var result = VisualTreeHelper.HitTest(this.ListBox, position);
                if (result != null && result.VisualHit is FrameworkElement)
                {
                    var value = (result.VisualHit as FrameworkElement).DataContext;
                    if (value != null && !object.ReferenceEquals(this.ListBox.SelectedValue, value))
                    {
                        var command = GetDragDropReorderCommand(this.ListBox);
                        var parameter = new[] { this.ListBox.SelectedValue, value };
                        if (command != null && command.CanExecute(parameter))
                        {
                            command.Execute(parameter);
                        }
                        effects |= DragDropEffects.Move;
                    }
                }
                e.Effects = effects;
            }

            protected override void OnDisposing()
            {
                this.ListBox.PreviewMouseDown -= this.OnMouseDown;
                this.ListBox.PreviewMouseUp -= this.OnMouseUp;
                this.ListBox.MouseMove -= this.OnMouseMove;
                this.ListBox.DragOver -= this.OnDragOver;
                base.OnDisposing();
            }
        }
    }
}