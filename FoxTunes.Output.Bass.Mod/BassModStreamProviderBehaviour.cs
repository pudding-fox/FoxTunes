using FoxDb;
using FoxTunes.Interfaces;

namespace FoxTunes
{
    public class BassModStreamProviderBehaviour : StandardBehaviour
    {
        public override void InitializeComponent(ICore core)
        {
            BassUtils.BUILT_IN_FORMATS.AddRange(BassModStreamProvider.EXTENSIONS);
        }
    }
}
