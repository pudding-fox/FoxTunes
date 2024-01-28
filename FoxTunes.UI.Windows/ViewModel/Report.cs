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

        private IReportComponent _Source { get; set; }

        public IReportComponent Source
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

        private IReportComponentRow _SelectedRow { get; set; }

        public IReportComponentRow SelectedRow
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
            this.OnRowActionCommandChanged();
        }

        public event EventHandler SelectedRowChanged;

        public ICommand RowActionCommand
        {
            get
            {
                if (this.SelectedRow == null)
                {
                    return CommandBase.Disabled;
                }
                return new AsyncCommand(this.SelectedRow.Action);
            }
        }

        protected virtual void OnRowActionCommandChanged()
        {
            if (this.RowActionCommandChanged != null)
            {
                this.RowActionCommandChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("RowActionCommand");
        }

        public event EventHandler RowActionCommandChanged;

        public string ActionName
        {
            get
            {
                if (this.Source != null && !string.IsNullOrEmpty(this.Source.ActionName))
                {
                    return this.Source.ActionName;
                }
                return Strings.Report_Close;
            }
        }

        protected virtual void OnActionNameChanged()
        {
            if (this.ActionNameChanged != null)
            {
                this.ActionNameChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("ActionName");
        }

        public event EventHandler ActionNameChanged;

        public ICommand ActionCommand
        {
            get
            {
                if (this.Source == null)
                {
                    return CommandBase.Disabled;
                }
                return new AsyncCommand(this.Source.Action);
            }
        }

        protected virtual void OnActionCommandChanged()
        {
            if (this.ActionCommandChanged != null)
            {
                this.ActionCommandChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("ActionCommand");
        }

        public event EventHandler ActionCommandChanged;

        protected virtual void Refresh()
        {
            this.GridViewColumnFactory = new ReportGridViewColumnFactory(this.Source);
            this.OnGridColumnsChanged();
            this.OnActionNameChanged();
            this.OnActionCommandChanged();
        }

        protected override Freezable CreateInstanceCore()
        {
            return new Report();
        }
    }
}
