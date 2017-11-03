namespace FoxTunes.Interfaces
{
    public interface IScriptingRuntime : IStandardComponent
    {
        ICoreScripts CoreScripts { get; }

        IScriptingContext CreateContext();
    }
}
