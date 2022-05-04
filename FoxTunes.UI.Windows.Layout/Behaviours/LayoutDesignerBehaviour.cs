using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FoxTunes
{
    [WindowsUserInterfaceDependency]
    public class LayoutDesignerBehaviour : StandardBehaviour, IInvocableComponent, IDisposable
    {
        public const string DESIGN = "AAAA";

        public LayoutDesignerBehaviour()
        {
            this.Overlays = new List<UIComponentDesignerOverlay>();
        }

        public IList<UIComponentDesignerOverlay> Overlays { get; private set; }

        public LayoutDesignerBehaviourTask DesignerTask { get; private set; }

        public bool Enabled
        {
            get
            {
                return UIComponentRoot.Active.Any();
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

        public IBackgroundTaskEmitter BackgroundTaskEmitter { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            Windows.ShuttingDown += this.OnShuttingDown;
            UIComponentRoot.ActiveChanged += this.OnActiveChanged;
            Windows.Registrations.AddIsVisibleChanged(ToolWindowManagerWindow.ID, this.OnWindowIsVisibleChanged);
            this.Core = core;
            this.BackgroundTaskEmitter = core.Components.BackgroundTaskEmitter;
            base.InitializeComponent(core);
        }

        protected virtual void OnShuttingDown(object sender, EventArgs e)
        {
            this.IsDesigning = false;
        }

        protected virtual void OnActiveChanged(object sender, EventArgs e)
        {
            if (!this.IsDesigning)
            {
                return;
            }
            var root = sender as UIComponentRoot;
            if (root == null || !UIComponentRoot.Active.Contains(root))
            {
                return;
            }
            this.ShowDesignerOverlay(root);
        }

        protected virtual void OnWindowIsVisibleChanged(object sender, EventArgs e)
        {
            this.IsDesigning = Windows.Registrations.IsVisible(ToolWindowManagerWindow.ID);
        }

        protected virtual void ShowDesignerOverlay()
        {
            this.DesignerTask = new LayoutDesignerBehaviourTask(this);
            this.DesignerTask.InitializeComponent(this.Core);
            var task = this.BackgroundTaskEmitter.Send(this.DesignerTask);
            this.Dispatch(this.DesignerTask.Run);
            foreach (var root in UIComponentRoot.Active)
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
            if (this.DesignerTask != null)
            {
                this.DesignerTask.Dispose();
                this.DesignerTask = null;
            }

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
                if (this.Enabled && !Windows.Registrations.IsVisible(ToolWindowManagerWindow.ID))
                {
                    yield return new InvocationComponent(
                        InvocationComponent.CATEGORY_SETTINGS,
                        DESIGN,
                        Strings.LayoutDesignerBehaviour_Design,
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
            Windows.ShuttingDown -= this.OnShuttingDown;
            UIComponentRoot.ActiveChanged -= this.OnActiveChanged;
            Windows.Registrations.RemoveIsVisibleChanged(ToolWindowManagerWindow.ID, this.OnWindowIsVisibleChanged);
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

        public class LayoutDesignerBehaviourTask : BackgroundTask
        {
            public const string ID = "A8C4F704-F688-4CD2-8CF8-BAF8C7815759";

            private LayoutDesignerBehaviourTask() : base(ID)
            {
                this.Name = "Editing Layout";
                this.Handle = new AutoResetEvent(false);
            }

            public LayoutDesignerBehaviourTask(LayoutDesignerBehaviour behaviour) : this()
            {
                this.Behaviour = behaviour;
            }

            public AutoResetEvent Handle { get; private set; }

            public LayoutDesignerBehaviour Behaviour { get; private set; }

            public override bool Visible
            {
                get
                {
                    return true;
                }
            }

            public override bool Cancellable
            {
                get
                {
                    return true;
                }
            }

            protected override Task OnRun()
            {
                this.Handle.WaitOne();
#if NET40
                return TaskEx.FromResult(false);
#else
                return Task.CompletedTask;
#endif
            }

            protected override void OnCancellationRequested()
            {
                var task = Windows.Invoke(() => this.Behaviour.IsDesigning = false);
                base.OnCancellationRequested();
            }

            protected override void OnDisposing()
            {
                if (this.Handle != null)
                {
                    this.Handle.Set();
                    this.Handle.Dispose();
                }
                base.OnDisposing();
            }
        }
    }
}
