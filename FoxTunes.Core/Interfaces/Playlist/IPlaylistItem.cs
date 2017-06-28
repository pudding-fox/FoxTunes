namespace FoxTunes.Interfaces
{
    public interface IPlaylistItem : IBaseComponent
    {
        string FileName { get; }

        IMetaDataSource MetaData { get; }
    }
}
