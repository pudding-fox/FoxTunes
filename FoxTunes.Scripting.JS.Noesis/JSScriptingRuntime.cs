using FoxTunes.Interfaces;
using Noesis.Javascript;

namespace FoxTunes
{
    [ComponentPreference(ComponentPreferenceAttribute.DEFAULT)]
    [Component(ID, ComponentSlots.ScriptingRuntime)]
    //TODO: Noesis.Javascript.dll targets x86 
    [PlatformDependency(Architecture = ProcessorArchitecture.X86)]
    public class JSScriptingRuntime : ScriptingRuntime
    {
        const string ID = "8D4693E0-6416-4B33-9DE7-89116D15F5EA";

        public JSScriptingRuntime() : base(ID, Strings.JSScriptingRuntime_Name, Strings.JSScriptingRuntime_Description)
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
            var context = new JSScriptingContext(new JavascriptContext());
            context.InitializeComponent(this.Core);
            return context;
        }
    }
}