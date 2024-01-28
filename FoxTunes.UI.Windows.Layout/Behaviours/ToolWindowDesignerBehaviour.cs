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
    public class ToolWindowDesignerBehaviour : StandardBehaviour
    {
        public ToolWindowDesignerBehaviour()
        {
            this.DesignerOverlays = new List<ToolWindowDesignerOverlay>();
        }

        public IList<ToolWindowDesignerOverlay> DesignerOverlays { get; private set; }

        public ICore Core { get; private set; }

        public ToolWindowBehaviour Behaviour { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            ToolWindowBehaviour.ToolWindowManagerWindowCreated += this.OnToolWindowManagerWindowCreated;
            ToolWindowBehaviour.ToolWindowManagerWindowClosed += this.OnToolWindowManagerWindowClosed;
            this.Behaviour = ComponentRegistry.Instance.GetComponent<ToolWindowBehaviour>();
            this.Behaviour.Loaded += this.OnLoaded;
            base.InitializeComponent(core);
        }

        protected virtual void OnToolWindowManagerWindowCreated(object sender, EventArgs e)
        {
            this.ShowDesignerOverlay();
        }

        protected virtual void OnToolWindowManagerWindowClosed(object sender, EventArgs e)
        {
            this.HideDesignerOverlay();
        }

        protected virtual void OnLoaded(object sender, ToolWindowConfigurationEventArgs e)
        {
            if (ToolWindowBehaviour.IsToolWindowManagerWindowCreated)
            {
                var task = Windows.Invoke(() => this.ShowDesignerOverlay(e.Configuration, e.Window));
            }
        }

        protected virtual void ShowDesignerOverlay()
        {
            foreach (var pair in this.Behaviour.Windows)
            {
                this.ShowDesignerOverlay(pair.Key, pair.Value);
            }
        }

        protected virtual void ShowDesignerOverlay(ToolWindowConfiguration config, ToolWindow window)
        {
            var designerOverlay = new ToolWindowDesignerOverlay(config, window);
            designerOverlay.InitializeComponent(this.Core);
            this.DesignerOverlays.Add(designerOverlay);
        }

        protected virtual void HideDesignerOverlay()
        {
            foreach (var designerOverlay in this.DesignerOverlays)
            {
                designerOverlay.Dispose();
            }
            this.DesignerOverlays.Clear();
        }

        public class ToolWindowDesignerOverlay : BaseComponent, IDisposable
        {
            public ToolWindowDesignerOverlay(ToolWindowConfiguration config, ToolWindow window)
            {
                this.Configuration = config;
                this.Window = window;
            }

            public ToolWindowConfiguration Configuration { get; private set; }

            public ToolWindow Window { get; private set; }

            public override void InitializeComponent(ICore core)
            {
                this.Window.Root.IsInDesignMode = true;
                this.Window.PreviewMouseRightButtonDown += this.OnPreviewMouseRightButtonDown;
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
                var point = e.GetPosition(this.Window);
                var result = VisualTreeHelper.HitTest(this.Window, point);
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
                var adorner = new ToolWindowDesignerAdorner(container);
                adorner.Closed += this.OnClosed;
                layer.Add(adorner);
            }

            protected virtual void OnClosed(object sender, EventArgs e)
            {
                var adorner = sender as ToolWindowDesignerAdorner;
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
                    if (adorner is ToolWindowDesignerAdorner designerAdorner)
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
                if (this.Window != null)
                {
                    this.Window.Root.IsInDesignMode = false;
                    this.Window.PreviewMouseRightButtonDown -= this.OnPreviewMouseRightButtonDown;
                }
            }

            ~ToolWindowDesignerOverlay()
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

            public class ToolWindowDesignerAdorner : Adorner, IDisposable
            {
                public ToolWindowDesignerAdorner(UIComponentContainer container) : base(container)
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

                ~ToolWindowDesignerAdorner()
                {
                    Logger.Write(typeof(ToolWindowDesignerAdorner), LogLevel.Error, "Component was not disposed: {0}", this.GetType().Name);
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
}
