using FoxDb;
using FoxTunes.Interfaces;

namespace FoxTunes
{
    public class BassModStreamProviderBehaviour : StandardBehaviour
    {
        static BassModStreamProviderBehaviour()
        {
            BassModPluginLoader.Instance.Load();
        }

        public override void InitializeComponent(ICore core)
        {
            BassUtils.BUILT_IN_FORMATS.AddRange(BassModStreamProvider.EXTENSIONS);
        }
    }
}
