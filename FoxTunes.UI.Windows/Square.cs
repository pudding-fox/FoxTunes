using System.Windows;
using System.Windows.Data;

namespace FoxTunes
{
    public abstract class Square : UIComponentBase
    {
        public Square()
        {
            this.SizeChanged += this.OnSizeChanged;
        }

        protected virtual void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.RefreshLayout();
        }

        protected virtual void RefreshLayout()
        {
            BindingOperations.ClearBinding(this, WidthProperty);
            BindingOperations.ClearBinding(this, HeightProperty);
            if (this.ActualWidth > 0)
            {
                BindingOperations.SetBinding(this, HeightProperty, new Binding("ActualWidth")
                {
                    RelativeSource = new RelativeSource(RelativeSourceMode.Self)
                });
            }
            else if (this.ActualHeight > 0)
            {
                BindingOperations.SetBinding(this, WidthProperty, new Binding("ActualHeight")
                {
                    RelativeSource = new RelativeSource(RelativeSourceMode.Self)
                });
            }
            else
            {
                return;
            }
            this.SizeChanged -= this.OnSizeChanged;
        }
    }
}
