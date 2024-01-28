using FoxTunes.Interfaces;
using ManagedBass;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoxTunes
{
    [Component("E0318CB1-57A0-4DC3-AA8D-F6E100F86190", ComponentSlots.Output)]
    public class BassOutput : Output, IBassOutput
    {
        public ICore Core { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public IBassStreamPipeline Pipeline { get; private set; }

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

        private bool _EnforceRate { get; set; }

        public bool EnforceRate
        {
            get
            {
                return this._EnforceRate;
            }
            private set
            {
                this._EnforceRate = value;
                Logger.Write(this, LogLevel.Debug, "Enforce Rate = {0}", this.EnforceRate);
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
            set
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
            set
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
            set
            {
                this._AsioDevice = value;
                Logger.Write(this, LogLevel.Debug, "ASIO Device = {0}", this.AsioDevice);
                this.Shutdown();
            }
        }

        private bool _DsdDirect { get; set; }

        public bool DsdDirect
        {
            get
            {
                return this._DsdDirect;
            }
            private set
            {
                this._DsdDirect = value;
                Logger.Write(this, LogLevel.Debug, "DSD = {0}", this.DsdDirect);
                this.Shutdown();
            }
        }


        private bool _Resampler { get; set; }

        public bool Resampler
        {
            get
            {
                return this._Resampler;
            }
            set
            {
                this._Resampler = value;
                Logger.Write(this, LogLevel.Debug, "Resampler = {0}", this.Resampler);
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
                        BassDefaultStreamOutput.Init(this);
                        break;
                    case BassOutputMode.ASIO:
                        BassAsioStreamOutput.Init(this);
                        break;
                }
                this.IsStarted = true;
                Logger.Write(this, LogLevel.Debug, "Started BASS.");
            }
            catch (Exception e)
            {
                this.Shutdown(true);
                this.OnError(e);
                throw;
            }
        }

        public override Task Shutdown()
        {
            return this.Shutdown(false);
        }

        protected virtual Task Shutdown(bool force)
        {
            if (force || this.IsStarted)
            {
                Logger.Write(this, LogLevel.Debug, "Stopping BASS.");
                try
                {
                    this.FreePipeline();
                    Bass.Free();
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
            return Task.CompletedTask;
        }

        protected virtual void OutputChannel_Error(object sender, ComponentOutputErrorEventArgs e)
        {
            this.Shutdown();
            this.OnError(e.Exception);
        }

        public override void InitializeComponent(ICore core)
        {
            BassPluginLoader.Instance.Load();
            this.Core = core;
            this.Configuration = core.Components.Configuration;
            this.Configuration.GetElement<SelectionConfigurationElement>(
                BassOutputConfiguration.OUTPUT_SECTION,
                BassOutputConfiguration.MODE_ELEMENT
            ).ConnectValue<string>(value => this.Mode = BassOutputConfiguration.GetMode(value));
            this.Configuration.GetElement<SelectionConfigurationElement>(
                BassOutputConfiguration.OUTPUT_SECTION,
                BassOutputConfiguration.ELEMENT_DS_DEVICE
            ).ConnectValue<string>(value => this.DirectSoundDevice = BassOutputConfiguration.GetDsDevice(value));
            this.Configuration.GetElement<SelectionConfigurationElement>(
                BassOutputConfiguration.OUTPUT_SECTION,
                BassOutputConfiguration.ELEMENT_ASIO_DEVICE
            ).ConnectValue<string>(value => this.AsioDevice = BassOutputConfiguration.GetAsioDevice(value));
            this.Configuration.GetElement<BooleanConfigurationElement>(
                BassOutputConfiguration.OUTPUT_SECTION,
                BassOutputConfiguration.DSD_RAW_ELEMENT
            ).ConnectValue<bool>(value => this.DsdDirect = value);
            this.Configuration.GetElement<SelectionConfigurationElement>(
                BassOutputConfiguration.OUTPUT_SECTION,
                BassOutputConfiguration.RATE_ELEMENT
            ).ConnectValue<string>(value => this.Rate = BassOutputConfiguration.GetRate(value));
            this.Configuration.GetElement<BooleanConfigurationElement>(
                BassOutputConfiguration.OUTPUT_SECTION,
                BassOutputConfiguration.ENFORCE_RATE_ELEMENT
            ).ConnectValue<bool>(value => this.EnforceRate = value);
            this.Configuration.GetElement<SelectionConfigurationElement>(
                BassOutputConfiguration.OUTPUT_SECTION,
                BassOutputConfiguration.DEPTH_ELEMENT
            ).ConnectValue<string>(value => this.Float = BassOutputConfiguration.GetFloat(value));
            this.Configuration.GetElement<BooleanConfigurationElement>(
                BassOutputConfiguration.OUTPUT_SECTION,
                BassOutputConfiguration.RESAMPLER_ELEMENT
            ).ConnectValue<bool>(value => this.Resampler = value);
            base.InitializeComponent(core);
        }

        public override bool IsSupported(string fileName)
        {
            return BassUtils
                .GetInputFormats()
                .Contains(fileName.GetExtension(), true);
        }

        public override Task<IOutputStream> Load(PlaylistItem playlistItem)
        {
            if (!this.IsStarted)
            {
                this.Start();
            }
            var channelHandle = this.CreateStream(playlistItem);
            var outputStream = new BassOutputStream(this, playlistItem, channelHandle);
            outputStream.InitializeComponent(this.Core);
            return Task.FromResult<IOutputStream>(outputStream);
        }

        public int CreateStream(PlaylistItem playlistItem)
        {
            var factory = new BassStreamFactory(this);
            return factory.CreateStream(playlistItem);
        }

        public void FreeStream(int channelHandle)
        {
            Logger.Write(this, LogLevel.Debug, "Freeing stream: {0}", channelHandle);
            Bass.StreamFree(channelHandle); //Not checking result code as it contains an error if the application is shutting down.
        }

        public override Task<bool> Preempt(IOutputStream stream)
        {
            var outputStream = stream as BassOutputStream;
            if (this.IsStarted && this.Pipeline != null)
            {
                if (this.Pipeline.Input.CheckFormat(outputStream.Rate, outputStream.Channels))
                {
                    if (this.Pipeline.Input.Add(outputStream.ChannelHandle))
                    {
                        Logger.Write(this, LogLevel.Debug, "Pre-empted playback of stream from file {0}: {1}", outputStream.FileName, outputStream.ChannelHandle);
                        return Task.FromResult(true);
                    }
                    else
                    {
                        //Probably already in the queue.
                    }
                }
                else
                {
                    Logger.Write(this, LogLevel.Debug, "Properties differ from current configuration, cannot pre-empt playback of stream from file {0}: {1}", outputStream.FileName, outputStream.ChannelHandle);
                }
            }
            else
            {
                Logger.Write(this, LogLevel.Debug, "Not yet started, cannot pre-empt playback of stream from file {0}: {1}", outputStream.FileName, outputStream.ChannelHandle);
            }
            return Task.FromResult(false);
        }

        public override Task Unload(IOutputStream stream)
        {
            var outputStream = stream as BassOutputStream;
            if (this.IsStarted && this.Pipeline != null)
            {
                if (this.Pipeline.Input.Contains(outputStream.ChannelHandle))
                {
                    var current = this.Pipeline.Input.Position(outputStream.ChannelHandle) == 0;
                    if (current)
                    {
                        this.Pipeline.Stop();
                    }
                    this.Pipeline.Input.Remove(outputStream.ChannelHandle);
                    if (current)
                    {
                        this.Pipeline.ClearBuffer();
                    }
                }
            }
            stream.Dispose();
            return Task.CompletedTask;
        }

        public IBassStreamPipeline GetOrCreatePipeline(IOutputStream stream)
        {
            if (this.Pipeline == null)
            {
                lock (BassStreamPipeline.SyncRoot)
                {
                    this.Pipeline = this.CreatePipeline(stream);
                }
            }
            else
            {
                var outputStream = stream as BassOutputStream;
                if (!this.Pipeline.Input.CheckFormat(outputStream.Rate, outputStream.Channels))
                {
                    this.FreePipeline();
                    return this.GetOrCreatePipeline(stream);
                }
            }
            return this.Pipeline;
        }

        protected virtual IBassStreamPipeline CreatePipeline(IOutputStream stream)
        {
            var outputStream = stream as BassOutputStream;
            var factory = new BassStreamPipelineFactory(this);
            var dsd = BassUtils.GetChannelDsdRaw(outputStream.ChannelHandle);
            var rate = dsd
                ? BassUtils.GetChannelDsdRate(outputStream.ChannelHandle)
                : BassUtils.GetChannelPcmRate(outputStream.ChannelHandle);
            var channels = BassUtils.GetChannelCount(outputStream.ChannelHandle);
            var pipeline = factory.CreatePipeline(dsd, rate, channels);
            pipeline.Input.Add(outputStream.ChannelHandle);
            return pipeline;
        }

        protected virtual void FreePipeline()
        {
            var pipeline = this.Pipeline;
            if (pipeline != null)
            {
                this.Pipeline = null;
                lock (BassStreamPipeline.SyncRoot)
                {
                    pipeline.Dispose();
                }
            }
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
