using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace FoxTunes.ViewModel
{
    public class Associations : ViewModelBase
    {
        public Associations()
        {
            this.FileAssociations = new ObservableCollection<Association>();
        }

        public IOutput Output { get; private set; }

        public ObservableCollection<Association> FileAssociations { get; private set; }

        public IEnumerable<IFileAssociation> Enabled
        {
            get
            {
                return this.FileAssociations
                    .Where(association => association.FileAssociation != null && association.IsSelected)
                    .Select(association => association.FileAssociation);
            }
        }

        public IEnumerable<IFileAssociation> Disabled
        {
            get
            {
                return this.FileAssociations
                    .Where(association => association.FileAssociation != null && !association.IsSelected)
                    .Select(association => association.FileAssociation);
            }
        }

        protected override void OnCoreChanged()
        {
            this.Output = this.Core.Components.Output;
            this.Refresh();
            base.OnCoreChanged();
        }

        protected virtual void Refresh()
        {
            var extensions = this.Core.Associations.Associations.Select(
                association => association.Extension.TrimStart('.')
            ).ToArray();
            this.FileAssociations.Clear();
            this.FileAssociations.AddRange(
                this.Output.SupportedExtensions.Select(extension => new Association(
                    this.Core.Associations.Create(extension),
                    extensions.Contains(extension)
                ))
            );
            this.FileAssociations.Add(new Association(
                null,
                this.FileAssociations.Count == extensions.Length
            ));
        }

        public ICommand SelectAllCommand
        {
            get
            {
                return new Command<bool>(selectAll => this.SelectAll(selectAll));
            }
        }

        public void SelectAll(bool selectAll)
        {
            if (selectAll)
            {
                foreach (var association in this.FileAssociations)
                {
                    association.IsSelected = true;
                }
            }
            else
            {
                this.Refresh();
            }
        }

        public ICommand SaveCommand
        {
            get
            {
                return new Command(this.Save)
                {
                    Tag = CommandHints.DISMISS
                };
            }
        }

        public void Save()
        {
            if (this.Enabled.Any())
            {
                this.Core.Associations.Enable();
            }
            else
            {
                this.Core.Associations.Disable();
            }
            this.Core.Associations.Disable(this.Disabled);
            this.Core.Associations.Enable(this.Enabled);
            this.Refresh();
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
            this.Refresh();
        }

        protected override Freezable CreateInstanceCore()
        {
            return new Associations();
        }
    }

    public class Association : ViewModelBase
    {
        public Association(IFileAssociation fileAssociation, bool isSelected)
        {
            this.FileAssociation = fileAssociation;
            this.IsSelected = isSelected;
        }

        public IFileAssociation FileAssociation { get; private set; }

        private bool _IsSelected { get; set; }

        public bool IsSelected
        {
            get
            {
                return this._IsSelected;
            }
            set
            {
                this._IsSelected = value;
                this.OnIsSelectedChanged();
            }
        }

        protected virtual void OnIsSelectedChanged()
        {
            if (this.IsSelectedChanged != null)
            {
                this.IsSelectedChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("IsSelected");
        }

        public event EventHandler IsSelectedChanged = delegate { };

        protected override Freezable CreateInstanceCore()
        {
            return new Association(null, false);
        }
    }
}
