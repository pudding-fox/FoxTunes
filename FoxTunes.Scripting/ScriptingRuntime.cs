using FoxTunes.Interfaces;

namespace FoxTunes
{
    public abstract class ScriptingRuntime : StandardComponent, IScriptingRuntime
    {
        public abstract ICoreScripts CoreScripts { get; }

        public abstract IScriptingContext CreateContext();
    }
}
