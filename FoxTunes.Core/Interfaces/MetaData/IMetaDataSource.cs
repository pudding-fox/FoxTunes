namespace FoxTunes.Interfaces
{
    public interface IMetaDataSource : IBaseComponent
    {
        IMetaDataItems Items { get; }
    }
}
