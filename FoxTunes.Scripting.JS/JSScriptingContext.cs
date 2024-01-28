using FoxTunes.Interfaces;
using Noesis.Javascript;

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

        public override object Run(string script)
        {
            return this.Context.Run(script);
        }

        protected override void OnDisposing()
        {
            this.Context.Dispose();
            base.OnDisposing();
        }
    }
}
