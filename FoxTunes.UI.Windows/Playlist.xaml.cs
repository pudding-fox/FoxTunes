using FoxTunes.Interfaces;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

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

        protected override void OnDragEnter(DragEventArgs e)
        {
            var effects = DragDropEffects.None;
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                effects = DragDropEffects.Copy;
            }
            if (e.Data.GetDataPresent(typeof(ObservableCollection<LibraryItem>)))
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
                this.AddToPlaylist(paths);
            }
            if (e.Data.GetDataPresent(typeof(ObservableCollection<LibraryItem>)))
            {
                var items = e.Data.GetData(typeof(ObservableCollection<LibraryItem>)) as ObservableCollection<LibraryItem>;
                this.AddToPlaylist(items);
            }
            base.OnDrop(e);
        }

        private void AddToPlaylist(IEnumerable<string> paths)
        {
            var sequence = default(int);
            if (this.TryGetInsertSequence(out sequence))
            {
                this.Core.Managers.Playlist.Insert(sequence, paths);
            }
            else
            {
                this.Core.Managers.Playlist.Add(paths);
            }
        }

        private void AddToPlaylist(IEnumerable<LibraryItem> libraryItems)
        {
            var sequence = default(int);
            if (this.TryGetInsertSequence(out sequence))
            {
                this.Core.Managers.Playlist.Insert(sequence, libraryItems);
            }
            else
            {
                this.Core.Managers.Playlist.Add(libraryItems);
            }
        }

        protected virtual bool TryGetInsertSequence(out int sequence)
        {
            var viewModel = this.FindResource("ViewModel") as global::FoxTunes.ViewModel.Playlist;
            if (!viewModel.InsertActive)
            {
                sequence = 0;
                return false;
            }
            sequence = viewModel.InsertIndex + viewModel.InsertOffset;
            return true;
        }
    }
}
