using FoxTunes.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Interop;
using System.Windows.Threading;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.UserInterface)]
    [PlatformDependency(Major = 6, Minor = 1)]
    public class TaskbarThumbnailBehaviour : StandardBehaviour, IConfigurableComponent, IDisposable
    {
        const int UPDATE_INTERVAL = 1000;

        private static readonly object SyncRoot = new object();

        public TaskbarThumbnailBehaviour()
        {
            this.Callback = new HwndSourceHook(this.OnCallback);
            this.Windows = new ConcurrentDictionary<IntPtr, TaskbarThumbnailWindowFlags>();
            this.Thumbnails = new ConcurrentDictionary<IntPtr, TaskbarThumbnail>();
        }

        public Timer Timer { get; private set; }

        public HwndSourceHook Callback { get; private set; }

        public ConcurrentDictionary<IntPtr, TaskbarThumbnailWindowFlags> Windows { get; private set; }

        protected virtual bool HasFlags(IntPtr handle)
        {
            var flags = default(TaskbarThumbnailWindowFlags);
            return this.Windows.TryGetValue(handle, out flags);
        }

        protected virtual bool HasFlag(IntPtr handle, TaskbarThumbnailWindowFlags flag)
        {
            var flags = default(TaskbarThumbnailWindowFlags);
            if (!this.Windows.TryGetValue(handle, out flags))
            {
                return false;
            }
            return flags.HasFlag(flag);
        }

        protected virtual bool AddFlag(IntPtr handle, TaskbarThumbnailWindowFlags flag)
        {
            var flags = default(TaskbarThumbnailWindowFlags);
            if (!this.Windows.TryGetValue(handle, out flags))
            {
                return false;
            }
            return this.Windows.TryUpdate(handle, flags | flag, flags);
        }

        protected virtual bool RemoveFlag(IntPtr handle, TaskbarThumbnailWindowFlags flag)
        {
            var flags = default(TaskbarThumbnailWindowFlags);
            if (!this.Windows.TryGetValue(handle, out flags))
            {
                return false;
            }
            return this.Windows.TryUpdate(handle, flags & ~flag, flags);
        }

        public ConcurrentDictionary<IntPtr, TaskbarThumbnail> Thumbnails { get; private set; }

        protected virtual bool HasThumbnail(IntPtr handle, string name)
        {
            var thumbnail = default(TaskbarThumbnail);
            if (!this.Thumbnails.TryGetValue(handle, out thumbnail))
            {
                return false;
            }
            return string.Equals(thumbnail.Name, name, StringComparison.OrdinalIgnoreCase);
        }

        protected virtual bool GetThumbnail(IntPtr handle, out TaskbarThumbnail thumbnail)
        {
            return this.Thumbnails.TryGetValue(handle, out thumbnail);
        }

        protected virtual void SetThumbnail(IntPtr handle, TaskbarThumbnail thumbnail)
        {
            this.Thumbnails.AddOrUpdate(handle, thumbnail);
        }

        protected virtual bool RemoveThumbnail(IntPtr handle)
        {
            return this.Thumbnails.TryRemove(handle);
        }

        public IPlaybackManager PlaybackManager { get; private set; }

        public IUserInterface UserInterface { get; private set; }

        public IArtworkProvider ArtworkProvider { get; private set; }

        public ILibraryBrowser LibraryBrowser { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public BooleanConfigurationElement Enabled { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.PlaybackManager = core.Managers.Playback;
            this.UserInterface = core.Components.UserInterface;
            this.UserInterface.WindowCreated += this.OnWindowCreated;
            this.UserInterface.WindowDestroyed += this.OnWindowDestroyed;
            this.ArtworkProvider = core.Components.ArtworkProvider;
            this.LibraryBrowser = core.Components.LibraryBrowser;
            this.Configuration = core.Components.Configuration;
            this.Enabled = this.Configuration.GetElement<BooleanConfigurationElement>(
                TaskbarThumbnailBehaviourConfiguration.SECTION,
                TaskbarThumbnailBehaviourConfiguration.ENABLED_ELEMENT
            );
            this.Enabled.ConnectValue(value =>
            {
                if (value)
                {
                    this.Enable();
                }
                else
                {
                    this.Disable();
                }
            });
            base.InitializeComponent(core);
        }

        protected virtual void OnWindowCreated(object sender, UserInterfaceWindowEventArgs e)
        {
            if (e.Window.Role != UserInterfaceWindowRole.Main)
            {
                //Only create taskbar thumbnail for main windows.
                return;
            }
            this.Windows.TryAdd(e.Window.Handle, TaskbarThumbnailWindowFlags.None);
        }

        protected virtual void OnWindowDestroyed(object sender, UserInterfaceWindowEventArgs e)
        {
            this.AddFlag(e.Window.Handle, TaskbarThumbnailWindowFlags.Destroyed);
        }

        public void Enable()
        {
            lock (SyncRoot)
            {
                if (this.Timer == null)
                {
                    this.Timer = new Timer();
                    this.Timer.Interval = UPDATE_INTERVAL;
                    this.Timer.Elapsed += this.OnElapsed;
                    this.Timer.AutoReset = false;
                    this.Timer.Start();
                    Logger.Write(this, LogLevel.Debug, "Updater enabled.");
                }
            }
        }

        public void Disable()
        {
            lock (SyncRoot)
            {
                if (this.Timer != null)
                {
                    this.Timer.Stop();
                    this.Timer.Elapsed -= this.OnElapsed;
                    this.Timer.Dispose();
                    this.Timer = null;
                    Logger.Write(this, LogLevel.Debug, "Updater disabled.");
                }
            }
        }

        protected virtual async void OnElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                await this.Update().ConfigureAwait(false);
                lock (SyncRoot)
                {
                    if (this.Timer != null)
                    {
                        this.Timer.Start();
                    }
                }
            }
            catch
            {
                //Nothing can be done, never throw on background thread.
            }
        }

        protected virtual async Task Update()
        {
            foreach (var pair in this.Windows)
            {
                await this.Update(pair.Key, pair.Value).ConfigureAwait(false);
            }
        }

        protected virtual async Task Update(IntPtr handle, TaskbarThumbnailWindowFlags flags)
        {
            if (flags.HasFlag(TaskbarThumbnailWindowFlags.Destroyed))
            {
                if (flags.HasFlag(TaskbarThumbnailWindowFlags.Registered))
                {
                    this.RemoveHook(handle);
                }
                //TODO: We should remove the window from our table but it causes issues and we're likely shutting down now.
                return;
            }
            if (flags.HasFlag(TaskbarThumbnailWindowFlags.Error))
            {
                return;
            }
            if (this.Enabled.Value)
            {
                if (!flags.HasFlag(TaskbarThumbnailWindowFlags.Registered))
                {
                    this.AddHook(handle);
                }
                if (!await this.UpdateThumbnail(handle).ConfigureAwait(false))
                {
                    return;
                }
            }
            else
            {
                //Although we're no longer enabled, these features can't be disabled so we must keep them updated.
                if (flags.HasFlag(TaskbarThumbnailWindowFlags.ThumbnailCreated))
                {
                    if (!await this.UpdateThumbnail(handle).ConfigureAwait(false))
                    {
                        return;
                    }
                }
            }
        }

        protected virtual IntPtr OnCallback(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WindowsIconicThumbnail.WM_DWMSENDICONICTHUMBNAIL)
            {
                var low = (lParam.ToInt64() & 0x0000FFFF);
                var high = (lParam.ToInt64() & 0xFFFF0000) >> 16;
                var task = this.OnSendIconicThumbnail(hwnd, Convert.ToInt32(high), Convert.ToInt32(low));
            }
            else if (msg == WindowsIconicThumbnail.WM_DWMSENDICONICLIVEPREVIEWBITMAP)
            {
                this.OnSendIconicLivePreviewBitmap(hwnd);
            }
            else if (msg == WindowMessages.WM_TASKBARCREATED)
            {
                this.OnTaskBarCreated(hwnd);
            }
            return IntPtr.Zero;
        }

        protected virtual void OnTaskBarCreated(IntPtr handle)
        {
            if (this.HasFlags(handle))
            {
                //Handing an event for an unknown window?
                return;
            }
            //TODO: Should we be destroying the image list?
            this.Windows[handle] = TaskbarThumbnailWindowFlags.Registered;
        }

        protected virtual void AddHook(IntPtr handle)
        {
            Logger.Write(this, LogLevel.Debug, "Adding Windows event handler.");
            var source = HwndSource.FromHwnd(handle);
            if (source == null)
            {
                Logger.Write(this, LogLevel.Warn, "No such window for handle: {0}", handle);
                this.AddFlag(handle, TaskbarThumbnailWindowFlags.Error);
                return;
            }
            source.AddHook(this.Callback);
            this.AddFlag(handle, TaskbarThumbnailWindowFlags.Registered);
        }

        protected virtual void RemoveHook(IntPtr handle)
        {
            Logger.Write(this, LogLevel.Debug, "Removing Windows event handler.");
            var source = HwndSource.FromHwnd(handle);
            if (source == null)
            {
                Logger.Write(this, LogLevel.Warn, "No such window for handle: {0}", handle);
                this.AddFlag(handle, TaskbarThumbnailWindowFlags.Error);
                return;
            }
            source.RemoveHook(this.Callback);
            this.RemoveFlag(handle, TaskbarThumbnailWindowFlags.Registered);
        }

        protected virtual async Task<bool> UpdateThumbnail(IntPtr handle)
        {
            var source = HwndSource.FromHwnd(handle);
            if (source == null)
            {
                Logger.Write(this, LogLevel.Warn, "No such window for handle: {0}", handle);
                return false;
            }
            if (!this.HasFlag(handle, TaskbarThumbnailWindowFlags.ThumbnailCreated))
            {
                int forceIconicRepresentation = 1;
                int hasIconicBitmap = 1;
                var result = default(WindowsIconicThumbnail.HResult);
                await this.Invoke(
                   source.Dispatcher,
                   () => result = WindowsIconicThumbnail.DwmSetWindowAttribute(
                        handle,
                        WindowsIconicThumbnail.DWM_FORCE_ICONIC_REPRESENTATION,
                        ref forceIconicRepresentation,
                        sizeof(int)
                    )
                ).ConfigureAwait(false);
                if (result != WindowsIconicThumbnail.HResult.Ok)
                {
                    Logger.Write(this, LogLevel.Warn, "Failed to set window attribute DWM_FORCE_ICONIC_REPRESENTATION: {0}", Enum.GetName(typeof(WindowsIconicThumbnail.HResult), result));
                    this.AddFlag(handle, TaskbarThumbnailWindowFlags.Error);
                    return false;
                }
                await this.Invoke(
                   source.Dispatcher,
                   () => result = WindowsIconicThumbnail.DwmSetWindowAttribute(
                        handle,
                        WindowsIconicThumbnail.DWM_HAS_ICONIC_BITMAP,
                        ref hasIconicBitmap,
                        sizeof(int)
                    )
                ).ConfigureAwait(false);
                if (result != WindowsIconicThumbnail.HResult.Ok)
                {
                    Logger.Write(this, LogLevel.Warn, "Failed to set window attribute DWM_HAS_ICONIC_BITMAP: {0}", Enum.GetName(typeof(WindowsIconicThumbnail.HResult), result));
                    this.AddFlag(handle, TaskbarThumbnailWindowFlags.Error);
                    return false;
                }
                this.AddFlag(handle, TaskbarThumbnailWindowFlags.ThumbnailCreated);
            }
            var thumbnail = await this.GetThumbnail().ConfigureAwait(false);
            if (!this.HasThumbnail(handle, thumbnail.Name))
            {
                Logger.Write(this, LogLevel.Debug, "Iconic thumbnail: {0}", thumbnail.Name);
                this.SetThumbnail(handle, thumbnail);
                var result = default(WindowsIconicThumbnail.HResult);
                await this.Invoke(
                    source.Dispatcher,
                    () => result = WindowsIconicThumbnail.DwmInvalidateIconicBitmaps(handle)
                ).ConfigureAwait(false);
                if (result != WindowsIconicThumbnail.HResult.Ok)
                {
                    Logger.Write(this, LogLevel.Warn, "Failed to invalidate iconic thumbnail: {0}", Enum.GetName(typeof(WindowsIconicThumbnail.HResult), result));
                    this.AddFlag(handle, TaskbarThumbnailWindowFlags.Error);
                    return false;
                }
            }
            return true;
        }

        protected virtual Task<bool> OnSendIconicThumbnail(IntPtr handle, int width, int height)
        {
            var thumbnail = default(TaskbarThumbnail);
            if (!this.GetThumbnail(handle, out thumbnail))
            {
#if NET40
                return TaskEx.FromResult(false);
#else
                return Task.FromResult(false);
#endif
            }
            return this.OnSendIconicThumbnail(handle, thumbnail.Scale(width, height));
        }

        protected virtual async Task<bool> OnSendIconicThumbnail(IntPtr handle, Bitmap bitmap)
        {
            var source = HwndSource.FromHwnd(handle);
            if (source == null)
            {
                Logger.Write(this, LogLevel.Warn, "No such window for handle: {0}", handle);
                return false;
            }
            var bitmapSection = default(IntPtr);
            if (!WindowsImaging.CreateDIBSection(bitmap, bitmap.Width, -bitmap.Height/* This isn't a mistake, DIB is top down. */, out bitmapSection))
            {
                Logger.Write(this, LogLevel.Warn, "Failed to create native bitmap.");
                this.AddFlag(handle, TaskbarThumbnailWindowFlags.Error);
                return false;
            }
            var result = default(WindowsIconicThumbnail.HResult);
            await this.Invoke(
                source.Dispatcher,
                () => result = WindowsIconicThumbnail.DwmSetIconicThumbnail(handle, bitmapSection, 0)
            ).ConfigureAwait(false);
            WindowsImaging.DeleteObject(bitmapSection);
            if (result != WindowsIconicThumbnail.HResult.Ok)
            {
                Logger.Write(this, LogLevel.Warn, "Failed to set iconic thumbnail: {0}", Enum.GetName(typeof(WindowsIconicThumbnail.HResult), result));
                this.AddFlag(handle, TaskbarThumbnailWindowFlags.Error);
                return false;
            }
            return true;
        }

        protected virtual async Task<TaskbarThumbnail> GetThumbnail()
        {
            var outputStream = this.PlaybackManager.CurrentStream;
            if (outputStream != null)
            {
                var fileData = default(IFileData);
                if (outputStream.PlaylistItem.LibraryItem_Id.HasValue)
                {
                    fileData = this.LibraryBrowser.Get(outputStream.PlaylistItem.LibraryItem_Id.Value);
                }
                else
                {
                    fileData = outputStream.PlaylistItem;
                }
                var fileName = await this.ArtworkProvider.Find(fileData, ArtworkType.FrontCover).ConfigureAwait(false);
                if (!string.IsNullOrEmpty(fileName) && File.Exists(fileName))
                {
                    return TaskbarThumbnail.GetOrAdd(fileName);
                }
            }
            return TaskbarThumbnail.Placeholder;
        }

        protected virtual void OnSendIconicLivePreviewBitmap(IntPtr handle)
        {
            //Nothing to do.
        }

        protected virtual Task Invoke(Dispatcher dispatcher, Action action)
        {
#if NET40
            var source = new TaskCompletionSource<bool>();
            dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    action();
                }
                finally
                {
                    source.SetResult(false);
                }
            }));
            return source.Task;
