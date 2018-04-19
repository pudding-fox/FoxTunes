namespace FoxTunes
{
    public class TestCore : Core
    {
        protected override void LoadConfiguration()
        {
            this.Components.Configuration.Reset();
            base.LoadConfiguration();
        }
    }
}
