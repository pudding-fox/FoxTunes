using FoxTunes.Interfaces;
using System;

namespace FoxTunes
{
    /// <summary>
    /// Interaction logic for Spectrum.xaml
    /// </summary>
    [UIComponent("381328C3-C2CE-4FDA-AC92-71A15C3FC387", role: UIComponentRole.Visualization)]
    public partial class Spectrum : UIComponentBase
    {
        public static readonly SelectionConfigurationElement BarCount;

        static Spectrum()
        {
            var configuration = ComponentRegistry.Instance.GetComponent<IConfiguration>();
            if (configuration == null)
            {
                return;
            }
            BarCount = configuration.GetElement<SelectionConfigurationElement>(
                SpectrumBehaviourConfiguration.SECTION,
                SpectrumBehaviourConfiguration.BARS_ELEMENT
            );
        }

        public Spectrum()
        {
            this.InitializeComponent();
            if (BarCount != null)
            {
                //Fix the width so all 2d math is integer.
                this.MinWidth = SpectrumBehaviourConfiguration.GetWidthForBars(BarCount.Value);
                BarCount.ValueChanged += this.OnValueChanged;
            }
        }

        private void OnValueChanged(object sender, EventArgs e)
        {
            var task = Windows.Invoke(() =>
            {
                //Fix the width so all 2d math is integer.
                this.MinWidth = SpectrumBehaviourConfiguration.GetWidthForBars(BarCount.Value);
            });
        }
    }
}