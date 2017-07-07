namespace FoxTunes.Interfaces
{
    public interface IPlaylistItem : IPersistableComponent
    {
        string FileName { get; }

        IMetaDataSource MetaData { get; }
    }
}
