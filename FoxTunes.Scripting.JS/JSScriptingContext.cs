using FoxTunes.Interfaces;
using Noesis.Javascript;
using System;
using System.Diagnostics;
using System.Globalization;

namespace FoxTunes
{
    public class JSScriptingContext : ScriptingContext
    {
        public JSScriptingContext(JavascriptContext context)
        {
            this.Context = context;
        }

        public JavascriptContext Context { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Context.Run(Resources.utils);
            //TODO: We need to update the runtime to support Date.toLocaleDateString().
            this.Context.SetParameter("toLocaleDateString", new Func<string, string>(
                value =>
                {
                    var date = default(DateTime);
                    if (!DateTime.TryParseExact(value, Constants.DATE_FORMAT, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                    {
                        return null;
                    }
                    return date.ToShortDateString() + " " + date.ToShortTimeString();
                }
            ));
            base.InitializeComponent(core);
        }

        public override void SetValue(string name, object value)
        {
            this.Context.SetParameter(name, value);
        }

        public override object GetValue(string name)
        {
            return this.Context.GetParameter(name);
        }

        [DebuggerNonUserCode]
        public override object Run(string script)
        {
            try
            {
                return this.Context.Run(script);
            }
            catch (JavascriptException e)
            {
                throw new ScriptingException(e.Line, e.StartColumn, e.EndColumn, e.Message);
            }
        }

        protected override void OnDisposing()
        {
            this.Context.Dispose();
            base.OnDisposing();
        }
    }
}
