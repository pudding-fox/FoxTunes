using System.Windows;
using System.Windows.Controls;

namespace FoxTunes
{
    /// <summary>
    /// Interaction logic for Menu.xaml
    /// </summary>
    public partial class Menu : ContextMenu
    {
        public Menu()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty CategoryProperty = DependencyProperty.Register(
            "Category",
            typeof(string),
            typeof(Menu)
        );

        public static string GetCategory(Menu source)
        {
            return (string)source.GetValue(CategoryProperty);
        }

        public static void SetCategory(Menu source, string value)
        {
            source.SetValue(CategoryProperty, value);
        }

        public string Category
        {
            get
            {
                return this.GetValue(CategoryProperty) as string;
            }
            set
            {
                this.SetValue(CategoryProperty, value);
            }
        }
    }
}
