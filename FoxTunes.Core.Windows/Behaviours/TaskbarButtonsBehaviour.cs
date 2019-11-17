using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Interop;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.UserInterface)]
    public class TaskbarButtonsBehaviour : StandardBehaviour, IConfigurableComponent, IDisposable
    {
        const int UPDATE_INTERVAL = 1000;

        private static readonly int WM_TASKBARBUTTONCREATED;

        static TaskbarButtonsBehaviour()
        {
            try
            {
                WM_TASKBARBUTTONCREATED = RegisterWindowMessage("TaskbarButtonCreated");
            }
            catch
            {
                Logger.Write(typeof(WindowsMessageSink), LogLevel.Warn, "Failed to register window message: TaskbarButtonCreated");
            }
        }

        const int BUTTON_PREVIOUS = 0;

        const int BUTTON_PLAY_PAUSE = 1;

        const int BUTTON_NEXT = 2;

        public TaskbarButtonsBehaviour()
        {
            this.Callback = new HwndSourceHook(this.OnCallback);
            this.Windows = new Dictionary<IntPtr, bool>();
        }

        public Timer Timer { get; private set; }

        public HwndSourceHook Callback { get; private set; }

        public Dictionary<IntPtr, bool> Windows { get; private set; }

        public IPlaylistManager PlaylistManager { get; private set; }

        public IPlaybackManager PlaybackManager { get; private set; }

        public IUserInterface UserInterface { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public BooleanConfigurationElement Enabled { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            if (TaskbarButtonsBehaviourConfiguration.IsPlatformSupported)
            {
                this.PlaylistManager = core.Managers.Playlist;
                this.PlaybackManager = core.Managers.Playback;
                this.UserInterface = core.Components.UserInterface;
                this.UserInterface.WindowCreated += this.OnWindowCreated;
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
                    this.UpdateHooks();
                    this.UpdateImages();
                    this.UpdateButtons();
                });
            }
            else
            {
                Logger.Write(this, LogLevel.Warn, "Platform is not supported.");
            }
            base.InitializeComponent(core);
        }

        public void Enable()
        {
            this.Timer = new Timer();
            this.Timer.Interval = UPDATE_INTERVAL;
            this.Timer.Elapsed += this.OnElapsed;
            this.Timer.Start();
            Logger.Write(this, LogLevel.Debug, "Updater enabled.");
        }

        public void Disable()
        {
            if (this.Timer == null)
            {
                return;
            }
            this.Timer.Stop();
            this.Timer.Elapsed -= this.OnElapsed;
            this.Timer.Dispose();
            this.Timer = null;
            Logger.Write(this, LogLevel.Debug, "Updater disabled.");
        }

        protected virtual void OnElapsed(object sender, ElapsedEventArgs e)
        {
            this.UpdateButtons();
        }

        protected virtual void OnWindowCreated(object sender, UserInterfaceWindowCreatedEvent e)
        {
            this.Windows[e.Handle] = false;
            this.UpdateHook(e.Handle);
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
            else if (msg == WM_TASKBARBUTTONCREATED)
            {
                this.Windows[hwnd] = false;
                this.UpdateImages(hwnd);
                this.UpdateButtons(hwnd);
            }
            return IntPtr.Zero;
        }

        protected virtual void AddHook(IntPtr handle)
        {
            Logger.Write(this, LogLevel.Debug, "Adding Windows event handler.");
            var source = HwndSource.FromHwnd(handle);
            source.AddHook(this.Callback);
        }

        protected virtual void RemoveHook(IntPtr handle)
        {
            Logger.Write(this, LogLevel.Debug, "Removing Windows event handler.");
            var source = HwndSource.FromHwnd(handle);
            source.RemoveHook(this.Callback);
        }

        protected virtual void UpdateHooks()
        {
            foreach (var handle in this.Windows.Keys.ToArray())
            {
                this.UpdateHook(handle);
            }
        }

        protected virtual void UpdateHook(IntPtr handle)
        {
            if (this.Enabled.Value)
            {
                this.AddHook(handle);
            }
            else
            {
                this.RemoveHook(handle);
            }
        }

        protected virtual void UpdateImages()
        {
            foreach (var handle in this.Windows.Keys.ToArray())
            {
                this.UpdateImages(handle);
            }
        }

        protected virtual void UpdateImages(IntPtr handle)
        {
            if (!this.Enabled.Value)
            {
                return;
            }
            if (this.Windows[handle])
            {
                return;
            }
            Logger.Write(this, LogLevel.Debug, "Creating taskbar button image list.");
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
                WindowsImageList.ILC_COLOR32,
                4,
                0
            );
            for (var a = 0; a < 4; a++)
            {
                using (var bitmap = this.GetImage(a, width, height))
                {
                    this.UpdateImage(imageList, bitmap, width, height);
                }
            }
            WindowsTaskbarList.Instance.ThumbBarSetImageList(handle, imageList);
            Logger.Write(this, LogLevel.Debug, "Taskbar button image list created.");
        }

        protected virtual void UpdateImage(IntPtr imageList, Bitmap bitmap, int width, int height)
        {
            var bitmapBits = default(IntPtr);
            var bitmapInfo = new WindowsImageList.BITMAPINFO()
            {
                biSize = 40,
                biBitCount = 32,
                biPlanes = 1,
                biWidth = width,
                biHeight = height
            };
            var bitmapSection = WindowsImageList.CreateDIBSection(
                IntPtr.Zero,
                bitmapInfo,
                0,
                out bitmapBits,
                IntPtr.Zero,
                0
            );
            var bitmapData = bitmap.LockBits(
                new Rectangle(
                    0,
                    0,
                    bitmap.Width,
                    bitmap.Height
                ),
                ImageLockMode.ReadOnly,
                PixelFormat.Format32bppArgb
            );
            WindowsImageList.RtlMoveMemory(
                bitmapBits,
                bitmapData.Scan0,
                bitmap.Height * bitmapData.Stride
            );
            bitmap.UnlockBits(bitmapData);
            WindowsImageList.ImageList_Add(
                imageList,
                bitmapSection,
                IntPtr.Zero
            );
        }

        protected virtual Bitmap GetImage(int index, int width, int height)
        {
            var name = string.Format("FoxTunes.Core.Windows.Images.{0}.png", this.GetImageName(index));
            Logger.Write(this, LogLevel.Debug, "Creating image: {0}", name);
            using (var stream = typeof(TaskbarButtonsBehaviour).Assembly.GetManifestResourceStream(name))
            {
                var image = (Bitmap)Bitmap.FromStream(stream);
                if (image.Width != width || image.Height != height)
                {
                    try
                    {
                        return this.GetImage(image, width, height);
                    }
                    finally
                    {
                        image.Dispose();
                    }
                }
                return image;
            }
        }

        protected virtual Bitmap GetImage(Bitmap sourceImage, int width, int height)
        {
            var resultImage = new Bitmap(width, height);
            using (var graphics = Graphics.FromImage(resultImage))
            {
                graphics.InterpolationMode = InterpolationMode.High;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.DrawImage(
                    sourceImage,
                    new Rectangle(
                        0,
                        0,
                        width,
                        height
                    )
                );
            }
            return resultImage;
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

        protected virtual void UpdateButtons()
        {
            foreach (var handle in this.Windows.Keys.ToArray())
            {
                this.UpdateButtons(handle);
            }
        }

        protected virtual void UpdateButtons(IntPtr handle)
        {
            var buttons = this.Buttons.ToArray();
            var result = default(WindowsTaskbarList.HResult);
            if (this.Windows[handle])
            {
                result = WindowsTaskbarList.Instance.ThumbBarUpdateButtons(
                    handle,
                    Convert.ToUInt32(buttons.Length),
                    buttons
                );
            }
            else if (this.Enabled.Value)
            {
                Logger.Write(this, LogLevel.Debug, "Creating taskbar buttons.");
                result = WindowsTaskbarList.Instance.ThumbBarAddButtons(
                    handle,
                    Convert.ToUInt32(buttons.Length),
                    buttons
                );
                this.Windows[handle] = true;
            }
            if (result != WindowsTaskbarList.HResult.Ok)
            {
                Logger.Write(this, LogLevel.Warn, "Failed to create or update taskbar buttons: {0}", Enum.GetName(typeof(WindowsTaskbarList.HResult), result));
            }
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
                    Tip = "Previous",
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
                    Tip = "Play or pause",
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
                    Tip = "Next",
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

        protected virtual async Task Previous()
        {
            Logger.Write(this, LogLevel.Debug, "Previous button was clicked.");
            await this.PlaylistManager.Previous();
            this.UpdateButtons();
        }

        protected virtual async Task PlayPause()
        {
            Logger.Write(this, LogLevel.Debug, "Play/pause button was clicked.");
            var currentStream = this.PlaybackManager.CurrentStream;
            if (currentStream == null)
            {
                await this.PlaylistManager.Next();
            }
            else
            {
                if (currentStream.IsPaused)
                {
                    await currentStream.Resume();
                }
                else if (currentStream.IsPlaying)
                {
                    await currentStream.Pause();
                }
            }
            this.UpdateButtons();
        }

        protected virtual async Task Next()
        {
            Logger.Write(this, LogLevel.Debug, "Next button was clicked.");
            await this.PlaylistManager.Next();
            this.UpdateButtons();
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
            //TODO: Remove the hooks, images and buttons.
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
}
