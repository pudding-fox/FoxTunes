using FoxTunes.Interfaces;
using Noesis.Javascript;

namespace FoxTunes
{
    public class JSScriptingRuntime : ScriptingRuntime
    {
        public override IScriptingContext CreateContext()
        {
            return new JSScriptingContext(new JavascriptContext());
        }
    }
}
