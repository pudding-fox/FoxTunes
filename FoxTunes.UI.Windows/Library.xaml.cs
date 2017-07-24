using FoxTunes.Interfaces;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace FoxTunes
{
    /// <summary>
    /// Interaction logic for Library.xaml
    /// </summary>
    public partial class Library : UserControl
    {
        public Library()
        {
            InitializeComponent();
        }

        public ICore Core
        {
            get
            {
                return this.DataContext as ICore;
            }
        }

        protected override void OnDragEnter(DragEventArgs e)
        {
            var effects = DragDropEffects.None;
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                effects = DragDropEffects.Copy;
            }
            e.Effects = effects;
            base.OnDragEnter(e);
        }

        protected override void OnDrop(DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var paths = e.Data.GetData(DataFormats.FileDrop) as IEnumerable<string>;
                this.Core.Managers.Library.Add(paths);
                this.Core.Components.Database.SaveChanges();
            }
            base.OnDrop(e);
        }
    }
}
