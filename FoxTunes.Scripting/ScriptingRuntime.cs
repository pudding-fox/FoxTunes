using FoxTunes.Interfaces;

namespace FoxTunes
{
    public abstract class ScriptingRuntime : StandardComponent, IScriptingRuntime
    {
        public abstract IScriptingContext CreateContext();
    }
}
