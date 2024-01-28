using FoxTunes.Interfaces;
using ManagedBass;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes
{
    [Component("E0318CB1-57A0-4DC3-AA8D-F6E100F86190", ComponentSlots.Output)]
    public class BassOutput : Output, IDisposable
    {
        public ICore Core { get; private set; }

        public BassMasterChannel MasterChannel { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            BassUtils.OK(Bass.Init());
            BassPluginLoader.Instance.Load();
            this.MasterChannel = new BassMasterChannel();
            this.MasterChannel.InitializeComponent(core);
            this.Core = core;
            base.InitializeComponent(core);
        }

        public override bool IsSupported(string fileName)
        {
            var extension = Path.GetExtension(fileName);
            if (string.IsNullOrEmpty(extension))
            {
                return false;
            }
            extension = extension.Substring(1);
            return BassUtils
                .GetInputFormats()
                .Any(format => string.Equals(format, extension, StringComparison.OrdinalIgnoreCase));
        }

        public override Task<IOutputStream> Load(PlaylistItem playlistItem)
        {
            var channelHandle = Bass.CreateStream(playlistItem.FileName, 0, 0, BassFlags.Decode);
            var outputStream = new BassOutputStream(this, playlistItem, channelHandle);
            outputStream.InitializeComponent(this.Core);
            return Task.FromResult<IOutputStream>(outputStream);
        }

        public override Task Preempt(IOutputStream stream)
        {
            var outputStream = stream as BassOutputStream;
            this.MasterChannel.SetStandbyChannelHandle(outputStream.ChannelHandle);
            return Task.CompletedTask;
        }

        public override Task Unload(IOutputStream stream)
        {
            //if (!stream.IsStopped)
            //{
            //    stream.Stop();
            //}
            stream.Dispose();
            return Task.CompletedTask;
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
            BassUtils.OK(Bass.Free());
            BassUtils.OK(Bass.PluginFree(0));
        }

        ~BassOutput()
        {
            Logger.Write(this, LogLevel.Error, "Component was not disposed: {0}", this.GetType().Name);
            this.Dispose(true);
        }
    }
}
