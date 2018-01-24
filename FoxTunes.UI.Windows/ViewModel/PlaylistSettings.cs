using FoxDb;
using FoxTunes.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace FoxTunes.ViewModel
{
    public class PlaylistSettings : ViewModelBase
    {
        public IDatabaseComponent Database { get; private set; }

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

        private ObservableCollection<PlaylistColumn> _PlaylistColumns { get; set; }

        public ObservableCollection<PlaylistColumn> PlaylistColumns
        {
            get
            {
                return this._PlaylistColumns;
            }
            set
            {
                this._PlaylistColumns = value;
                this.OnPlaylistColumnsChanged();
            }
        }

        protected virtual void OnPlaylistColumnsChanged()
        {
            if (this.PlaylistColumnsChanged != null)
            {
                this.PlaylistColumnsChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("PlaylistColumns");
        }

        public event EventHandler PlaylistColumnsChanged = delegate { };

        public ICommand SaveCommand
        {
            get
            {
                return new Command(this.Save);
            }
        }

        public void Save()
        {
            using (var transaction = this.Database.BeginTransaction())
            {
                var playlistColumns = this.Database.Set<PlaylistColumn>(transaction);
                playlistColumns.Remove(playlistColumns.Except(this.PlaylistColumns));
                playlistColumns.AddOrUpdate(this.PlaylistColumns);
                transaction.Commit();
            }
            this.Refresh();
            this.SignalEmitter.Send(new Signal(this, CommonSignals.PlaylistColumnsUpdated));
        }

        protected override void OnCoreChanged()
        {
            this.Database = this.Core.Components.Database;
            this.SignalEmitter = this.Core.Components.SignalEmitter;
            this.Refresh();
            base.OnCoreChanged();
        }

        protected virtual void Refresh()
        {
            this.PlaylistColumns = new ObservableCollection<PlaylistColumn>(this.Database.Sets.PlaylistColumn);
        }

        protected override Freezable CreateInstanceCore()
        {
            return new PlaylistSettings();
        }
    }
}
