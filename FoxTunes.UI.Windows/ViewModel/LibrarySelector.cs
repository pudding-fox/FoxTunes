using FoxTunes.Interfaces;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace FoxTunes.ViewModel
{
    public class LibrarySelector : ViewModelBase
    {
        private LibraryHierarchyCollection _Hierarchies { get; set; }

        public LibraryHierarchyCollection Hierarchies
        {
            get
            {
                return this._Hierarchies;
            }
            set
            {
                this._Hierarchies = value;
                this.OnHierarchiesChanged();
            }
        }

        protected virtual void OnHierarchiesChanged()
        {
            if (this.HierarchiesChanged != null)
            {
                this.HierarchiesChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Hierarchies");
        }

        public event EventHandler HierarchiesChanged;

        public LibraryHierarchy SelectedHierarchy
        {
            get
            {
                if (this.LibraryManager == null)
                {
                    return LibraryHierarchy.Empty;
                }
                return this.LibraryManager.SelectedHierarchy;
            }
            set
            {
                if (this.LibraryManager == null || value == null)
                {
                    return;
                }
                this.LibraryManager.SelectedHierarchy = value;
            }
        }

        protected virtual void OnSelectedHierarchyChanged()
        {
            if (this.SelectedHierarchyChanged != null)
            {
                this.SelectedHierarchyChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("SelectedHierarchy");
        }

        public event EventHandler SelectedHierarchyChanged;

        public ILibraryHierarchyBrowser LibraryHierarchyBrowser { get; private set; }

        public ILibraryManager LibraryManager { get; private set; }

        public ISignalEmitter SignalEmitter { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.LibraryHierarchyBrowser = this.Core.Components.LibraryHierarchyBrowser;
            this.LibraryManager = this.Core.Managers.Library;
            this.LibraryManager.SelectedHierarchyChanged += this.OnSelectedHierarchyChanged;
            this.SignalEmitter = this.Core.Components.SignalEmitter;
            this.SignalEmitter.Signal += this.OnSignal;
            var task = this.Refresh();
            base.InitializeComponent(core);
        }

        protected virtual void OnSelectedHierarchyChanged(object sender, EventArgs e)
        {
            var task = Windows.Invoke(new Action(this.OnSelectedHierarchyChanged));
        }

        public virtual async Task Refresh()
        {
            await this.RefreshHierarchies().ConfigureAwait(false);
            await Windows.Invoke(this.OnSelectedHierarchyChanged).ConfigureAwait(false);
        }

        protected virtual Task RefreshHierarchies()
        {
            var hierarchies = this.LibraryHierarchyBrowser.GetHierarchies();
            if (this.Hierarchies == null)
            {
                return Windows.Invoke(() => this.Hierarchies = new LibraryHierarchyCollection(hierarchies));
            }
            else
            {
                return Windows.Invoke(this.Hierarchies.Reset(hierarchies));
            }
        }


        protected virtual Task OnSignal(object sender, ISignal signal)
        {
            switch (signal.Name)
            {
                case CommonSignals.HierarchiesUpdated:
                    return this.Refresh();
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        protected override Freezable CreateInstanceCore()
        {
            return new LibrarySelector();
        }
    }
}
