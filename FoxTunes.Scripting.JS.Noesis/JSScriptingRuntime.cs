using FoxTunes.Interfaces;
using Noesis.Javascript;

namespace FoxTunes
{
    [Component("8D4693E0-6416-4B33-9DE7-89116D15F5EA", ComponentSlots.ScriptingRuntime, @default: true)]
    //TODO: This component (Noesis.Javascript) was unstable on amd64 platforms.
    [PlatformDependency(Architecture = ProcessorArchitecture.X86)]
    public class JSScriptingRuntime : ScriptingRuntime
    {
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
            var context = new JSScriptingContext(new JavascriptContext());
            context.InitializeComponent(this.Core);
            return context;
        }
    }
}