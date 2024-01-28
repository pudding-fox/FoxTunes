using FoxTunes.Interfaces;

namespace FoxTunes
{
    public class JSCoreScripts : BaseComponent, ICoreScripts
    {
        public string Artist
        {
            get
            {
                return Resources.Artist;
            }
        }

        public string Artist_Album
        {
            get
            {
                return Resources.Artist_Album;
            }
        }

        public string Disk_Track_Title
        {
            get
            {
                return Resources.Disk_Track_Title;
            }
        }

        public string Duration
        {
            get
            {
                return Resources.Duration;
            }
        }

        public string Genre
        {
            get
            {
                return Resources.Genre;
            }
        }

        public string Codec
        {
            get
            {
                return Resources.Codec;
            }
        }

        public string Playing
        {
            get
            {
                return Resources.Playing;
            }
        }

        public string Title_Performer
        {
            get
            {
                return Resources.Title_Performer;
            }
        }

        public string Track
        {
            get
            {
                return Resources.Track;
            }
        }

        public string Year_Album
        {
            get
            {
                return Resources.Year_Album;
            }
        }

        public string BPM
        {
            get
            {
                return Resources.BPM;
            }
        }

        public static ICoreScripts Instance = new JSCoreScripts();
    }
}
