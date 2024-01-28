using FoxTunes.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Interop;
using System.Windows.Threading;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.UserInterface)]
    public class TaskbarButtonsBehaviour : StandardBehaviour, IConfigurableComponent, IDisposable
    {
        const int UPDATE_INTERVAL = 1000;

        const string THUMBNAIL_PLACEHOLDER = "FoxTunes.Core.Windows.Images.fox.ico";

        private static bool IsThumbnailPlaceholder(string name)
        {
            return string.Equals(name, THUMBNAIL_PLACEHOLDER, StringComparison.OrdinalIgnoreCase);
        }

        private static readonly int WM_TASKBARCREATED;

        private static readonly object SyncRoot = new object();

        static TaskbarButtonsBehaviour()
        {
            try
            {
                WM_TASKBARCREATED = RegisterWindowMessage("TaskbarCreated");
            }
            catch
            {
                Logger.Write(typeof(TaskbarButtonsBehaviour), LogLevel.Warn, "Failed to register window message: TaskbarCreated");
            }
        }

        const int BUTTON_PREVIOUS = 0;

        const int BUTTON_PLAY_PAUSE = 1;

        const int BUTTON_NEXT = 2;

        public TaskbarButtonsBehaviour()
        {
            this.Callback = new HwndSourceHook(this.OnCallback);
            this.Windows = new ConcurrentDictionary<IntPtr, TaskbarButtonsWindowFlags>();
            this.ImageLists = new ConcurrentDictionary<IntPtr, IntPtr>();
            this.Thumbnails = new ConcurrentDictionary<IntPtr, string>();
        }

        public Timer Timer { get; private set; }

        public HwndSourceHook Callback { get; private set; }

        public ConcurrentDictionary<IntPtr, TaskbarButtonsWindowFlags> Windows { get; private set; }

        protected virtual bool HasFlags(IntPtr handle)
        {
            var flags = default(TaskbarButtonsWindowFlags);
            return this.Windows.TryGetValue(handle, out flags);
        }

        protected virtual bool HasFlag(IntPtr handle, TaskbarButtonsWindowFlags flag)
        {
            var flags = default(TaskbarButtonsWindowFlags);
            if (!this.Windows.TryGetValue(handle, out flags))
            {
                return false;
            }
            return flags.HasFlag(flag);
        }

        protected virtual bool AddFlag(IntPtr handle, TaskbarButtonsWindowFlags flag)
        {
            var flags = default(TaskbarButtonsWindowFlags);
            if (!this.Windows.TryGetValue(handle, out flags))
            {
                return false;
            }
            return this.Windows.TryUpdate(handle, flags | flag, flags);
        }

        protected virtual bool RemoveFlag(IntPtr handle, TaskbarButtonsWindowFlags flag)
        {
            var flags = default(TaskbarButtonsWindowFlags);
            if (!this.Windows.TryGetValue(handle, out flags))
            {
                return false;
            }
            return this.Windows.TryUpdate(handle, flags & ~flag, flags);
        }

        public ConcurrentDictionary<IntPtr, IntPtr> ImageLists { get; private set; }

        public ConcurrentDictionary<IntPtr, string> Thumbnails { get; private set; }

        protected virtual bool HasThumbnail(IntPtr handle, string name)
        {
            var currentName = default(string);
            if (!this.Thumbnails.TryGetValue(handle, out currentName))
            {
                return false;
            }
            return string.Equals(currentName, name, StringComparison.OrdinalIgnoreCase);
        }

        protected virtual bool GetThumbnail(IntPtr handle, out string name)
        {
            return this.Thumbnails.TryGetValue(handle, out name);
        }

        protected virtual void SetThumbnail(IntPtr handle, string name)
        {
            this.Thumbnails.AddOrUpdate(handle, name);
        }

        protected virtual bool RemoveThumbnail(IntPtr handle)
        {
            return this.Thumbnails.TryRemove(handle);
        }

        public IPlaylistManager PlaylistManager { get; private set; }

        public IPlaybackManager PlaybackManager { get; private set; }

        public IUserInterface UserInterface { get; private set; }

        public IArtworkProvider ArtworkProvider { get; private set; }

        public ILibraryBrowser LibraryBrowser { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public BooleanConfigurationElement Enabled { get; private set; }

        public BooleanConfigurationElement Progress { get; private set; }

        public BooleanConfigurationElement Thumbnail { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            if (TaskbarButtonsBehaviourConfiguration.IsPlatformSupported)
            {
                this.PlaylistManager = core.Managers.Playlist;
                this.PlaybackManager = core.Managers.Playback;
                this.UserInterface = core.Components.UserInterface;
                this.UserInterface.WindowCreated += this.OnWindowCreated;
                this.UserInterface.WindowDestroyed += this.OnWindowDestroyed;
                this.ArtworkProvider = core.Components.ArtworkProvider;
                this.LibraryBrowser = core.Components.LibraryBrowser;
                this.Configuration = core.Components.Configuration;
                this.Enabled = this.Configuration.GetElement<BooleanConfigurationElement>(
                    TaskbarButtonsBehaviourConfiguration.SECTION,
                    TaskbarButtonsBehaviourConfiguration.ENABLED_ELEMENT
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
                this.Progress = this.Configuration.GetElement<BooleanConfigurationElement>(
                    TaskbarButtonsBehaviourConfiguration.SECTION,
                    TaskbarButtonsBehaviourConfiguration.PROGRESS_ELEMENT
                );
                this.Thumbnail = this.Configuration.GetElement<BooleanConfigurationElement>(
                    TaskbarButtonsBehaviourConfiguration.SECTION,
                    TaskbarButtonsBehaviourConfiguration.THUMBNAIL_ELEMENT
                );
            }
            else
            {
                Logger.Write(this, LogLevel.Warn, "Platform is not supported.");
            }
            base.InitializeComponent(core);
        }

        protected virtual void OnWindowCreated(object sender, UserInterfaceWindowEventArgs e)
        {
            if (e.Window.Role != UserInterfaceWindowRole.Main)
            {
                //Only create taskbar buttons for main windows.
                return;
            }
            this.Windows.TryAdd(e.Window.Handle, TaskbarButtonsWindowFlags.None);
        }

        protected virtual void OnWindowDestroyed(object sender, UserInterfaceWindowEventArgs e)
        {
            this.AddFlag(e.Window.Handle, TaskbarButtonsWindowFlags.Destroyed);
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

        protected virtual async Task Update(IntPtr handle, TaskbarButtonsWindowFlags flags)
        {
            if (flags.HasFlag(TaskbarButtonsWindowFlags.Destroyed))
            {
                if (flags.HasFlag(TaskbarButtonsWindowFlags.Registered))
                {
                    this.RemoveHook(handle);
                }
                if (flags.HasFlag(TaskbarButtonsWindowFlags.ImagesCreated))
                {
                    this.DestroyImages(handle);
                }
                //TODO: We should remove the window from our table but it causes issues and we're likely shutting down now.
                return;
            }
            if (flags.HasFlag(TaskbarButtonsWindowFlags.Error))
            {
                return;
            }
            if (this.Enabled.Value)
            {
                if (!flags.HasFlag(TaskbarButtonsWindowFlags.Registered))
                {
                    this.AddHook(handle);
                }
                if (!flags.HasFlag(TaskbarButtonsWindowFlags.ImagesCreated))
                {
                    if (!await this.CreateImages(handle).ConfigureAwait(false))
                    {
                        return;
                    }
                }
                if (!flags.HasFlag(TaskbarButtonsWindowFlags.ButtonsCreated))
                {
                    if (!await this.CreateButtons(handle).ConfigureAwait(false))
                    {
                        return;
                    }
                }
                else
                {
                    if (!await this.UpdateButtons(handle).ConfigureAwait(false))
                    {
                        return;
                    }
                }
                if (this.Progress.Value)
                {
                    if (!await this.UpdateProgress(handle).ConfigureAwait(false))
                    {
                        return;
                    }
                }
                if (this.Thumbnail.Value)
                {
                    if (!await this.UpdateThumbnail(handle).ConfigureAwait(false))
                    {
                        return;
                    }
                }
            }
            else
            {
                //Although we're no longer enabled, these features can't be disabled so we must keep them updated.
                if (flags.HasFlag(TaskbarButtonsWindowFlags.ButtonsCreated))
                {
                    if (!await this.UpdateButtons(handle).ConfigureAwait(false))
                    {
                        return;
                    }
                }
                if (flags.HasFlag(TaskbarButtonsWindowFlags.ProgressCreated))
                {
                    if (!await this.UpdateProgress(handle).ConfigureAwait(false))
                    {
                        return;
                    }
                }
                if (flags.HasFlag(TaskbarButtonsWindowFlags.ThumbnailCreated))
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
            if (msg == WindowsTaskbarList.WM_COMMAND)
            {
                var low = (wParam.ToInt64() & 0x0000FFFF);
                var high = (wParam.ToInt64() & 0xFFFF0000) >> 16;
                if (high == WindowsTaskbarList.THBN_CLICKED)
                {
                    this.OnButtonPressed(Convert.ToInt32(low));
                }
            }
            else if (msg == WindowsIconicThumbnail.WM_DWMSENDICONICTHUMBNAIL)
            {
                var low = (lParam.ToInt64() & 0x0000FFFF);
                var high = (lParam.ToInt64() & 0xFFFF0000) >> 16;
                var task = this.OnSendIconicThumbnail(hwnd, Convert.ToInt32(high), Convert.ToInt32(low));
            }
            else if (msg == WindowsIconicThumbnail.WM_DWMSENDICONICLIVEPREVIEWBITMAP)
            {
                this.OnSendIconicLivePreviewBitmap(hwnd);
            }
            else if (msg == WM_TASKBARCREATED)
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
            this.Windows[handle] = TaskbarButtonsWindowFlags.Registered;
        }

        protected virtual void AddHook(IntPtr handle)
        {
            Logger.Write(this, LogLevel.Debug, "Adding Windows event handler.");
            var source = HwndSource.FromHwnd(handle);
            if (source == null)
            {
                Logger.Write(this, LogLevel.Warn, "No such window for handle: {0}", handle);
                this.AddFlag(handle, TaskbarButtonsWindowFlags.Error);
                return;
            }
            source.AddHook(this.Callback);
            this.AddFlag(handle, TaskbarButtonsWindowFlags.Registered);
        }

        protected virtual void RemoveHook(IntPtr handle)
        {
            Logger.Write(this, LogLevel.Debug, "Removing Windows event handler.");
            var source = HwndSource.FromHwnd(handle);
            if (source == null)
            {
                Logger.Write(this, LogLevel.Warn, "No such window for handle: {0}", handle);
                this.AddFlag(handle, TaskbarButtonsWindowFlags.Error);
                return;
            }
            source.RemoveHook(this.Callback);
            this.RemoveFlag(handle, TaskbarButtonsWindowFlags.Registered);
        }

        protected virtual async Task<bool> CreateImages(IntPtr handle)
        {
            Logger.Write(this, LogLevel.Debug, "Creating taskbar button image list.");
            var source = HwndSource.FromHwnd(handle);
            if (source == null)
            {
                Logger.Write(this, LogLevel.Warn, "No such window for handle: {0}", handle);
                this.AddFlag(handle, TaskbarButtonsWindowFlags.Error);
                return false;
            }
            var width = WindowsImageList.GetSystemMetrics(
                WindowsImageList.SystemMetric.SM_CXSMICON
            );
            var height = WindowsImageList.GetSystemMetrics(
                WindowsImageList.SystemMetric.SM_CYSMICON
            );
            Logger.Write(this, LogLevel.Debug, "Taskbar buttom image dimentions: {0}x{1}", width, height);
            var imageList = WindowsImageList.ImageList_Create(
                width,
                height,
                WindowsImaging.ILC_COLOR32,
                4,
                0
            );
            if (IntPtr.Zero.Equals(imageList))
            {
                Logger.Write(this, LogLevel.Warn, "Failed to create button image list.");
                this.AddFlag(handle, TaskbarButtonsWindowFlags.Error);
                return false;
            }
            else
            {
                //TODO: Potentially overwriting the previous value.
                this.ImageLists[handle] = imageList;
            }
            for (var a = 0; a < 4; a++)
            {
                using (var bitmap = this.GetButtonImage(a, width, height))
                {
                    if (!this.AddImage(handle, imageList, bitmap, width, height))
                    {
                        return false;
                    }
                }
            }
            var result = default(WindowsTaskbarList.HResult);
            await this.Invoke(
                source.Dispatcher,
                () => result = WindowsTaskbarList.Instance.ThumbBarSetImageList(handle, imageList)
            ).ConfigureAwait(false);
            if (result != WindowsTaskbarList.HResult.Ok)
            {
                Logger.Write(this, LogLevel.Warn, "Failed to create button image list: {0}", Enum.GetName(typeof(WindowsTaskbarList.HResult), result));
                this.AddFlag(handle, TaskbarButtonsWindowFlags.Error);
                return false;
            }
            else
            {
                Logger.Write(this, LogLevel.Debug, "Taskbar button image list created.");
                this.AddFlag(handle, TaskbarButtonsWindowFlags.ImagesCreated);
                return true;
            }
        }

        protected virtual bool AddImage(IntPtr handle, IntPtr imageList, Bitmap bitmap, int width, int height)
        {
            var bitmapSection = default(IntPtr);
            if (!WindowsImaging.CreateDIBSection(bitmap, width, height, out bitmapSection))
            {
                Logger.Write(this, LogLevel.Warn, "Failed to create native bitmap.");
                this.AddFlag(handle, TaskbarButtonsWindowFlags.Error);
                return false;
            }
            var result = WindowsImageList.ImageList_Add(
                imageList,
                bitmapSection,
                IntPtr.Zero
            );
            WindowsImaging.DeleteObject(bitmapSection);
            if (result < 0)
            {
                Logger.Write(this, LogLevel.Warn, "Failed to add image to ImageList.");
                this.AddFlag(handle, TaskbarButtonsWindowFlags.Error);
                return false;
            }
            return true;
        }

        protected virtual bool DestroyImages(IntPtr handle)
        {
            var imageList = default(IntPtr);
            if (!this.ImageLists.TryGetValue(handle, out imageList))
            {
                //There was no image list to destroy.
                return true;
            }
            var result = WindowsImageList.ImageList_Destroy(imageList);
            if (!result)
            {
                Logger.Write(this, LogLevel.Warn, "Failed to destroy image list: {0}", Enum.GetName(typeof(WindowsTaskbarList.HResult), result));
                this.AddFlag(handle, TaskbarButtonsWindowFlags.Error);
                return false;
            }
            else
            {
                Logger.Write(this, LogLevel.Debug, "Taskbar button image list destroyed.");
                this.RemoveFlag(handle, TaskbarButtonsWindowFlags.ImagesCreated);
                return true;
            }
        }

        protected virtual Bitmap GetButtonImage(int index, int width, int height)
        {
            var name = string.Format("FoxTunes.Core.Windows.Images.{0}.png", this.GetImageName(index));
            Logger.Write(this, LogLevel.Debug, "Creating image: {0}", name);
            using (var stream = typeof(TaskbarButtonsBehaviour).Assembly.GetManifestResourceStream(name))
            {
                return this.GetImage(stream, width, height, false);
            }
        }

        protected virtual Bitmap GetPlaceholderImage(int width, int height)
        {
            var name = "FoxTunes.Core.Windows.Images.fox.ico";
            Logger.Write(this, LogLevel.Debug, "Creating image: {0}", name);
            using (var stream = typeof(TaskbarButtonsBehaviour).Assembly.GetManifestResourceStream(name))
            {
                return this.GetImage(stream, width, height, true);
            }
        }

        protected virtual Bitmap GetExternalImage(string fileName, int width, int height)
        {
            Logger.Write(this, LogLevel.Debug, "Creating image: {0}", fileName);
            using (var stream = File.OpenRead(fileName))
            {
                return this.GetImage(stream, width, height, true);
            }
        }

        protected virtual Bitmap GetImage(Stream stream, int width, int height, bool scale)
        {
            var image = (Bitmap)Bitmap.FromStream(stream);
            if (image.Width != width || image.Height != height)
            {
                try
                {
                    return WindowsImaging.Resize(image, width, height, scale);
                }
                finally
                {
                    image.Dispose();
                }
            }
            return image;
        }

        protected virtual string GetImageName(int index)
        {
            switch (index)
            {
                case 0:
                    return "prev";
                case 1:
                    return "play";
                case 2:
                    return "pause";
                case 3:
                    return "next";
            }
            throw new NotImplementedException();
        }

        protected virtual async Task<bool> CreateButtons(IntPtr handle)
        {
            Logger.Write(this, LogLevel.Debug, "Creating taskbar buttons.");
            var source = HwndSource.FromHwnd(handle);
            if (source == null)
            {
                Logger.Write(this, LogLevel.Warn, "No such window for handle: {0}", handle);
                return false;
            }
            var buttons = this.Buttons.ToArray();
            var result = default(WindowsTaskbarList.HResult);
            await this.Invoke(
                source.Dispatcher,
                () => result = WindowsTaskbarList.Instance.ThumbBarAddButtons(
                    handle,
                    Convert.ToUInt32(buttons.Length),
                    buttons
                )
            ).ConfigureAwait(false);
            if (result != WindowsTaskbarList.HResult.Ok)
            {
                Logger.Write(this, LogLevel.Warn, "Failed to create taskbar buttons: {0}", Enum.GetName(typeof(WindowsTaskbarList.HResult), result));
                this.AddFlag(handle, TaskbarButtonsWindowFlags.Error);
                return false;
            }
            else
            {
                Logger.Write(this, LogLevel.Debug, "Taskbar buttons created.");
                this.AddFlag(handle, TaskbarButtonsWindowFlags.ButtonsCreated);
                return true;
            }
        }

        protected virtual async Task UpdateButtons()
        {
            foreach (var pair in this.Windows)
            {
                await this.UpdateButtons(pair.Key).ConfigureAwait(false);
            }
        }

        protected virtual async Task<bool> UpdateButtons(IntPtr handle)
        {
            var source = HwndSource.FromHwnd(handle);
            if (source == null)
            {
                Logger.Write(this, LogLevel.Warn, "No such window for handle: {0}", handle);
                return false;
            }
            var buttons = this.Buttons.ToArray();
            var result = default(WindowsTaskbarList.HResult);
            await this.Invoke(
                source.Dispatcher,
                () => result = WindowsTaskbarList.Instance.ThumbBarUpdateButtons(
                    handle,
                    Convert.ToUInt32(buttons.Length),
                    buttons
                )
            ).ConfigureAwait(false);
            if (result != WindowsTaskbarList.HResult.Ok)
            {
                Logger.Write(this, LogLevel.Warn, "Failed to update taskbar buttons: {0}", Enum.GetName(typeof(WindowsTaskbarList.HResult), result));
                this.AddFlag(handle, TaskbarButtonsWindowFlags.Error);
                return false;
            }
            else
            {
                return true;
            }
        }

        protected virtual async Task<bool> UpdateProgress(IntPtr handle)
        {
            if (await this.UpdateTaskProgress(handle).ConfigureAwait(false))
            {
                return true;
            }
            if (await this.UpdatePlaybackProgress(handle).ConfigureAwait(false))
            {
                return true;
            }
            if (this.HasFlag(handle, TaskbarButtonsWindowFlags.ProgressCreated))
            {
                return await this.ClearProgress(handle).ConfigureAwait(false);
            }
            return true;
        }

        protected virtual async Task<bool> UpdateTaskProgress(IntPtr handle)
        {
            var backgroundTask = BackgroundTask.Active.FirstOrDefault(_backgroundTask => _backgroundTask.Visible);
            if (backgroundTask == null)
            {
                return false;
            }
            var source = HwndSource.FromHwnd(handle);
            if (source == null)
            {
                Logger.Write(this, LogLevel.Warn, "No such window for handle: {0}", handle);
                return false;
            }
            var result = default(WindowsTaskbarList.HResult);
            await this.Invoke(
                source.Dispatcher,
                () =>
                {
                    if (backgroundTask.Count != 0)
                    {
                        result = WindowsTaskbarList.Instance.SetProgressValue(
                            handle,
                            Convert.ToUInt64(backgroundTask.Position),
                            Convert.ToUInt64(backgroundTask.Count)
                        );
                    }
                    else
                    {
                        result = WindowsTaskbarList.Instance.SetProgressState(
                            handle,
                            WindowsTaskbarList.TaskbarProgressBarStatus.Indeterminate
                        );
                    }
                }
            ).ConfigureAwait(false);
            if (result != WindowsTaskbarList.HResult.Ok)
            {
                Logger.Write(this, LogLevel.Warn, "Failed to update taskbar progress: {0}", Enum.GetName(typeof(WindowsTaskbarList.HResult), result));
                this.AddFlag(handle, TaskbarButtonsWindowFlags.Error);
                return false;
            }
            else
            {
                this.AddFlag(handle, TaskbarButtonsWindowFlags.ProgressCreated);
                return true;
            }
        }

        protected virtual async Task<bool> UpdatePlaybackProgress(IntPtr handle)
        {
            var outputStream = this.PlaybackManager.CurrentStream;
            if (outputStream == null)
            {
                return false;
            }
            var source = HwndSource.FromHwnd(handle);
            if (source == null)
            {
                Logger.Write(this, LogLevel.Warn, "No such window for handle: {0}", handle);
                return false;
            }
            var result = default(WindowsTaskbarList.HResult);
            await this.Invoke(
                source.Dispatcher,
                () =>
                {
                    result = WindowsTaskbarList.Instance.SetProgressValue(
                        handle,
                        Convert.ToUInt64(outputStream.Position),
                        Convert.ToUInt64(outputStream.Length)
                    );
                    if (outputStream.IsPaused && result == WindowsTaskbarList.HResult.Ok)
                    {
                        result = WindowsTaskbarList.Instance.SetProgressState(
                            handle,
                            WindowsTaskbarList.TaskbarProgressBarStatus.Paused
                        );
                    }
                    else
                    {
                        result = WindowsTaskbarList.Instance.SetProgressState(
                            handle,
                            WindowsTaskbarList.TaskbarProgressBarStatus.Normal
                        );
                    }
                }
            ).ConfigureAwait(false);
            if (result != WindowsTaskbarList.HResult.Ok)
            {
                Logger.Write(this, LogLevel.Warn, "Failed to update taskbar progress: {0}", Enum.GetName(typeof(WindowsTaskbarList.HResult), result));
                this.AddFlag(handle, TaskbarButtonsWindowFlags.Error);
                return false;
            }
            else
            {
                this.AddFlag(handle, TaskbarButtonsWindowFlags.ProgressCreated);
                return true;
            }
        }

        protected virtual async Task<bool> ClearProgress(IntPtr handle)
        {
            var source = HwndSource.FromHwnd(handle);
            if (source == null)
            {
                Logger.Write(this, LogLevel.Warn, "No such window for handle: {0}", handle);
                return false;
            }
            var result = default(WindowsTaskbarList.HResult);
            await this.Invoke(
                source.Dispatcher,
                () => result = WindowsTaskbarList.Instance.SetProgressState(
                    handle,
                    WindowsTaskbarList.TaskbarProgressBarStatus.NoProgress
                )
            ).ConfigureAwait(false);
            if (result != WindowsTaskbarList.HResult.Ok)
            {
                Logger.Write(this, LogLevel.Warn, "Failed to update taskbar progress: {0}", Enum.GetName(typeof(WindowsTaskbarList.HResult), result));
                this.AddFlag(handle, TaskbarButtonsWindowFlags.Error);
                return false;
            }
            else
            {
                this.RemoveFlag(handle, TaskbarButtonsWindowFlags.ProgressCreated);
                return true;
            }
        }

        protected virtual async Task<bool> UpdateThumbnail(IntPtr handle)
        {
            var source = HwndSource.FromHwnd(handle);
            if (source == null)
            {
                Logger.Write(this, LogLevel.Warn, "No such window for handle: {0}", handle);
                return false;
            }
            if (!this.HasFlag(handle, TaskbarButtonsWindowFlags.ThumbnailCreated))
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
                    this.AddFlag(handle, TaskbarButtonsWindowFlags.Error);
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
                    this.AddFlag(handle, TaskbarButtonsWindowFlags.Error);
                    return false;
                }
                this.AddFlag(handle, TaskbarButtonsWindowFlags.ThumbnailCreated);
            }
            else
            {
                var name = await this.GetIconicThumbnail().ConfigureAwait(false);
                if (!this.HasThumbnail(handle, name))
                {
                    var result = default(WindowsIconicThumbnail.HResult);
                    await this.Invoke(
                        source.Dispatcher,
                        () => result = WindowsIconicThumbnail.DwmInvalidateIconicBitmaps(handle)
                    ).ConfigureAwait(false);
                    if (result != WindowsIconicThumbnail.HResult.Ok)
                    {
                        Logger.Write(this, LogLevel.Warn, "Failed to invalidate iconic thumbnail: {0}", Enum.GetName(typeof(WindowsIconicThumbnail.HResult), result));
                        this.AddFlag(handle, TaskbarButtonsWindowFlags.Error);
                        return false;
                    }
                    Logger.Write(this, LogLevel.Debug, "Iconic thumbnail: {0}", name);
                    this.SetThumbnail(handle, name);
                }
            }
            return true;
        }

        protected virtual Task<bool> OnSendIconicThumbnail(IntPtr handle, int width, int height)
        {
            var name = default(string);
            if (this.GetThumbnail(handle, out name))
            {
                if (!IsThumbnailPlaceholder(name))
                {
                    return this.OnSendIconicThumbnail(handle, name, width, height);
                }
            }
            return this.OnSendPlaceholderIconicThumbnail(handle, width, height);
        }

        protected virtual async Task<bool> OnSendIconicThumbnail(IntPtr handle, Bitmap bitmap, int width, int height)
        {
            var source = HwndSource.FromHwnd(handle);
            if (source == null)
            {
                Logger.Write(this, LogLevel.Warn, "No such window for handle: {0}", handle);
                return false;
            }
            var bitmapSection = default(IntPtr);
            if (!WindowsImaging.CreateDIBSection(bitmap, width, -height /* This isn't a mistake, DIB is top down. */, out bitmapSection))
            {
                Logger.Write(this, LogLevel.Warn, "Failed to create native bitmap.");
                this.AddFlag(handle, TaskbarButtonsWindowFlags.Error);
                return false;
            }
            var result = default(WindowsIconicThumbnail.HResult);
            await this.Invoke(
                source.Dispatcher,
                () => result = WindowsIconicThumbnail.DwmSetIconicThumbnail(handle, bitmapSection, 0)
            ).ConfigureAwait(false);
            if (result != WindowsIconicThumbnail.HResult.Ok)
            {
                Logger.Write(this, LogLevel.Warn, "Failed to set iconic thumbnail: {0}", Enum.GetName(typeof(WindowsIconicThumbnail.HResult), result));
                this.AddFlag(handle, TaskbarButtonsWindowFlags.Error);
                return false;
            }
            WindowsImaging.DeleteObject(bitmapSection);
            return true;
        }

        protected virtual async Task<bool> OnSendIconicThumbnail(IntPtr handle, string fileName, int width, int height)
        {
            using (var bitmap = this.GetExternalImage(fileName, width, height))
            {
                return await this.OnSendIconicThumbnail(handle, bitmap, width, height).ConfigureAwait(false);
            }
        }

        protected virtual async Task<bool> OnSendPlaceholderIconicThumbnail(IntPtr handle, int width, int height)
        {
            using (var bitmap = this.GetPlaceholderImage(width, height))
            {
                return await this.OnSendIconicThumbnail(handle, bitmap, width, height).ConfigureAwait(false);
            }
        }

        protected virtual async Task<string> GetIconicThumbnail()
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
                    return fileName;
                }
            }
            return THUMBNAIL_PLACEHOLDER;
        }

        protected virtual void OnSendIconicLivePreviewBitmap(IntPtr handle)
        {
            //Nothing to do.
        }

        protected virtual IEnumerable<WindowsTaskbarList.ThumbButton> Buttons
        {
            get
            {
                yield return this.PreviousButton;
                yield return this.PlayPauseButton;
                yield return this.NextButton;
            }
        }

        protected virtual WindowsTaskbarList.ThumbButton PreviousButton
        {
            get
            {
                return new WindowsTaskbarList.ThumbButton()
                {
                    Id = BUTTON_PREVIOUS,
                    Tip = Strings.TaskbarButtonsBehaviour_Previous,
                    Bitmap = 0,
                    Mask = this.ButtonMask,
                    Flags = this.ButtonFlags
                };
            }
        }

        protected virtual WindowsTaskbarList.ThumbButton PlayPauseButton
        {
            get
            {
                var outputStream = this.PlaybackManager.CurrentStream;
                return new WindowsTaskbarList.ThumbButton()
                {
                    Id = BUTTON_PLAY_PAUSE,
                    Tip = Strings.TaskbarButtonsBehaviour_PlayPause,
                    Bitmap = outputStream != null && outputStream.IsPlaying ? 2u : 1u,
                    Mask = this.ButtonMask,
                    Flags = this.ButtonFlags
                };
            }
        }

        protected virtual WindowsTaskbarList.ThumbButton NextButton
        {
            get
            {
                return new WindowsTaskbarList.ThumbButton()
                {
                    Id = BUTTON_NEXT,
                    Tip = Strings.TaskbarButtonsBehaviour_Next,
                    Bitmap = 3,
                    Mask = this.ButtonMask,
                    Flags = this.ButtonFlags
                };
            }
        }

        protected virtual WindowsTaskbarList.ThumbButtonMask ButtonMask
        {
            get
            {
                return WindowsTaskbarList.ThumbButtonMask.THB_FLAGS | WindowsTaskbarList.ThumbButtonMask.Bitmap;
            }
        }

        protected virtual WindowsTaskbarList.ThumbButtonOptions ButtonFlags
        {
            get
            {
                var flags = default(WindowsTaskbarList.ThumbButtonOptions);
                if (!this.Enabled.Value)
                {
                    flags |= WindowsTaskbarList.ThumbButtonOptions.Hidden;
                }
                return flags;
            }
        }

        protected virtual void OnButtonPressed(int button)
        {
            var task = default(Task);
            switch (button)
            {
                case BUTTON_PREVIOUS:
                    task = this.Previous();
                    break;
                case BUTTON_PLAY_PAUSE:
                    task = this.PlayPause();
                    break;
                case BUTTON_NEXT:
                    task = this.Next();
                    break;
            }
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

        protected virtual async Task Previous()
        {
            Logger.Write(this, LogLevel.Debug, "Previous button was clicked.");
            await this.PlaylistManager.Previous().ConfigureAwait(false);
            var task = this.UpdateButtons();
        }

        protected virtual async Task PlayPause()
        {
            Logger.Write(this, LogLevel.Debug, "Play/pause button was clicked.");
            var currentStream = this.PlaybackManager.CurrentStream;
            if (currentStream == null)
            {
                await this.PlaylistManager.Next().ConfigureAwait(false);
            }
            else
            {
                if (currentStream.IsPaused)
                {
                    await currentStream.Resume().ConfigureAwait(false);
                }
                else if (currentStream.IsPlaying)
                {
                    await currentStream.Pause().ConfigureAwait(false);
                }
            }
            var task = this.UpdateButtons();
        }

        protected virtual async Task Next()
        {
            Logger.Write(this, LogLevel.Debug, "Next button was clicked.");
            await this.PlaylistManager.Next().ConfigureAwait(false);
            var task = this.UpdateButtons();
        }

        public IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return TaskbarButtonsBehaviourConfiguration.GetConfigurationSections();
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
            if (this.UserInterface != null)
            {
                this.UserInterface.WindowCreated -= this.OnWindowCreated;
                this.UserInterface.WindowDestroyed -= this.OnWindowDestroyed;
            }
            this.Disable();
        }

        ~TaskbarButtonsBehaviour()
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

        [DllImport("user32.dll")]
        public static extern int RegisterWindowMessage(string msg);
    }

    [Flags]
    public enum TaskbarButtonsWindowFlags : byte
    {
        None = 0,
        Registered = 1,
        ImagesCreated = 2,
        ButtonsCreated = 4,
        Error = 8,
        Destroyed = 16,
        ProgressCreated = 32,
        ThumbnailCreated = 64
    }
}
