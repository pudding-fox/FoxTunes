using FoxTunes.Interfaces;

namespace FoxTunes
{
    public class BassDtsStreamProviderBehaviour : StandardBehaviour
    {
        public override void InitializeComponent(ICore core)
        {
            BassUtils.BUILT_IN_FORMATS.Add("dts");
        }
    }
}
