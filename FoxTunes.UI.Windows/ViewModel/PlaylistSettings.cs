using FoxTunes.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace FoxTunes.ViewModel
{
    public class PlaylistSettings : ViewModelBase
    {
        public IDatabaseContext DatabaseContext { get; private set; }

        public ISignalEmitter SignalEmitter { get; private set; }

        private PlaylistColumn _SelectedPlaylistColumn { get; set; }

        public PlaylistColumn SelectedPlaylistColumn
        {
            get
            {
                return this._SelectedPlaylistColumn;
            }
            set
            {
                this._SelectedPlaylistColumn = value;
                this.OnSelectedPlaylistColumnChanged();
            }
        }

        protected virtual void OnSelectedPlaylistColumnChanged()
        {
            if (this.SelectedPlaylistColumnChanged != null)
            {
                this.SelectedPlaylistColumnChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("SelectedPlaylistColumn");
        }

        public event EventHandler SelectedPlaylistColumnChanged = delegate { };

        public ObservableCollection<PlaylistColumn> Columns
        {
            get
            {
                if (this.DatabaseContext == null)
                {
                    return null;
                }
                return this.DatabaseContext.Sets.PlaylistColumn.Local;
            }
        }

        protected virtual void OnColumnsChanged()
        {
            if (this.ColumnsChanged != null)
            {
                this.ColumnsChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Columns");
        }

        public event EventHandler ColumnsChanged = delegate { };

        public ICommand UpdateCommand
        {
            get
            {
                return new Command(this.Update);
            }
        }

        public void Update()
        {
            if (this.SelectedPlaylistColumn.Id == 0)
            {
                return;
            }
            this.DatabaseContext.Sets.PlaylistColumn.Update(this.SelectedPlaylistColumn);
        }

        public ICommand SaveCommand
        {
            get
            {
                return new Command(this.Save);
            }
        }

        public void Save()
        {
            this.DatabaseContext.SaveChanges();
            this.DatabaseContext.Dispose();
            this.DatabaseContext = this.Core.Managers.Data.CreateWriteContext();
            this.SignalEmitter.Send(new Signal(this, CommonSignals.PlaylistColumnsUpdated));
        }

        protected override void OnCoreChanged()
        {
            this.DatabaseContext = this.Core.Managers.Data.CreateWriteContext();
            this.DatabaseContext.Sets.PlaylistColumn.Load();
            this.SignalEmitter = this.Core.Components.SignalEmitter;
            this.OnColumnsChanged();
            base.OnCoreChanged();
        }

        protected override Freezable CreateInstanceCore()
        {
            return new PlaylistSettings();
        }
    }
}
