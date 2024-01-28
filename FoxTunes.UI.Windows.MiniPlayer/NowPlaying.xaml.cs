using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoxTunes
{
    /// <summary>
    /// Interaction logic for NowPlaying.xaml
    /// </summary>
    [UIComponent("CFF16494-CB86-4483-99C7-07E496FE894A", role: UIComponentRole.Info)]
    public partial class NowPlaying : ConfigurableUIComponentBase
    {
        public const string CATEGORY = "C0803688-E230-4DFF-AFCB-931B3AA5BE6D";

        public NowPlaying()
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

        public override IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return NowPlayingConfiguration.GetConfigurationSections();
        }

        protected override Task<bool> ShowSettings()
        {
            return this.ShowSettings(
                Strings.NowPlaying_Name,
                NowPlayingConfiguration.SECTION
            );
        }
    }
}
