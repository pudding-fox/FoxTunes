using FoxTunes.Interfaces;
using ManagedBass;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace FoxTunes
{
    [Component("E0318CB1-57A0-4DC3-AA8D-F6E100F86190", ComponentSlots.Output)]
    public class BassOutput : Output, IConfigurableComponent, IDisposable
    {
        public ICore Core { get; private set; }

        private int _Rate { get; set; }

        public int Rate
        {
            get
            {
                return this._Rate;
            }
            private set
            {
                this._Rate = value;
                Logger.Write(this, LogLevel.Debug, "Rate = {0}", this.Rate);
                this.Shutdown();
            }
        }

        private bool _Float { get; set; }

        public bool Float
        {
            get
            {
                return this._Float;
            }
            private set
            {
                this._Float = value;
                Logger.Write(this, LogLevel.Debug, "Float = {0}", this.Float);
                this.Shutdown();
            }
        }

        public BassFlags Flags
        {
            get
            {
                var flags = BassFlags.Decode;
                if (this.Float)
                {
                    flags |= BassFlags.Float;
                }
                return flags;
            }
        }

        public BassMasterChannel MasterChannel { get; private set; }

        public void Start()
        {
            if (this.IsStarted)
            {
                this.Shutdown();
            }
            Logger.Write(this, LogLevel.Debug, "Starting BASS.");
            try
            {
                BassUtils.OK(Bass.Init(Bass.DefaultDevice, this.Rate));
                this.MasterChannel = new BassMasterChannel(this);
                this.MasterChannel.InitializeComponent(this.Core);
                this.MasterChannel.Error += this.MasterChannel_Error;
                this.IsStarted = true;
                Logger.Write(this, LogLevel.Debug, "Started BASS.");
            }
            catch (Exception e)
            {
                this.OnError(e);
            }
        }

        public void Shutdown()
        {
            if (!this.IsStarted)
            {
                return;
            }
            Logger.Write(this, LogLevel.Debug, "Stopping BASS.");
            try
            {
                this.MasterChannel.Dispose();
                this.MasterChannel = null;
                BassUtils.OK(Bass.Free());
                Logger.Write(this, LogLevel.Debug, "Stopped BASS.");
            }
            catch (Exception e)
            {
                this.OnError(e);
            }
            finally
            {
                this.IsStarted = false;
            }
        }

        protected virtual void MasterChannel_Error(object sender, ComponentOutputErrorEventArgs e)
        {
            this.Shutdown();
            this.OnError(e.Exception);
        }

        public override void InitializeComponent(ICore core)
        {
            BassPluginLoader.Instance.Load();
            this.Core = core;
            this.Core.Components.Configuration.GetElement<SelectionConfigurationElement>(
                BassOutputConfiguration.OUTPUT_SECTION,
                BassOutputConfiguration.RATE_ELEMENT
            ).ConnectValue<string>(value => this.Rate = BassOutputConfiguration.GetRate(value));
            this.Core.Components.Configuration.GetElement<SelectionConfigurationElement>(
                BassOutputConfiguration.OUTPUT_SECTION,
                BassOutputConfiguration.DEPTH_ELEMENT
            ).ConnectValue<string>(value => this.Float = BassOutputConfiguration.GetFloat(value));
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
            if (!this.IsStarted)
            {
                this.Start();
            }
            Logger.Write(this, LogLevel.Debug, "Creating stream from file {0}", playlistItem.FileName);
            var channelHandle = Bass.CreateStream(playlistItem.FileName, 0, 0, this.Flags);
            if (channelHandle == 0)
            {
                BassUtils.Throw();
            }
            Logger.Write(this, LogLevel.Debug, "Created stream from file {0}: {1}", playlistItem.FileName, channelHandle);
            var outputStream = new BassOutputStream(this, playlistItem, channelHandle);
            outputStream.InitializeComponent(this.Core);
            return Task.FromResult<IOutputStream>(outputStream);
        }

        public override Task Preempt(IOutputStream stream)
        {
            var outputStream = stream as BassOutputStream;
            Logger.Write(this, LogLevel.Debug, "Pre-empting playback of stream from file {0}: {1}", outputStream.FileName, outputStream.ChannelHandle);
            this.MasterChannel.SetSecondaryChannelHandle(outputStream.ChannelHandle);
            return Task.CompletedTask;
        }

        public override Task Unload(IOutputStream stream)
        {
            stream.Dispose();
            return Task.CompletedTask;
        }

        public IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return BassOutputConfiguration.GetConfigurationSections();
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
            if (this.IsStarted)
            {
                this.Shutdown();
            }
        }

        ~BassOutput()
        {
            Logger.Write(this, LogLevel.Error, "Component was not disposed: {0}", this.GetType().Name);
            this.Dispose(true);
        }
    }
}
