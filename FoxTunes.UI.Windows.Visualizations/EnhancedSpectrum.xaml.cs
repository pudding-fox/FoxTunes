using FoxTunes.Interfaces;
using System;

namespace FoxTunes
{
    /// <summary>
    /// Interaction logic for Spectrum.xaml
    /// </summary>
    [UIComponent("9A696BBC-F6AB-4180-8661-353A79253AA9", role: UIComponentRole.Visualization)]
    public partial class EnhancedSpectrum : UIComponentBase
    {
        public static readonly SelectionConfigurationElement BandCount;

        static EnhancedSpectrum()
        {
            var configuration = ComponentRegistry.Instance.GetComponent<IConfiguration>();
            if (configuration == null)
            {
                return;
            }
            BandCount = configuration.GetElement<SelectionConfigurationElement>(
                SpectrumBehaviourConfiguration.SECTION,
                SpectrumBehaviourConfiguration.BANDS_ELEMENT
            );
        }

        public EnhancedSpectrum()
        {
            this.InitializeComponent();
            if (BandCount != null)
            {
                //Fix the width so all 2d math is integer.
                this.MinWidth = SpectrumBehaviourConfiguration.GetWidthForBands(BandCount.Value);
                BandCount.ValueChanged += this.OnValueChanged;
            }
        }

        private void OnValueChanged(object sender, EventArgs e)
        {
            var task = Windows.Invoke(() =>
            {
                //Fix the width so all 2d math is integer.
                this.MinWidth = SpectrumBehaviourConfiguration.GetWidthForBands(BandCount.Value);
            });
        }
    }
}