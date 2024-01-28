namespace FoxTunes.Interfaces
{
    public interface IMetaDataItem : IBaseComponent
    {
        string Name { get; }

        object Value { get; }

        object[] Values { get; }
    }
}
