using FoxTunes.Interfaces;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System;
using System.Linq;

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
            var sequence = this.GetInsertSequence();
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var paths = e.Data.GetData(DataFormats.FileDrop) as IEnumerable<string>;
                this.Core.Managers.Playlist.Add(sequence, paths);
            }
            if (e.Data.GetDataPresent(typeof(ObservableCollection<LibraryItem>)))
            {
                var items = e.Data.GetData(typeof(ObservableCollection<LibraryItem>)) as ObservableCollection<LibraryItem>;
                this.Core.Managers.Playlist.Add(sequence, items);
            }
            base.OnDrop(e);
        }

        protected virtual int GetInsertSequence()
        {
            var viewModel = this.FindResource("ViewModel") as global::FoxTunes.ViewModel.Playlist;
            if (!viewModel.InsertActive)
            {
                if (!this.Core.Components.Playlist.Query.Any())
                {
                    return 0;
                }
                return this.Core.Components.Playlist.Query.Max(playlistItem => playlistItem.Sequence) + 1;
            }
            return viewModel.InsertIndex + viewModel.InsertOffset;
        }
    }
}
