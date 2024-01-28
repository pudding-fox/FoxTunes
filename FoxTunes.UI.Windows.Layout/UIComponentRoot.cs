using System.Windows;
using System.Windows.Data;

namespace FoxTunes
{
    public class UIComponentRoot : UIComponentPanel
    {
        public UIComponentRoot()
        {
            var container = new UIComponentContainer();
            container.SetBinding(
                UIComponentContainer.ComponentProperty,
                new Binding()
                {
                    Source = this,
                    Path = new PropertyPath(nameof(this.Component))
                }
            );
            this.Content = container;
        }

        protected override void CreateBindings()
        {
            //Nothing to do.
        }
    }
}
