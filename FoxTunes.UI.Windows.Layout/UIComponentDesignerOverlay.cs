using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace FoxTunes
{
    public class UIComponentDesignerOverlay : BaseComponent, IDisposable
    {
        public UIComponentDesignerOverlay(UIComponentRoot root)
        {
            this.Root = root;
        }

        public UIComponentRoot Root { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Root.IsInDesignMode = true;
            this.Root.PreviewMouseRightButtonDown += this.OnPreviewMouseRightButtonDown;
            base.InitializeComponent(core);
        }

        protected virtual void OnPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var container = this.GetContainer(e);
            if (container == null)
            {
                return;
            }
            this.ShowDesignerOverlay(container);
        }

        protected virtual UIComponentContainer GetContainer(MouseButtonEventArgs e)
        {
            var point = e.GetPosition(this.Root);
            var result = VisualTreeHelper.HitTest(this.Root, point);
            if (result == null || result.VisualHit == null)
            {
                return null;
            }
            return result.VisualHit.FindAncestor<UIComponentContainer>();
        }

        protected virtual void ShowDesignerOverlay(UIComponentContainer container)
        {
            var layer = AdornerLayer.GetAdornerLayer(container);
            if (layer == null)
            {
                return;
            }
            var adorner = new UIComponentDesignerAdorner(container);
            adorner.Closed += this.OnClosed;
            layer.Add(adorner);
        }

        protected virtual void OnClosed(object sender, EventArgs e)
        {
            var adorner = sender as UIComponentDesignerAdorner;
            if (adorner == null)
            {
                return;
            }
            try
            {
                this.HideDesignerOverlay(adorner.Container);
            }
            finally
            {
                adorner.Dispose();
            }
        }

        protected virtual void HideDesignerOverlay(UIComponentContainer container)
        {
            var layer = AdornerLayer.GetAdornerLayer(container);
            if (layer == null)
            {
                return;
            }
            var adorners = layer.GetAdorners(container);
            if (adorners == null)
            {
                return;
            }
            foreach (var adorner in adorners)
            {
                layer.Remove(adorner);
                if (adorner is UIComponentDesignerAdorner designerAdorner)
                {
                    designerAdorner.Closed -= this.OnClosed;
                    designerAdorner.Dispose();
                }
            }
        }

        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.IsDisposed || !disposing)
            {
                return;
            }
            this.OnDisposing();
            this.IsDisposed = true;
        }

        protected virtual void OnDisposing()
        {
            if (this.Root != null)
            {
                this.Root.IsInDesignMode = false;
                this.Root.PreviewMouseRightButtonDown -= this.OnPreviewMouseRightButtonDown;
            }
        }

        ~UIComponentDesignerOverlay()
        {
            Logger.Write(this, LogLevel.Error, "Component was not disposed: {0}", this.GetType().Name);
            try
            {
                this.Dispose(true);
            }
            catch
            {
                //Nothing can be done, never throw on GC thread.
            }
        }

        public class UIComponentDesignerAdorner : Adorner, IDisposable
        {
            public UIComponentDesignerAdorner(UIComponentContainer container) : base(container)
            {
                this.Container = container;
                this.InitializeComponent();
            }

            public UIComponentContainer Container { get; private set; }

            protected virtual void InitializeComponent()
            {
                //TODO: Max depth should be about 2.
                var panel = this.Container.FindAncestor<UIComponentPanel>();
                var components = new List<IInvocableComponent>();
                if (panel != null)
                {
                    components.Add(panel);
                }
                components.Add(this.Container);
                this.ContextMenu = new Menu()
                {
                    Category = InvocationComponent.CATEGORY_GLOBAL,
                    Components = new ObservableCollection<IInvocableComponent>(components),
                    Source = this.Container
                };
                this.ContextMenu.Opened += this.OnOpened;
                this.ContextMenu.Closed += this.OnClosed;
                this.ContextMenu.IsOpen = true;
            }

            protected virtual void OnOpened(object sender, RoutedEventArgs e)
            {
                this.InvalidateVisual();
            }

            protected virtual void OnClosed(object sender, RoutedEventArgs e)
            {
                this.InvalidateVisual();
                if (this.Closed == null)
                {
                    return;
                }
                this.Closed(this, EventArgs.Empty);
            }

            public event EventHandler Closed;

            protected override void OnRender(DrawingContext drawingContext)
            {
                //TODO: Assuming TextBrush is a SolidColorBrush.
                var brush = new SolidColorBrush((this.FindResource("TextBrush") as SolidColorBrush).Color);
                if (this.ContextMenu.IsOpen)
                {
                    brush.Opacity = 0.25;
                }
                else
                {
                    brush.Opacity = 0;
                }
                var pen = new Pen(brush, 1);
                var rectangle = new Rect(this.RenderSize);
                drawingContext.DrawRectangle(brush, pen, rectangle);
            }

            public bool IsDisposed { get; private set; }

            public void Dispose()
            {
                this.Dispose(true);
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool disposing)
            {
                if (this.IsDisposed || !disposing)
                {
                    return;
                }
                this.OnDisposing();
                this.IsDisposed = true;
            }

            protected virtual void OnDisposing()
            {
                if (this.ContextMenu != null)
                {
                    this.ContextMenu.Opened -= this.OnOpened;
                    this.ContextMenu.Closed -= this.OnClosed;
                }
            }

            ~UIComponentDesignerAdorner()
            {
                Logger.Write(typeof(UIComponentDesignerAdorner), LogLevel.Error, "Component was not disposed: {0}", this.GetType().Name);
                try
                {
                    this.Dispose(true);
                }
                catch
                {
                    //Nothing can be done, never throw on GC thread.
                }
            }
        }
    }
}
