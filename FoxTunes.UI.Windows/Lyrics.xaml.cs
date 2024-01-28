namespace FoxTunes
{
    /// <summary>
    /// Interaction logic for Lyrics.xaml
    /// </summary>
    [UIComponent("F7774E81-26FC-4F0C-8E0A-67214D155547", UIComponentSlots.TOP_RIGHT, "Lyrics")]
    [UIComponentDependency(MetaDataBehaviourConfiguration.SECTION, MetaDataBehaviourConfiguration.READ_LYRICS_TAGS)]
    public partial class Lyrics : UIComponentBase
    {
        public Lyrics()
        {
            this.InitializeComponent();
        }
    }
}
