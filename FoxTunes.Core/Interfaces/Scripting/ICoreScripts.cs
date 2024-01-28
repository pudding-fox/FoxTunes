namespace FoxTunes.Interfaces
{
    public interface ICoreScripts : IBaseComponent
    {
        string PlaylistSortValues { get; }

        string Artist { get; }

        string Artist_Album { get; }

        string Disk_Track_Title { get; }

        string Duration { get; }

        string Genre { get; }

        string Playing { get; }

        string Title_Performer { get; }

        string Track { get; }

        string Year_Album { get; }
    }
}
