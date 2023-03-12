using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoxTunes
{
    /// <summary>
    /// Interaction logic for BandedWaveFormStreamPosition.xaml
    /// </summary>
    [UIComponent("6EDA5DCD-7A00-4933-A2EF-C70C99F7B36A", role: UIComponentRole.Playback)]
    public partial class BandedWaveFormStreamPosition : ConfigurableUIComponentBase
    {
        public const string CATEGORY = "BEE11F64-A91C-461C-9199-98854BF68708";

        public BandedWaveFormStreamPosition()
        {
            this.InitializeComponent();
        }

        public override IEnumerable<string> InvocationCategories
        {
            get
            {
                yield return CATEGORY;
            }
        }

        protected override Task<bool> ShowSettings()
        {
            return this.ShowSettings(
                Strings.WaveFormStreamPositionConfiguration_Section,
                BandedWaveFormStreamPositionConfiguration.SECTION
            );
        }

        public override IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return BandedWaveFormStreamPositionConfiguration.GetConfigurationSections();
        }
    }
}
