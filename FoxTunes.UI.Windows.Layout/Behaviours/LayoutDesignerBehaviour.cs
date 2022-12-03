using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FoxTunes
{
    [WindowsUserInterfaceDependency]
    public class LayoutDesignerBehaviour : StandardBehaviour, IInvocableComponent, IDisposable
    {
        public const string LOAD = "AAAA";

        public const string DESIGN = "BBBB";

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
                if (this._IsDesigning == value)
                {
                    return;
                }
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

        public IUserInterface UserInterface { get; private set; }

        public IBackgroundTaskEmitter BackgroundTaskEmitter { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public SelectionConfigurationElement MainPreset { get; private set; }

        public TextConfigurationElement MainLayout { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            Windows.ShuttingDown += this.OnShuttingDown;
            UIComponentRoot.ActiveChanged += this.OnActiveChanged;
            Windows.Registrations.AddIsVisibleChanged(ToolWindowManagerWindow.ID, this.OnWindowIsVisibleChanged);
            this.Core = core;
            this.UserInterface = core.Components.UserInterface;
            this.BackgroundTaskEmitter = core.Components.BackgroundTaskEmitter;
            this.Configuration = core.Components.Configuration;
            this.MainPreset = this.Configuration.GetElement<SelectionConfigurationElement>(
                UIComponentLayoutProviderConfiguration.SECTION,
                UIComponentLayoutProviderConfiguration.MAIN_PRESET
            );
            this.MainLayout = this.Configuration.GetElement<TextConfigurationElement>(
                UIComponentLayoutProviderConfiguration.SECTION,
                UIComponentLayoutProviderConfiguration.MAIN_LAYOUT
            );
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
                    foreach (var option in this.MainPreset.Options)
                    {
                        var preset = UIComponentLayoutProviderPresets.GetPresetById(UIComponentLayoutProviderPresets.Main.Presets, option.Id);
                        var isActive = UIComponentLayoutProviderPresets.IsLoaded(
                            UIComponentLayoutProviderConfiguration.SECTION,
                            UIComponentLayoutProviderConfiguration.MAIN_PRESET,
                            UIComponentLayoutProviderConfiguration.MAIN_LAYOUT,
                            UIComponentLayoutProviderPresets.Main.Presets,
                            preset
                        );
                        yield return new InvocationComponent(
                            InvocationComponent.CATEGORY_SETTINGS,
                            LOAD,
                            option.Name,
                            path: Path.Combine(Strings.LayoutDesignerBehaviour_Path, Strings.LayoutDesignerBehaviour_Load),
                            attributes: isActive ? InvocationComponent.ATTRIBUTE_SELECTED : InvocationComponent.ATTRIBUTE_NONE
                        );
                    }
                    yield return new InvocationComponent(
                        InvocationComponent.CATEGORY_SETTINGS,
                        DESIGN,
                        Strings.LayoutDesignerBehaviour_Design,
                        path: Strings.LayoutDesignerBehaviour_Path,
                        attributes: (byte)((this.IsDesigning ? InvocationComponent.ATTRIBUTE_SELECTED : InvocationComponent.ATTRIBUTE_NONE) | InvocationComponent.ATTRIBUTE_SEPARATOR)
                    );
                }
            }
        }

        public Task InvokeAsync(IInvocationComponent component)
        {
            switch (component.Id)
            {
                case LOAD:
                    return this.Load(component.Name);
                case DESIGN:
                    return this.Design();
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        public Task Load(string name)
        {
            var preset = UIComponentLayoutProviderPresets.GetActivePreset(
                UIComponentLayoutProviderConfiguration.SECTION,
                UIComponentLayoutProviderConfiguration.MAIN_PRESET,
                UIComponentLayoutProviderConfiguration.MAIN_LAYOUT,
                UIComponentLayoutProviderPresets.Main.Presets
            );
            if (preset == null)
            {
                if (!this.UserInterface.Confirm(Strings.LayoutDesignerBehaviour_Overwrite))
                {
#if NET40
                    return TaskEx.FromResult(false);
#else
                    return Task.CompletedTask;
#endif
                }
            }
            this.MainPreset.Value = this.MainPreset.Options.FirstOrDefault(option => string.Equals(option.Name, name));
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        public Task Design()
        {
            return Windows.Invoke(() => this.IsDesigning = !this.IsDesigning);
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
