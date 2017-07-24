using FoxTunes.Interfaces;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace FoxTunes.Managers
{
    public class LibraryManager : StandardManager, ILibraryManager
    {
        public ILibrary Library { get; private set; }

        public IDatabase Database { get; private set; }

        public ILibraryItemFactory LibraryItemFactory { get; private set; }

        public IPlaybackManager PlaybackManager { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Library = core.Components.Library;
            this.Database = core.Components.Database;
            this.LibraryItemFactory = core.Factories.LibraryItem;
            this.PlaybackManager = core.Managers.Playback;
            base.InitializeComponent(core);
        }

        public void Add(IEnumerable<string> paths)
        {
            foreach (var path in paths)
            {
                if (Directory.Exists(path))
                {
                    this.AddDirectory(path);
                }
                else if (File.Exists(path))
                {
                    this.AddFile(path);
                }
            }
        }

        protected virtual void AddDirectory(string directoryName)
        {
            var fileNames = Directory.GetFiles(directoryName, "*.*", SearchOption.AllDirectories);
            foreach (var fileName in fileNames)
            {
                this.AddFile(fileName);
            }
        }

        protected virtual void AddFile(string fileName)
        {
            if (!this.PlaybackManager.IsSupported(fileName))
            {
                return;
            }
            this.Items.Add(this.LibraryItemFactory.Create(fileName));
        }

        public void Clear()
        {
            this.Library.Items.Clear();
        }

        public ObservableCollection<LibraryItem> Items
        {
            get
            {
                return this.Library.Items;
            }
        }
    }
}
