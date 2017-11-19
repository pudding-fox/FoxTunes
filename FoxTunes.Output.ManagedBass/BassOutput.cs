using FoxTunes.Interfaces;
using ManagedBass;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using ManagedBass.Asio;

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

        private BassOutputMode _Mode { get; set; }

        public BassOutputMode Mode
        {
            get
            {
                return this._Mode;
            }
            private set
            {
                this._Mode = value;
                Logger.Write(this, LogLevel.Debug, "Mode = {0}", Enum.GetName(typeof(BassOutputMode), this.Mode));
                this.Shutdown();
            }
        }

        private int _DirectSoundDevice { get; set; }

        public int DirectSoundDevice
        {
            get
            {
                return this._DirectSoundDevice;
            }
            private set
            {
                this._DirectSoundDevice = value;
                Logger.Write(this, LogLevel.Debug, "Direct Sound Device = {0}", this.DirectSoundDevice);
                this.Shutdown();
            }
        }

        private int _AsioDevice { get; set; }

        public int AsioDevice
        {
            get
            {
                return this._AsioDevice;
            }
            private set
            {
                this._AsioDevice = value;
                Logger.Write(this, LogLevel.Debug, "ASIO Device = {0}", this.AsioDevice);
                this.Shutdown();
            }
        }

        private bool _DSDDirect { get; set; }

        public bool DSDDirect
        {
            get
            {
                return this._DSDDirect;
            }
            private set
            {
                this._DSDDirect = value;
                Logger.Write(this, LogLevel.Debug, "DSD = {0}", this.DSDDirect);
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
                switch (this.Mode)
                {
                    case BassOutputMode.DirectSound:
                        this.StartDirectSound();
                        break;
                    case BassOutputMode.ASIO:
                        this.StartASIO();
                        break;
                }
                this.MasterChannel = BassMasterChannelFactory.Instance.Create(this);
                this.MasterChannel.InitializeComponent(this.Core);
                this.MasterChannel.Error += this.MasterChannel_Error;
                this.IsStarted = true;
                Logger.Write(this, LogLevel.Debug, "Started BASS.");
            }
            catch (Exception e)
            {
                this.Shutdown(true);
                this.OnError(e);
            }
        }

        private void StartDirectSound()
        {
            BassUtils.OK(Bass.Init(this.DirectSoundDevice, this.Rate));
            Logger.Write(this, LogLevel.Debug, "BASS Initialized.");
        }

        private void StartASIO()
        {
            BassUtils.OK(Bass.Init(Bass.NoSoundDevice, this.Rate));
            BassUtils.OK(BassAsio.Init(this.AsioDevice, AsioInitFlags.Thread));
            BassAsio.Rate = this.Rate;
            Logger.Write(this, LogLevel.Debug, "BASS ASIO Initialized.");
        }

        public void Shutdown(bool force = false)
        {
            if (!force && !this.IsStarted)
            {
                return;
            }
            Logger.Write(this, LogLevel.Debug, "Stopping BASS.");
            try
            {
                if (this.MasterChannel != null)
                {
                    this.MasterChannel.Dispose();
                    this.MasterChannel = null;
                }
                switch (this.Mode)
                {
                    case BassOutputMode.DirectSound:
                        this.StopDirectSound();
                        break;
                    case BassOutputMode.ASIO:
                        this.StopASIO();
                        break;
                }
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

        private void StopDirectSound()
        {
            //Not checking result code as shutdown may be forced regardless of state.
            Bass.Free();
        }

        private void StopASIO()
        {
            //Not checking result code as shutdown may be forced regardless of state.
            Bass.Free();
            BassAsio.Free();
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
                BassOutputConfiguration.MODE_ELEMENT
            ).ConnectValue<string>(value => this.Mode = BassOutputConfiguration.GetMode(value));
            this.Core.Components.Configuration.GetElement<SelectionConfigurationElement>(
                BassOutputConfiguration.OUTPUT_SECTION,
                BassOutputConfiguration.ELEMENT_DS_DEVICE
            ).ConnectValue<string>(value => this.DirectSoundDevice = BassOutputConfiguration.GetDsDevice(value));
            this.Core.Components.Configuration.GetElement<SelectionConfigurationElement>(
                BassOutputConfiguration.OUTPUT_SECTION,
                BassOutputConfiguration.ELEMENT_ASIO_DEVICE
            ).ConnectValue<string>(value => this.AsioDevice = BassOutputConfiguration.GetAsioDevice(value));
            this.Core.Components.Configuration.GetElement<BooleanConfigurationElement>(
                BassOutputConfiguration.OUTPUT_SECTION,
                BassOutputConfiguration.DSD_RAW_ELEMENT
            ).ConnectValue<bool>(value => this.DSDDirect = value);
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
            if (this.IsStarted)
            {
                Logger.Write(this, LogLevel.Debug, "Pre-empting playback of stream from file {0}: {1}", outputStream.FileName, outputStream.ChannelHandle);
                this.MasterChannel.SetSecondaryChannel(outputStream.ChannelHandle);
            }
            else
            {
                Logger.Write(this, LogLevel.Debug, "Not yet started, cannot pre-emp playback of stream from file {0}: {1}", outputStream.FileName, outputStream.ChannelHandle);
            }
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

    public enum BassOutputMode : byte
    {
        None,
        DirectSound,
        ASIO
    }
}
