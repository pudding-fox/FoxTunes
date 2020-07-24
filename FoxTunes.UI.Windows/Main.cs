using FoxTunes.Interfaces;
using System;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace FoxTunes
{
    public class Main : ContentControl
    {
        public Main()
        {
            LayoutManager.Instance.LayoutChanged += this.OnLayoutChanged;
            this.LoadLayout();
        }

        protected virtual void OnLayoutChanged(object sender, EventArgs e)
        {
            this.LoadLayout();
        }

        protected virtual Task LoadLayout()
        {
            return Windows.Invoke(() =>
            {
                this.Content = LayoutManager.Instance.Load(UILayoutTemplate.Main);
            });
        }
    }
}
