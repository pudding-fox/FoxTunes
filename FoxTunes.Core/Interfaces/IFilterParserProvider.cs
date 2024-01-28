namespace FoxTunes.Interfaces
{
    public interface IFilterParserProvider : IStandardComponent
    {
        bool TryParse(ref string filter, out IFilterParserResultGroup result);
    }
}
