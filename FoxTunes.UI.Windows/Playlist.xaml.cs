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
            if (e.Data.GetDataPresent(typeof(LibraryHierarchyNode)))
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
            if (e.Data.GetDataPresent(typeof(LibraryHierarchyNode)))
            {
                var libraryHierarchyNode = e.Data.GetData(typeof(LibraryHierarchyNode)) as LibraryHierarchyNode;
                this.AddToPlaylist(libraryHierarchyNode);
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

        private void AddToPlaylist(LibraryHierarchyNode libraryHierarchyNode)
        {
            var sequence = default(int);
            if (this.TryGetInsertSequence(out sequence))
            {
                this.Core.Managers.Playlist.Insert(sequence, libraryHierarchyNode);
            }
            else
            {
                this.Core.Managers.Playlist.Add(libraryHierarchyNode);
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