#else
            return dispatcher.BeginInvoke(action).Task;
#endif
        }

        public IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return TaskbarThumbnailBehaviourConfiguration.GetConfigurationSections();
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
            if (this.Thumbnails != null)
            {
                foreach (var pair in this.Thumbnails)
                {
                    if (pair.Value != null)
                    {
                        pair.Value.Dispose();
                    }
                }
            }
            if (this.UserInterface != null)
            {
                this.UserInterface.WindowCreated -= this.OnWindowCreated;
                this.UserInterface.WindowDestroyed -= this.OnWindowDestroyed;
            }
            this.Disable();
        }

        ~TaskbarThumbnailBehaviour()
        {
            Logger.Write(this, LogLevel.Error, "Component was not disposed: {0}", this.GetType().Name);
            try
            {
                this.Dispose(true);
            }
            catch
            {
                //Nothing can be done, never throw on GC thread.
            }
        }

        public class TaskbarThumbnail : IDisposable
        {
            const int CAPACITY = 10;

            const string PLACEHOLDER = "FoxTunes.Core.Windows.Images.fox.ico";

            private static readonly CappedDictionary<string, TaskbarThumbnail> Store = new CappedDictionary<string, TaskbarThumbnail>(CAPACITY, StringComparer.OrdinalIgnoreCase);

            public TaskbarThumbnail(string name, Bitmap sourceBitmap)
            {
                this.Name = name;
                this.SourceBitmap = sourceBitmap;
            }

            public string Name { get; private set; }

            public Bitmap SourceBitmap { get; private set; }

            public Bitmap ScaledBitmap { get; private set; }

            public int Count { get; set; }

            public bool IsPlaceholder
            {
                get
                {
                    return string.Equals(this.Name, PLACEHOLDER, StringComparison.OrdinalIgnoreCase);
                }
            }

            public Bitmap Scale(int width, int height)
            {
                lock (this.SourceBitmap)
                {
                    if (this.ScaledBitmap != null && this.ScaledBitmap.Width == width && this.ScaledBitmap.Height == height)
                    {
                        return this.ScaledBitmap;
                    }
                    Logger.Write(typeof(TaskbarThumbnail), LogLevel.Debug, "Resizing image {0}: {1}x{2}", this.Name, width, height);
                    this.ScaledBitmap = WindowsImaging.Resize(this.SourceBitmap, width, height, true);
                }
                return this.ScaledBitmap;
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
                if (this.SourceBitmap != null)
                {
                    this.SourceBitmap.Dispose();
                }
                if (this.ScaledBitmap != null)
                {
                    this.ScaledBitmap.Dispose();
                }
            }

            ~TaskbarThumbnail()
            {
                Logger.Write(this.GetType(), LogLevel.Error, "Component was not disposed: {0}", this.GetType().Name);
                try
                {
                    this.Dispose(true);
                }
                catch
                {
                    //Nothing can be done, never throw on GC thread.
                }
            }

            public static TaskbarThumbnail GetOrAdd(string fileName)
            {
                return Store.GetOrAdd(fileName, () =>
                {
                    Logger.Write(typeof(TaskbarThumbnail), LogLevel.Debug, "Creating image: {0}", fileName);
                    var stream = File.OpenRead(fileName);
                    return new TaskbarThumbnail(fileName, (Bitmap)Bitmap.FromStream(stream));
                });
            }

            public static readonly TaskbarThumbnail Placeholder = GetPlaceholder();

            private static TaskbarThumbnail GetPlaceholder()
            {
                Logger.Write(typeof(TaskbarThumbnail), LogLevel.Debug, "Creating image: {0}", PLACEHOLDER);
                var stream = typeof(TaskbarThumbnailBehaviour).Assembly.GetManifestResourceStream(PLACEHOLDER);
                return new TaskbarThumbnail(PLACEHOLDER, (Bitmap)Bitmap.FromStream(stream));
            }
        }
    }

    [Flags]
    public enum TaskbarThumbnailWindowFlags : byte
    {
        None = 0,
        Registered = 1,
        Error = 8,
        Destroyed = 16,
        ThumbnailCreated = 64
    }
}
