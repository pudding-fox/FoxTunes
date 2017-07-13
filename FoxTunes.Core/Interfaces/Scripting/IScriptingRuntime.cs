namespace FoxTunes.Interfaces
{
    public interface IScriptingRuntime : IStandardComponent
    {
        IScriptingContext CreateContext();
    }
}
