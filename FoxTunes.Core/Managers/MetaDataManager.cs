using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.Database)]
    public class MetaDataManager : StandardManager, IMetaDataManager
    {
        public ICore Core { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Core = core;
            base.InitializeComponent(core);
        }

        public async Task Rescan(IEnumerable<IFileData> fileDatas)
        {
            var libraryItems = fileDatas.OfType<LibraryItem>().ToArray();
            var playlistItems = fileDatas.OfType<PlaylistItem>().ToArray();
            if (libraryItems.Any())
            {
                using (var task = new RefreshLibraryMetaDataTask(libraryItems))
                {
                    task.InitializeComponent(this.Core);
                    await this.OnBackgroundTask(task).ConfigureAwait(false);
                    await task.Run().ConfigureAwait(false);
                }
            }
            if (playlistItems.Any())
            {
                using (var task = new RefreshPlaylistMetaDataTask(playlistItems))
                {
                    task.InitializeComponent(this.Core);
                    await this.OnBackgroundTask(task).ConfigureAwait(false);
                    await task.Run().ConfigureAwait(false);
                }
            }
        }

        public async Task Save(IEnumerable<IFileData> fileDatas, bool write, bool report, params string[] names)
        {
            var libraryItems = fileDatas.OfType<LibraryItem>().ToArray();
            var playlistItems = fileDatas.OfType<PlaylistItem>().ToArray();
            if (libraryItems.Any())
            {
                using (var task = new WriteLibraryMetaDataTask(libraryItems, write, names))
                {
                    task.InitializeComponent(this.Core);
                    await this.OnBackgroundTask(task).ConfigureAwait(false);
                    await task.Run().ConfigureAwait(false);
                    if (report && task.Errors.Any())
                    {
                        this.OnReport(libraryItems, task.Errors);
                    }
                }
            }
            if (playlistItems.Any())
            {
                using (var task = new WritePlaylistMetaDataTask(playlistItems, names, write))
                {
                    task.InitializeComponent(this.Core);
                    await this.OnBackgroundTask(task).ConfigureAwait(false);
                    await task.Run().ConfigureAwait(false);
                    if (report && task.Errors.Any())
                    {
                        this.OnReport(playlistItems, task.Errors);
                    }
                }
            }
        }

        public async Task Synchronize()
        {
            using (var task = new SynchronizeMetaDataTask())
            {
                task.InitializeComponent(this.Core);
                await this.OnBackgroundTask(task).ConfigureAwait(false);
                await task.Run().ConfigureAwait(false);
            }
        }

        public async Task<bool> Synchronize(IEnumerable<IFileData> fileDatas, params string[] names)
        {
            var libraryItems = fileDatas.OfType<LibraryItem>().ToArray();
            var playlistItems = fileDatas.OfType<PlaylistItem>().ToArray();
            var result = true;
            if (libraryItems.Any())
            {
                using (var task = new SynchronizeLibraryMetaDataTask(libraryItems, names))
                {
                    task.InitializeComponent(this.Core);
                    await this.OnBackgroundTask(task).ConfigureAwait(false);
                    await task.Run().ConfigureAwait(false);
                    if (task.Errors.Any())
                    {
                        result = false;
                    }
                }
            }
            if (playlistItems.Any())
            {
                using (var task = new SynchronizePlaylistMetaDataTask(playlistItems, names))
                {
                    task.InitializeComponent(this.Core);
                    await this.OnBackgroundTask(task).ConfigureAwait(false);
                    await task.Run().ConfigureAwait(false);
                    if (task.Errors.Any())
                    {
                        result = false;
                    }
                }
            }
            return result;
        }

        protected virtual Task OnBackgroundTask(IBackgroundTask backgroundTask)
        {
            if (this.BackgroundTask == null)
            {
#if NET40
                return TaskEx.FromResult(false);
#else
                return Task.CompletedTask;
#endif
            }
            var e = new BackgroundTaskEventArgs(backgroundTask);
            this.BackgroundTask(this, e);
            return e.Complete();
        }

        public event BackgroundTaskEventHandler BackgroundTask;

        protected virtual void OnReport(IEnumerable<LibraryItem> libraryItems, IDictionary<LibraryItem, IList<string>> errors)
        {
            var report = new MetaDataManagerReport<LibraryItem>(libraryItems, errors);
            report.InitializeComponent(this.Core);
            this.OnReport(report);
        }

        protected virtual void OnReport(IEnumerable<PlaylistItem> playlistItems, IDictionary<PlaylistItem, IList<string>> errors)
        {
            var report = new MetaDataManagerReport<PlaylistItem>(playlistItems, errors);
            report.InitializeComponent(this.Core);
            this.OnReport(report);
        }

        protected virtual Task OnReport(IReport Report)
        {
            if (this.Report == null)
            {
#if NET40
                return TaskEx.FromResult(false);
#else
                return Task.CompletedTask;
#endif
            }
            var e = new ReportEventArgs(Report);
            this.Report(this, e);
            return e.Complete();
        }

        public event ReportEventHandler Report;

        public class MetaDataManagerReport<T> : BaseComponent, IReport where T : IFileData
        {
            public MetaDataManagerReport(IEnumerable<T> sequence, IDictionary<T, IList<string>> errors)
            {
                this.Sequence = sequence.ToDictionary(encoderItem => Guid.NewGuid());
                this.Errors = errors;
            }

            public IDictionary<Guid, T> Sequence { get; private set; }

            public IDictionary<T, IList<string>> Errors { get; private set; }

            public string Title
            {
                get
                {
                    return "Meta Data Writer";
                }
            }

            public string Description
            {
                get
                {
                    return string.Join(
                        Environment.NewLine,
                        this.Sequence.Values.Select(
                            element => this.GetDescription(element)
                        )
                    );
                }
            }

            protected virtual string GetDescription(T element)
            {
                var builder = new StringBuilder();
                var errors = default(IList<string>);
                builder.Append(element.FileName);
                if (this.Errors.TryGetValue(element, out errors))
                {
                    builder.AppendLine(" -> Error");
                    foreach (var error in errors)
                    {
                        builder.AppendLine('\t' + error);
                    }
                }
                else
                {
                    builder.AppendLine(" -> OK");
                }
                return builder.ToString();
            }

            public string[] Headers
            {
                get
                {
                    return new[]
                    {
                        "Path",
                        "Status"
                    };
                }
            }

            public IEnumerable<IReportRow> Rows
            {
                get
                {
                    return this.Sequence.Select(element => new ReportRow(element.Key, element.Value, this.Errors));
                }
            }

            public Action<Guid> Action
            {
                get
                {
                    return key =>
                    {
                        var element = default(T);
                        if (!this.Sequence.TryGetValue(key, out element) || !File.Exists(element.FileName))
                        {
                            return;
                        }
                        this.FileSystemBrowser.Select(element.FileName);
                    };
                }
            }

            public IFileSystemBrowser FileSystemBrowser { get; private set; }

            public override void InitializeComponent(ICore core)
            {
                this.FileSystemBrowser = core.Components.FileSystemBrowser;
                base.InitializeComponent(core);
            }

            public class ReportRow : IReportRow
            {
                public ReportRow(Guid id, T element, IDictionary<T, IList<string>> errors)
                {
                    this.Id = id;
                    this.Element = element;
                    this.Errors = errors;
                }

                public Guid Id { get; private set; }

                public T Element { get; private set; }

                public IDictionary<T, IList<string>> Errors { get; private set; }

                public string[] Values
                {
                    get
                    {
                        return new[]
                        {
                            this.Element.FileName,
                            this.Errors.ContainsKey(this.Element) ?
                                "Error" :
                                "OK"
                        };
                    }
                }
            }
        }
    }
}
