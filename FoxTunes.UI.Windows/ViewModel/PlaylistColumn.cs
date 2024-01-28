using System.Windows;

namespace FoxTunes.ViewModel
{
    public class PlaylistColumn : ViewModelBase
    {
        public string Header { get; set; }

        public string Script { get; set; }

        protected override Freezable CreateInstanceCore()
        {
            return new PlaylistColumn();
        }
    }
}
