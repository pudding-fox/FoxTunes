using FoxTunes.Interfaces;
using FoxTunes.Tasks;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class ClearPlaylistTask : PlaylistTaskBase
    {
        public const string ID = "D2F22C47-386F-4333-AD4F-693951C0E5A1";

        public ClearPlaylistTask()
            : base(ID)
        {

        }

        public IDataManager DataManager { get; private set; }

        public ISignalEmitter SignalEmitter { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.DataManager = core.Managers.Data;
            this.SignalEmitter = core.Components.SignalEmitter;
            base.InitializeComponent(core);
        }

        protected override Task OnRun()
        {
            this.IsIndeterminate = true;
            using (var context = this.DataManager.CreateWriteContext())
            {
                context.Execute(Resources.ClearPlaylist);
            }
            this.SignalEmitter.Send(new Signal(this, CommonSignals.PlaylistUpdated));
            return Task.CompletedTask;
        }
    }
}
