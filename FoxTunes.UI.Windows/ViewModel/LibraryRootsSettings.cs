using FoxDb;
using FoxTunes.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace FoxTunes.ViewModel
{
    public class LibraryRootsSettings : ViewModelBase
    {
        public IDatabaseFactory DatabaseFactory { get; private set; }

        public ISignalEmitter SignalEmitter { get; private set; }

        private CollectionManager<LibraryRoot> _LibraryRoots { get; set; }

        public CollectionManager<LibraryRoot> LibraryRoots
        {
            get
            {
                return this._LibraryRoots;
            }
            set
            {
                this._LibraryRoots = value;
                this.OnLibraryRootsChanged();
            }
        }

        protected virtual void OnLibraryRootsChanged()
        {
            this.OnPropertyChanged("LibraryRoots");
        }

        private bool _IsSaving { get; set; }

        public bool IsSaving
        {
            get
            {
                return this._IsSaving;
            }
            set
            {
                this._IsSaving = value;
                this.OnIsSavingChanged();
            }
        }

        protected virtual void OnIsSavingChanged()
        {
            if (this.IsSavingChanged != null)
            {
                this.IsSavingChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("IsSaving");
        }

        public event EventHandler IsSavingChanged;

        public ICommand SaveCommand
        {
            get
            {
                var command = CommandFactory.Instance.CreateCommand(
                    new Func<Task>(this.Save)
                );
                command.Tag = CommandHints.DISMISS;
                return command;
            }
        }

        public async Task Save()
        {
            var exception = default(Exception);
            try
            {
                await Windows.Invoke(() => this.IsSaving = true).ConfigureAwait(false);
                using (var database = this.DatabaseFactory.Create())
                {
                    using (var task = new SingletonReentrantTask(CancellationToken.None, ComponentSlots.Database, SingletonReentrantTask.PRIORITY_HIGH, cancellationToken =>
                    {
                        using (var transaction = database.BeginTransaction(database.PreferredIsolationLevel))
                        {
                            var libraryRoots = database.Set<LibraryRoot>(transaction);
                            libraryRoots.Remove(libraryRoots.Except(this.LibraryRoots.ItemsSource));
                            libraryRoots.AddOrUpdate(this.LibraryRoots.ItemsSource);
                            transaction.Commit();
                        }
#if NET40
                        return TaskEx.FromResult(false);
#else
                        return Task.CompletedTask;
#endif
                    }))
                    {
                        await task.Run().ConfigureAwait(false);
                    }
                }
                return;
            }
            catch (Exception e)
            {
                exception = e;
            }
            finally
            {
                await Windows.Invoke(() => this.IsSaving = false).ConfigureAwait(false);
            }
            await this.OnError("Save", exception).ConfigureAwait(false);
            throw exception;
        }

        public ICommand CancelCommand
        {
            get
            {
                return new Command(this.Cancel)
                {
                    Tag = CommandHints.DISMISS
                };
            }
        }

        public void Cancel()
        {
            this.Dispatch(this.Refresh);
        }

        public override void InitializeComponent(ICore core)
        {
            this.DatabaseFactory = this.Core.Factories.Database;
            this.SignalEmitter = this.Core.Components.SignalEmitter;
            this.SignalEmitter.Signal += this.OnSignal;
            this.LibraryRoots = new CollectionManager<LibraryRoot>();
            this.Dispatch(this.Refresh);
            base.InitializeComponent(core);
        }

        private Task OnSignal(object sender, ISignal signal)
        {
            switch (signal.Name)
            {
                case CommonSignals.SettingsUpdated:
                    return this.Refresh();
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        protected virtual Task Refresh()
        {
            return Windows.Invoke(() =>
            {
                using (var database = this.DatabaseFactory.Create())
                {
                    using (var transaction = database.BeginTransaction(database.PreferredIsolationLevel))
                    {
                        this.LibraryRoots.ItemsSource = new ObservableCollection<LibraryRoot>(
                            database.Set<LibraryRoot>(transaction)
                        );
                    }
                }
            });
        }

        protected override void OnDisposing()
        {
            if (this.SignalEmitter != null)
            {
                this.SignalEmitter.Signal -= this.OnSignal;
            }
            base.OnDisposing();
        }

        protected override Freezable CreateInstanceCore()
        {
            return new LibraryRootsSettings();
        }
    }
}
