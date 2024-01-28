using FoxTunes.Interfaces;
using Noesis.Javascript;

namespace FoxTunes
{
    public class JSScriptingRuntime : ScriptingRuntime
    {
        public ICore Core { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Core = core;
            base.InitializeComponent(core);
        }

        public override IScriptingContext CreateContext()
        {
            var context = new JSScriptingContext(new JavascriptContext());
            context.InitializeComponent(this.Core);
            return context;
        }
    }
}
