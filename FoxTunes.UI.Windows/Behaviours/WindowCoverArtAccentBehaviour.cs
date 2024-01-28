using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace FoxTunes
{
    [WindowsUserInterfaceDependency]
    [PlatformDependency(Major = 6, Minor = 2, Build = 22621)]
    public class WindowCoverArtAccentBehaviour : StandardBehaviour, IDisposable
    {
        public WindowCoverArtAccentBehaviour()
        {
            this.AccentColors = new Dictionary<IntPtr, Color>();
        }

        public IDictionary<IntPtr, Color> AccentColors { get; private set; }

        public IPlaybackManager PlaybackManager { get; private set; }

        public ILibraryBrowser LibraryBrowser { get; private set; }

        public IArtworkProvider ArtworkProvider { get; private set; }

        public ImageResizer ImageResizer { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public BooleanConfigurationElement ArtworkAccent { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            WindowBase.ActiveChanged += this.OnActiveChanged;
            this.PlaybackManager = core.Managers.Playback;
            this.PlaybackManager.CurrentStreamChanged += this.OnCurrentStreamChanged;
            this.LibraryBrowser = core.Components.LibraryBrowser;
            this.ArtworkProvider = core.Components.ArtworkProvider;
            this.ImageResizer = ComponentRegistry.Instance.GetComponent<ImageResizer>();
            this.Configuration = core.Components.Configuration;
            this.ArtworkAccent = this.Configuration.GetElement<BooleanConfigurationElement>(
                WindowsUserInterfaceConfiguration.SECTION,
                WindowsUserInterfaceConfiguration.ARTWORK_ACCENT
            );
            this.ArtworkAccent.ValueChanged += this.OnValueChanged;
            base.InitializeComponent(core);
        }

        protected virtual void OnActiveChanged(object sender, EventArgs e)
        {
            this.Dispatch(this.Refresh);
        }

        protected virtual void OnValueChanged(object sender, EventArgs e)
        {
            this.Dispatch(this.Refresh);
        }

        private void OnCurrentStreamChanged(object sender, EventArgs e)
        {
            //Critical: Don't block in this event handler, it causes a deadlock.
            this.Dispatch(this.Refresh);
        }

        protected virtual async Task Refresh()
        {
            if (!this.ArtworkAccent.Value)
            {
                return;
            }
            var outputStream = this.PlaybackManager.CurrentStream;
            if (outputStream == null)
            {
                return;
            }
            var fileData = default(IFileData);
            if (outputStream.PlaylistItem.LibraryItem_Id.HasValue)
            {
                fileData = this.LibraryBrowser.Get(outputStream.PlaylistItem.LibraryItem_Id.Value);
            }
            else
            {
                fileData = outputStream.PlaylistItem;
            }
            var fileName = await this.ArtworkProvider.Find(
                fileData,
                ArtworkType.FrontCover
            ).ConfigureAwait(false);
            if (string.IsNullOrEmpty(fileName))
            {
                return;
            }
            var color = this.GetAccentColor(fileName);
            await this.Refresh(color).ConfigureAwait(false);
        }

        protected virtual async Task Refresh(Color color)
        {
            var windows = new HashSet<IntPtr>();
            foreach (var window in WindowBase.Active)
            {
                windows.Add(window.Handle);
                var currentColor = default(Color);
                if (AccentColors.TryGetValue(window.Handle, out currentColor) && currentColor == color)
                {
                    continue;
                }
                await this.Refresh(window, currentColor, color).ConfigureAwait(false);
                this.AccentColors[window.Handle] = color;
            }
            foreach (var handle in AccentColors.Keys.ToArray())
            {
                if (!windows.Contains(handle))
                {
                    AccentColors.Remove(handle);
                }
            }
        }

        protected virtual Task Refresh(WindowBase window, Color currentColor, Color newColor)
        {
            return Windows.Invoke(() =>
            {
                ColorAnimation animation = new ColorAnimation(
                    currentColor,
                    newColor,
                    new Duration(TimeSpan.FromSeconds(1))
                )
                {
                    EasingFunction = new QuadraticEase()
                };
                window.BeginAnimation(WindowExtensions.AccentColorProperty, animation);
            });
        }

        protected virtual Color GetAccentColor(string fileName)
        {
            var color = this.ImageResizer.GetMainColor(fileName);
            return Color.FromArgb(color.A, color.R, color.G, color.B);
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

        }

        ~WindowCoverArtAccentBehaviour()
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
    }
}
