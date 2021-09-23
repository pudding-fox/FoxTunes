using System.Windows;
using System.Windows.Controls;

namespace FoxTunes
{
    /// <summary>
    /// Interaction logic for ComponentSettingsDialog.xaml
    /// </summary>
    public partial class ComponentSettingsDialog : UserControl
    {
        public static readonly DependencyProperty SectionsProperty = DependencyProperty.Register(
            "Sections",
            typeof(StringCollection),
            typeof(ComponentSettingsDialog),
            new PropertyMetadata(default(StringCollection))
        );

        public static StringCollection GetSections(ComponentSettingsDialog source)
        {
            return (StringCollection)source.GetValue(SectionsProperty);
        }

        public static void SetSections(ComponentSettingsDialog source, StringCollection value)
        {
            source.SetValue(SectionsProperty, value);
        }

        public ComponentSettingsDialog()
        {
            this.InitializeComponent();
        }

        public StringCollection Sections
        {
            get
            {
                return this.GetValue(SectionsProperty) as StringCollection;
            }
            set
            {
                this.SetValue(SectionsProperty, value);
            }
        }
    }
}
