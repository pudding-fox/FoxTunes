using FoxTunes.Interfaces;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FoxTunes
{
    /// <summary>
    /// Interaction logic for Playlist.xaml
    /// </summary>
    public partial class Playlist : UserControl
    {
        public Playlist()
        {
            this.InitializeComponent();
        }

        public ICore Core
        {
            get
            {
                return this.DataContext as ICore;
            }
        }

        private void ListBox_DragEnter(object sender, DragEventArgs e)
        {
            var effects = DragDropEffects.None;
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                effects = DragDropEffects.Copy;
            }
            e.Effects = effects;
        }

        private void ListBox_DragLeave(object sender, DragEventArgs e)
        {

        }

        private void ListBox_DragOver(object sender, DragEventArgs e)
        {

        }

        private void ListBox_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var paths = e.Data.GetData(DataFormats.FileDrop) as IEnumerable<string>;
                foreach (var path in paths)
                {
                    if (Directory.Exists(path))
                    {
                        this.Core.Managers.Playlist.AddDirectory(path);
                    }
                    else if (File.Exists(path))
                    {
                        this.Core.Managers.Playlist.AddFile(path);
                    }
                }
                this.Core.Managers.Playlist.Save();
            }
        }
    }
}
