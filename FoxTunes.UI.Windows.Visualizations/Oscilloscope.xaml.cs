using FoxTunes.Interfaces;
using System;
using System.Windows.Media.Effects;
using System.Windows.Shapes;

namespace FoxTunes
{
    /// <summary>
    /// Interaction logic for Oscilloscope.xaml
    /// </summary>
    [UIComponent("D3FBE95D-3B9E-4DAB-B3AD-B66A53AF5F85", role: UIComponentRole.Visualization)]
    public partial class Oscilloscope : UIComponentBase
    {
        public static readonly BooleanConfigurationElement DropShadow;

        static Oscilloscope()
        {
            var configuration = ComponentRegistry.Instance.GetComponent<IConfiguration>();
            if (configuration == null)
            {
                return;
            }
            DropShadow = configuration.GetElement<BooleanConfigurationElement>(
                OscilloscopeBehaviourConfiguration.SECTION,
                OscilloscopeBehaviourConfiguration.DROP_SHADOW_ELEMENT
            );
        }

        public Oscilloscope()
        {
            this.InitializeComponent();
            if (DropShadow != null)
            {
                this.UpdateEffects();
                DropShadow.ValueChanged += this.OnValueChanged;
            }
        }

        protected virtual void UpdateEffects()
        {
            var rectangle = default(Rectangle);
            if (!this.TryFindResource<Rectangle>("Rectangle", out rectangle))
            {
                return;
            }
            if (DropShadow.Value)
            {
                rectangle.Effect = new DropShadowEffect();
            }
            else
            {
                rectangle.Effect = null;
            }
        }

        private void OnValueChanged(object sender, EventArgs e)
        {
            var task = Windows.Invoke(() =>
            {
                this.UpdateEffects();
            });
        }
    }
}