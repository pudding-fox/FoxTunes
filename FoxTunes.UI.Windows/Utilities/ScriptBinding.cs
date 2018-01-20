using FoxTunes.Interfaces;
using System;
using System.Globalization;
using System.Windows.Data;

namespace FoxTunes.Utilities
{
    public abstract class ScriptBinding : Binding, IValueConverter
    {
        private ScriptBinding()
        {
            this.Converter = this;
        }

        protected ScriptBinding(IScriptingContext scriptingContext, string script) : this()
        {
            this.ScriptingContext = scriptingContext;
            this.Script = script;
        }

        public IScriptingContext ScriptingContext { get; private set; }

        public string Script { get; private set; }

        public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public virtual object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
