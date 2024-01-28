using FoxTunes.Integration;
using FoxTunes.Interfaces;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.UserInterface)]
    public class WindowsFileSystemBrowser : StandardComponent, IFileSystemBrowser
    {
        public void Select(string fileName)
        {
            Explorer.Select(fileName);
        }

        public BrowseResult Browse(BrowseOptions options)
        {
            if (options.Flags.HasFlag(BrowseFlags.File))
            {
                return this.BrowseFile(options);
            }
            if (options.Flags.HasFlag(BrowseFlags.Folder))
            {
                return this.BrowseFolder(options);
            }
            throw new NotImplementedException();
        }

        protected virtual BrowseResult BrowseFile(BrowseOptions options)
        {
            var result = default(BrowseResult);
            Windows.Invoke(() =>
             {
                 var dialog = new OpenFileDialog()
                 {
                     Title = options.Title,
                     Filter = GetFilter(options.Filters),
                     Multiselect = options.Flags.HasFlag(BrowseFlags.Multiselect),
                 };
                 if (File.Exists(options.Path))
                 {
                     dialog.InitialDirectory = Path.GetDirectoryName(options.Path);
                     dialog.FileName = options.Path;
                 }
                 else if (Directory.Exists(options.Path))
                 {
                     dialog.InitialDirectory = options.Path;
                 }
                 var window = Windows.ActiveWindow;
                 var success = dialog.ShowDialog(window);
                 result = new BrowseResult(dialog.FileNames, success.GetValueOrDefault());
                 //TODO: Bad .Wait().
             }).Wait();
            return result;
        }

        protected virtual BrowseResult BrowseFolder(BrowseOptions options)
        {
            var result = default(BrowseResult);
            Windows.Invoke(() =>
            {
                //TODO: Use only WPF frameworks.
                using (var dialog = new global::System.Windows.Forms.FolderBrowserDialog())
                {
                    dialog.Description = options.Title;
                    if (Directory.Exists(options.Path))
                    {
                        dialog.SelectedPath = options.Path;
                    }
                    var window = Windows.ActiveWindow;
                    var success = default(bool);
                    switch (dialog.ShowDialog(new Win32Window(window.GetHandle())))
                    {
                        case global::System.Windows.Forms.DialogResult.OK:
                            success = true;
                            break;
                    }
                    result = new BrowseResult(new[] { dialog.SelectedPath }, success);
                }
                //TODO: Bad .Wait().
            }).Wait();
            return result;
        }

        private static string GetFilter(IEnumerable<BrowseFilter> filters)
        {
            return string.Join(
                "|",
                filters.Select(
                    filter => string.Format(
                        "{0} ({1})|{1}",
                        filter.Name,
                        string.Join(
                            ";",
                            filter.Extensions.Select(
                                extension => string.Format(
                                    "*.{0}",
                                    extension.TrimStart('.')
                                )
                            )
                        )
                    )
                )
            );
        }

        //TODO: Use only WPF frameworks.
        private class Win32Window : global::System.Windows.Forms.IWin32Window
        {
            public Win32Window(IntPtr handle)
            {
                this.Handle = handle;
            }

            public IntPtr Handle { get; private set; }
        }
    }
}
