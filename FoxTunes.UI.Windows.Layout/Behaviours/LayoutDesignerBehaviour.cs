using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class LayoutDesignerBehaviour : StandardBehaviour, IInvocableComponent, IDisposable
    {
        public const string DESIGN = "AAAA";

        public LayoutDesignerBehaviour()
        {
            this.Roots = new List<UIComponentRoot>();
            this.Overlays = new List<UIComponentDesignerOverlay>();
        }

        public IList<UIComponentRoot> Roots { get; private set; }

        public IList<UIComponentDesignerOverlay> Overlays { get; private set; }

        public bool Enabled
        {
            get
            {
                return this.Roots.Count > 0;
            }
        }

        private bool _IsDesigning { get; set; }

        public bool IsDesigning
        {
            get
            {
                return this._IsDesigning;
            }
            set
            {
                this._IsDesigning = value;
                this.OnIsDesigningChanged();
            }
        }

        protected virtual void OnIsDesigningChanged()
        {
            if (this.IsDesigning)
            {
                this.ShowDesignerOverlay();
            }
            else
            {
                this.HideDesignerOverlay();
            }
            if (this.IsDesigningChanged != null)
            {
                this.IsDesigningChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("IsDesigning");
        }

        public event EventHandler IsDesigningChanged;

        public ICore Core { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            UIComponentRoot.Loaded += this.OnLoaded;
            UIComponentRoot.Unloaded += this.OnUnloaded;
            ToolWindowBehaviour.ToolWindowManagerWindowCreated += this.OnToolWindowManagerWindowCreated;
            ToolWindowBehaviour.ToolWindowManagerWindowClosed += this.OnToolWindowManagerWindowClosed;
            this.Core = core;
            base.InitializeComponent(core);
        }

        protected virtual void OnLoaded(object sender, EventArgs e)
        {
            var root = sender as UIComponentRoot;
            if (root == null)
            {
                return;
            }
            this.Roots.Add(root);
            if (this.IsDesigning)
            {
                this.ShowDesignerOverlay(root);
            }
        }

        protected virtual void OnUnloaded(object sender, EventArgs e)
        {
            var root = sender as UIComponentRoot;
            if (root == null)
            {
                return;
            }
            this.Roots.Remove(root);
        }

        protected virtual void OnToolWindowManagerWindowCreated(object sender, EventArgs e)
        {
            this.IsDesigning = true;
        }

        protected virtual void OnToolWindowManagerWindowClosed(object sender, EventArgs e)
        {
            this.IsDesigning = false;
        }

        protected virtual void ShowDesignerOverlay()
        {
            foreach (var root in this.Roots)
            {
                this.ShowDesignerOverlay(root);
            }
        }

        protected virtual void ShowDesignerOverlay(UIComponentRoot root)
        {
            var designerOverlay = new UIComponentDesignerOverlay(root);
            designerOverlay.InitializeComponent(this.Core);
            this.Overlays.Add(designerOverlay);
        }

        protected virtual void HideDesignerOverlay()
        {
            foreach (var designerOverlay in this.Overlays)
            {
                designerOverlay.Dispose();
            }
            this.Overlays.Clear();
        }

        public IEnumerable<IInvocationComponent> Invocations
        {
            get
            {
                if (this.Enabled)
                {
                    yield return new InvocationComponent(
                        InvocationComponent.CATEGORY_SETTINGS,
                        DESIGN,
                        "Edit Layout",
                        attributes: this.IsDesigning ? InvocationComponent.ATTRIBUTE_SELECTED : InvocationComponent.ATTRIBUTE_NONE
                    );
                }
            }
        }

        public Task InvokeAsync(IInvocationComponent component)
        {
            switch (component.Id)
            {
                case DESIGN:
                    return Windows.Invoke(() => this.IsDesigning = !this.IsDesigning);
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
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
            this.HideDesignerOverlay();
            UIComponentRoot.Loaded -= this.OnLoaded;
            UIComponentRoot.Unloaded -= this.OnUnloaded;
            ToolWindowBehaviour.ToolWindowManagerWindowCreated -= this.OnToolWindowManagerWindowCreated;
            ToolWindowBehaviour.ToolWindowManagerWindowClosed -= this.OnToolWindowManagerWindowClosed;
        }

        ~LayoutDesignerBehaviour()
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
    }
}
