using FoxTunes.Interfaces;

namespace FoxTunes.Behaviours
{
    [Component("D3B587B2-C1AE-4E06-A6C9-592DD7FF157D", ComponentSlots.None, priority: ComponentAttribute.PRIORITY_HIGH)]
    public class VariousArtistsBehaviour : StandardBehaviour
    {
        public IDatabase Database { get; private set; }

        public ISignalEmitter SignalEmitter { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Database = core.Components.Database;
            this.SignalEmitter = core.Components.SignalEmitter;
            this.SignalEmitter.Signal += this.OnSignal;
            base.InitializeComponent(core);
        }

        protected virtual void OnSignal(object sender, ISignal signal)
        {
            switch (signal.Name)
            {
                case CommonSignals.LibraryUpdated:
                    this.OnRun();
                    break;
            }
        }

        protected virtual void OnRun()
        {
            var parameters = default(IDbParameterCollection);
            using (var command = this.Database.CreateCommand(this.Database.Queries.VariousArtists, out parameters))
            {
                parameters["name"] = CustomMetaData.VariousArtists;
                parameters["type"] = MetaDataItemType.Tag;
                parameters["numericValue"] = 1;
                command.ExecuteNonQuery();
            }
        }
    }
}
