#if X64
using FoxTunes.Interfaces;
using V8.Net;

namespace FoxTunes
{
    [Component("8D4693E0-6416-4B33-9DE7-89116D15F5EA", ComponentSlots.ScriptingRuntime)]
    [PlatformDependency(Major = 6, Minor = 1)]
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
            var context = new JSScriptingContext(new V8Engine());
            context.InitializeComponent(this.Core);
            return context;
        }
    }
}
#endif