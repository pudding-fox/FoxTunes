using System;
using System.ComponentModel;
using System.Windows;

namespace FoxTunes.ViewModel
{
    public class LibraryHierarchyLevel : ViewModelBase
    {
        private string _Script { get; set; }

        [EditorAttribute("System.ComponentModel.Design.MultilineStringEditor, System.Design", "System.Drawing.Design.UITypeEditor")]
        public string Script
        {
            get
            {
                return this._Script;
            }
            set
            {
                this._Script = value;
                this.OnScriptChanged();
            }
        }

        protected virtual void OnScriptChanged()
        {
            if (this.ScriptChanged != null)
            {
                this.ScriptChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Script");
        }

        public event EventHandler ScriptChanged = delegate { };

        protected override Freezable CreateInstanceCore()
        {
            return new LibraryHierarchyLevel();
        }
    }
}
