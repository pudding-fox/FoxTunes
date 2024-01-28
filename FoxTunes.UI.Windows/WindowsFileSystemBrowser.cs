using FoxTunes.Integration;
using FoxTunes.Interfaces;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;

namespace FoxTunes
{
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
            return this.WithApartmentState(ApartmentState.STA, () =>
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
                 var window = this.GetActiveWindow();
                 var success = dialog.ShowDialog(window);
                 return new BrowseResult(dialog.FileNames, success.GetValueOrDefault());
             });
        }

        protected virtual BrowseResult BrowseFolder(BrowseOptions options)
        {
            return this.WithApartmentState(ApartmentState.STA, () =>
            {
                //TODO: Use only WPF frameworks.
                using (var dialog = new global::System.Windows.Forms.FolderBrowserDialog())
                {
                    dialog.Description = options.Title;
                    if (Directory.Exists(options.Path))
                    {
                        dialog.SelectedPath = options.Path;
                    }
                    var window = this.GetActiveWindow();
                    var success = default(bool);
                    switch (dialog.ShowDialog(new Win32Window(window.GetHandle())))
                    {
                        case global::System.Windows.Forms.DialogResult.OK:
                            success = true;
                            break;
                    }
                    return new BrowseResult(new[] { dialog.SelectedPath }, success);
                }
            });
        }

        protected virtual T WithApartmentState<T>(ApartmentState apartmentState, Func<T> func)
        {
            if (Thread.CurrentThread.GetApartmentState() == apartmentState)
            {
                return func();
            }
            Logger.Write(this, LogLevel.Debug, "Current thread does not have the required apartment state \"{0}\", creating a new one.", Enum.GetName(typeof(ApartmentState), apartmentState));
            var result = default(T);
            var thread = new Thread(() => result = func());
            thread.IsBackground = true;
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            //TODO: No timeout, could deadlock.
            thread.Join();
            return result;
        }

        protected virtual Window GetActiveWindow()
        {
            if (Windows.IsSettingsWindowCreated && Windows.SettingsWindow.IsVisible)
            {
                return Windows.SettingsWindow;
            }
            if (Windows.ActiveWindow != null)
            {
                return Windows.ActiveWindow;
            }
            //TODO: Hack: This temporary topmost window ensures that the dialog is focused and visible.
            Logger.Write(this, LogLevel.Debug, "Creating temporary window to host windows browse dialog.");
            var window = new Window()
            {
                Topmost = true
            };
            return window;
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
