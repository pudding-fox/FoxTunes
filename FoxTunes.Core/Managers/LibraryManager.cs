using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
            var fileNames = new List<string>();
            foreach (var path in paths)
            {
                if (Directory.Exists(path))
                {
                    fileNames.AddRange(Directory.GetFiles(path, "*.*", SearchOption.AllDirectories));
                }
                else if (File.Exists(path))
                {
                    fileNames.Add(path);
                }
            }
            this.AddFiles(fileNames);
        }

        protected virtual void AddFiles(IEnumerable<string> fileNames)
        {
            var query =
                from fileName in fileNames
                where this.PlaybackManager.IsSupported(fileName)
                select this.LibraryItemFactory.Create(fileName);
            this.Library.Set.AddRange(query);
            this.OnUpdated();
        }

        protected virtual void OnUpdated()
        {
            if (this.Updated == null)
            {
                return;
            }
            this.Updated(this, EventArgs.Empty);
        }

        public event EventHandler Updated = delegate { };

        public void Clear()
        {
            this.Library.Set.Clear();
        }
    }
}
