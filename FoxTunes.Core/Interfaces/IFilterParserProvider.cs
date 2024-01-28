namespace FoxTunes.Interfaces
{
    public interface IFilterParserProvider : IBaseComponent
    {
        byte Priority { get; }

        bool TryParse(ref string filter, out IFilterParserResultGroup result);
    }
}
