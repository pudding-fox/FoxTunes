using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;

namespace FoxTunes
{
    /// <summary>
    /// Interaction logic for MetaDataEditorWindow.xaml
    /// </summary>
    public partial class MetaDataEditorWindow : WindowBase
    {
        const int STARTUP_LOCATION_OFFSET = 90;

        protected static ILogger Logger
        {
            get
            {
                return LogManager.Logger;
            }
        }

        static MetaDataEditorWindow()
        {
            Instances = new List<WeakReference<MetaDataEditorWindow>>();
        }

        private static IList<WeakReference<MetaDataEditorWindow>> Instances { get; set; }

        public static IEnumerable<MetaDataEditorWindow> Active
        {
            get
            {
                lock (Instances)
                {
                    return Instances
                        .Where(instance => instance != null && instance.IsAlive)
                        .Select(instance => instance.Target)
                        .ToArray();
                }
            }
        }

        protected static void OnActiveChanged(MetaDataEditorWindow sender)
        {
            if (ActiveChanged == null)
            {
                return;
            }
            ActiveChanged(sender, EventArgs.Empty);
        }

        public static event EventHandler ActiveChanged;

        public MetaDataEditorWindow()
        {
            var instance = Active.LastOrDefault();
            if (instance != null)
            {
                this.Left = instance.Left + STARTUP_LOCATION_OFFSET;
                this.Top = instance.Top + STARTUP_LOCATION_OFFSET;
                this.Width = instance.Width;
                this.Height = instance.Height;
            }
            else if (!global::FoxTunes.Properties.Settings.Default.MetaDataWindowBounds.IsEmpty())
            {
                if (ScreenHelper.WindowBoundsVisible(global::FoxTunes.Properties.Settings.Default.MetaDataWindowBounds))
                {
                    this.Left = global::FoxTunes.Properties.Settings.Default.MetaDataWindowBounds.Left;
                    this.Top = global::FoxTunes.Properties.Settings.Default.MetaDataWindowBounds.Top;
                }
                this.Width = global::FoxTunes.Properties.Settings.Default.MetaDataWindowBounds.Width;
                this.Height = global::FoxTunes.Properties.Settings.Default.MetaDataWindowBounds.Height;
            }
            else
            {
                this.Width = 400;
                this.Height = 800;
            }
            if (double.IsNaN(this.Left) || double.IsNaN(this.Top))
            {
                this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }
            this.InitializeComponent();
            lock (Instances)
            {
                Instances.Add(new WeakReference<MetaDataEditorWindow>(this));
            }
            OnActiveChanged(this);
        }

        public void Show(IFileData[] fileDatas)
        {
            switch (fileDatas.Length)
            {
                case 1:
                    var title = Path.GetFileNameWithoutExtension(fileDatas[0].FileName);
                    var metaDatas = fileDatas[0].MetaDatas;
                    if (metaDatas != null)
                    {
                        foreach (var metaData in metaDatas)
                        {
                            if (!string.Equals(metaData.Name, CommonMetaData.Title, StringComparison.OrdinalIgnoreCase))
                            {
                                continue;
                            }
                            title = metaData.Value;
                            break;
                        }
                    }
                    this.Title = string.Format("Tags - {0}", title);
                    break;
                default:
                    this.Title = string.Format("Tags - {0} items", fileDatas.Length);
                    break;
            }
            this.MetaDataEditor.Edit(fileDatas);
            this.Show();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            global::FoxTunes.Properties.Settings.Default.MetaDataWindowBounds = this.RestoreBounds;
            global::FoxTunes.Properties.Settings.Default.Save();
            base.OnClosing(e);
        }

        protected override void OnClosed(EventArgs e)
        {
            this.Dispose();
            base.OnClosed(e);
        }

        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.IsDisposed || !disposing)
            {
                return;
            }
            this.OnDisposing();
            this.IsDisposed = true;
        }

        protected virtual void OnDisposing()
        {
            lock (Instances)
            {
                for (var a = Instances.Count - 1; a >= 0; a--)
                {
                    var instance = Instances[a];
                    if (instance == null || !instance.IsAlive)
                    {
                        Instances.RemoveAt(a);
                    }
                    else if (object.ReferenceEquals(this, instance.Target))
                    {
                        Instances.RemoveAt(a);
                    }
                }
            }
            OnActiveChanged(this);
        }

        ~MetaDataEditorWindow()
        {
            Logger.Write(typeof(MetaDataEditorWindow), LogLevel.Error, "Component was not disposed: {0}", this.GetType().Name);
            try
            {
                this.Dispose(true);
            }
            catch
            {
                //Nothing can be done, never throw on GC thread.
            }
        }
    }
}
