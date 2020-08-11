using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FoxTunes.ViewModel
{
    public class Report : ViewModelBase
    {
        public ReportGridViewColumnFactory GridViewColumnFactory { get; private set; }

        private IReport _Source { get; set; }

        public IReport Source
        {
            get
            {
                return this._Source;
            }
            set
            {
                this._Source = value;
                this.OnSourceChanged();
            }
        }

        protected virtual void OnSourceChanged()
        {
            this.Refresh();
            if (this.SourceChanged != null)
            {
                this.SourceChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Source");
        }

        public event EventHandler SourceChanged;

        public IEnumerable<GridViewColumn> GridColumns
        {
            get
            {
                if (this.Source != null && this.GridViewColumnFactory != null)
                {
                    for (var a = 0; a < this.Source.Headers.Length; a++)
                    {
                        yield return this.GridViewColumnFactory.Create(a);
                    }
                }
            }
        }

        protected virtual void OnGridColumnsChanged()
        {
            if (this.GridColumnsChanged != null)
            {
                this.GridColumnsChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("GridColumns");
        }

        public event EventHandler GridColumnsChanged;

        private IReportRow _SelectedRow { get; set; }

        public IReportRow SelectedRow
        {
            get
            {
                return this._SelectedRow;
            }
            set
            {
                this._SelectedRow = value;
                this.OnSelectedRowChanged();
            }
        }

        protected virtual void OnSelectedRowChanged()
        {
            if (this.SelectedRowChanged != null)
            {
                this.SelectedRowChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("SelectedRow");
        }

        public event EventHandler SelectedRowChanged;

        public ICommand ActionCommand
        {
            get
            {
                return new Command(this.Action);
            }
        }

        public void Action()
        {
            if (this.Source == null || this.SelectedRow == null)
            {
                return;
            }
            this.Source.Action(this.SelectedRow.Id);
        }

        protected virtual void Refresh()
        {
            this.GridViewColumnFactory = new ReportGridViewColumnFactory(this.Source);
            this.OnGridColumnsChanged();
        }

        protected override Freezable CreateInstanceCore()
        {
            return new Report();
        }
    }
}
