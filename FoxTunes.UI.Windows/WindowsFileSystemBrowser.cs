using FoxTunes.Interfaces;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;

namespace FoxTunes
{
    public class WindowsFileSystemBrowser : StandardComponent, IFileSystemBrowser
    {
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
                     InitialDirectory = options.InitialDirectory,
                     Filter = GetFilter(options.Filters),
                     Multiselect = options.Flags.HasFlag(BrowseFlags.Multiselect),
                 };
                 var success = default(bool?);
                 if (Windows.ActiveWindow != null)
                 {
                     success = dialog.ShowDialog(Windows.ActiveWindow);
                 }
                 else
                 {
                     //TODO: Hack: This temporary topmost window ensures that the dialog is focused and visible.
                     var window = new Window()
                     {
                         Topmost = true
                     };
                     success = dialog.ShowDialog(window);
                 }
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
                    dialog.SelectedPath = options.InitialDirectory;
                    var result = default(global::System.Windows.Forms.DialogResult);
                    if (Windows.ActiveWindow != null)
                    {
                        result = dialog.ShowDialog(new Win32Window(Windows.ActiveWindow.GetHandle()));
                    }
                    else
                    {
                        //TODO: Hack: This temporary topmost window ensures that the dialog is focused and visible.
                        var window = new Window()
                        {
                            Topmost = true
                        };
                        result = dialog.ShowDialog(new Win32Window(window.GetHandle()));
                    }
                    var success = default(bool);
                    switch (result)
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
                                extension => string.Format("*.{0}", extension)
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
