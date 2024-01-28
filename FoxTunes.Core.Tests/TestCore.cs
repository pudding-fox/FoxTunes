using FoxTunes.Interfaces;

namespace FoxTunes
{
    public class TestCore : Core
    {
        public TestCore() : base(GetSetup())
        {

        }

        protected override void LoadConfiguration()
        {
            this.Components.Configuration.Reset();
            base.LoadConfiguration();
        }

        public static ICoreSetup GetSetup()
        {
            var setup = new CoreSetup();
            setup.Disable(ComponentSlots.UserInterface);
            return setup;
        }
    }
}
