using FoxTunes.Interfaces;
using Noesis.Javascript;

namespace FoxTunes
{
    [Component("8D4693E0-6416-4B33-9DE7-89116D15F5EA", ComponentSlots.ScriptingRuntime)]
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
