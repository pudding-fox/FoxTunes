#if X86
using FoxTunes.Interfaces;
using Noesis.Javascript;
using System.Diagnostics;

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
            this.Context.SetParameter("Publication", new Publication());
            this.Context.SetParameter("DateHelper", new DateHelper());
            this.Context.SetParameter("NumberHelper", new NumberHelper());
            //Note: Lower case to match tag, property etc.
            this.Context.SetParameter("strings", StringsHelper.Strings);
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

        private class Publication
        {
            public string Company
            {
                get
                {
                    return global::FoxTunes.Publication.Company;
                }
            }

            public string Product
            {
                get
                {
                    return global::FoxTunes.Publication.Product;
                }
            }

            public string Version
            {
                get
                {
                    return global::FoxTunes.Publication.Version;
                }
            }
        }
    }
}
#endif