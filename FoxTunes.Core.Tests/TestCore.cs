using FoxTunes.Interfaces;

namespace FoxTunes
{
    public class TestCore : Core
    {
        public TestCore() : base(CoreFlags.Headless)
        {

        }

        protected override void LoadConfiguration()
        {
            this.Components.Configuration.Reset();
            base.LoadConfiguration();
        }
    }
}
