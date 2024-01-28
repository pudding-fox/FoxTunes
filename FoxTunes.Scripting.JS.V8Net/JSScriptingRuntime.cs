using FoxTunes.Interfaces;
using V8.Net;

namespace FoxTunes
{
    [Component(ID, ComponentSlots.ScriptingRuntime)]
    [PlatformDependency(Major = 6, Minor = 1)]
    public class JSScriptingRuntime : ScriptingRuntime
    {
        const string ID = "D3A862B1-4770-489E-9C2A-A3DEB8A47D73";

        public JSScriptingRuntime() : base(ID, string.Format(Strings.JSScriptingRuntime_Name, V8Engine.Version))
        {
        }

        public ICore Core { get; private set; }

        public override ICoreScripts CoreScripts
        {
            get
            {
                return JSCoreScripts.Instance;
            }
        }

        public override void InitializeComponent(ICore core)
        {
            this.Core = core;
            base.InitializeComponent(core);
        }

        public override IScriptingContext CreateContext()
        {
            Logger.Write(this, LogLevel.Debug, "Creating javascript scripting context.");
            var context = new JSScriptingContext(new V8Engine());
            context.InitializeComponent(this.Core);
            return context;
        }
    }
}