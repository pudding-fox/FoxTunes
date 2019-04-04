using FoxTunes.Interfaces;
using System.Diagnostics;

namespace FoxTunes
{
    public class DataBindingSourceBehaviour : StandardBehaviour
    {
        public IConfiguration Configuration { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Configuration = core.Components.Configuration;
            this.Configuration.GetElement<BooleanConfigurationElement>(
                LoggingBehaviourConfiguration.SECTION,
                LoggingBehaviourConfiguration.TRACE_ELEMENT
            ).ConnectValue(value =>
            {
                if (value)
                {
                    this.EnableTracing();
                }
                else
                {
                    this.DisableTracing();
                }
            });
            base.InitializeComponent(core);
        }

        public virtual void EnableTracing()
        {
            PresentationTraceSources.DataBindingSource.Listeners.Add(new TraceListener());
            PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.All;
            Logger.Write(this, LogLevel.Debug, "Tracing enabled.");
        }

        public virtual void DisableTracing()
        {
            PresentationTraceSources.DataBindingSource.Listeners.Clear();
            PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Off;
            Logger.Write(this, LogLevel.Debug, "Tracing disabled.");
        }

        public class TraceListener : global::System.Diagnostics.TraceListener
        {
            public override void Write(string message)
            {
                //Nothing to do.
            }

            public override void WriteLine(string message)
            {
                Logger.Write(typeof(DataBindingSourceBehaviour), LogLevel.Debug, message);
            }
        }
    }
}
