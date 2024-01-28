using FoxTunes.Interfaces;
using ManagedBass;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class BassReplayGainScanner : BaseComponent, IBassReplayGainScanner
    {
        private BassReplayGainScanner()
        {
            this.CancellationToken = new CancellationToken();
        }

        public BassReplayGainScanner(IEnumerable<ScannerItem> scannerItems) : this()
        {
            this.ScannerItems = scannerItems;
        }

        public CancellationToken CancellationToken { get; private set; }

        public Process Process
        {
            get
            {
                return Process.GetCurrentProcess();
            }
        }

        public IEnumerable<ScannerItem> ScannerItems { get; private set; }

        public int Threads { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            core.Components.Configuration.GetElement<IntegerConfigurationElement>(
                BassOutputConfiguration.SECTION,
                BassReplayGainScannerBehaviourConfiguration.THREADS
            ).ConnectValue(value => this.Threads = value);
            base.InitializeComponent(core);
        }

        public void Scan()
        {
            Logger.Write(this, LogLevel.Debug, "Initializing BASS (NoSound).");
            Bass.Init(Bass.NoSoundDevice);
            try
            {
                if (this.Threads > 1)
                {
                    Logger.Write(this, LogLevel.Debug, "Beginning parallel scanning with {0} threads.", this.Threads);
                }
                else
                {
                    Logger.Write(this, LogLevel.Debug, "Beginning single threaded scanning.");
                }
                var parallelOptions = new ParallelOptions()
                {
                    MaxDegreeOfParallelism = this.Threads
                };
                Parallel.ForEach(this.ScannerItems, parallelOptions, scannerItem =>
                {
                    try
                    {
                        if (this.CancellationToken.IsCancellationRequested)
                        {
                            Logger.Write(this, LogLevel.Warn, "Skipping file \"{0}\" due to cancellation.", scannerItem.FileName);
                            scannerItem.Status = ScannerItemStatus.Cancelled;
                            return;
                        }
                        if (!this.CheckInput(scannerItem.FileName))
                        {
                            Logger.Write(this, LogLevel.Warn, "Skipping file \"{0}\" due to file \"{1}\" does not exist.", scannerItem.FileName);
                            scannerItem.Status = ScannerItemStatus.Failed;
                            scannerItem.AddError(string.Format("File \"{0}\" does not exist.", scannerItem.FileName));
                            return;
                        }
                        Logger.Write(this, LogLevel.Debug, "Beginning scanning file \"{0}\".", scannerItem.FileName);
                        scannerItem.Progress = ScannerItem.PROGRESS_NONE;
                        scannerItem.Status = ScannerItemStatus.Processing;
                        this.Scan(scannerItem);
                        if (scannerItem.Status == ScannerItemStatus.Complete)
                        {
                            Logger.Write(this, LogLevel.Debug, "Scanning file \"{0}\" completed successfully.", scannerItem.FileName);
                        }
                        else
                        {
                            Logger.Write(this, LogLevel.Warn, "Scanning file \"{0}\" failed: Unknown error.", scannerItem.FileName);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        Logger.Write(this, LogLevel.Warn, "Skipping file \"{0}\" due to cancellation.", scannerItem.FileName);
                        scannerItem.Status = ScannerItemStatus.Cancelled;
                        return;
                    }
                    catch (Exception e)
                    {
                        Logger.Write(this, LogLevel.Warn, "Scanning file \"{0}\" failed: {1}", scannerItem.FileName, e.Message);
                        scannerItem.Status = ScannerItemStatus.Failed;
                        scannerItem.AddError(e.Message);
                    }
                    finally
                    {
                        scannerItem.Progress = ScannerItem.PROGRESS_COMPLETE;
                    }
                });
                Logger.Write(this, LogLevel.Debug, "Encoding completed successfully.");
            }
            finally
            {
                Logger.Write(this, LogLevel.Debug, "Releasing BASS (NoSound).");
                Bass.Free();
            }
        }

        protected virtual void Scan(ScannerItem scannerItem)
        {
            var flags = BassFlags.Decode;
            var stream = this.CreateStream(scannerItem.FileName, flags);
            if (stream.IsEmpty)
            {
                Logger.Write(this, LogLevel.Debug, "Failed to create stream for file \"{0}\": Unknown error.", scannerItem.FileName);
                return;
            }
            Logger.Write(this, LogLevel.Debug, "Created stream for file \"{0}\": {1}", scannerItem.FileName, stream.ChannelHandle);
            try
            {
                this.Scan(scannerItem, stream);
            }
            finally
            {
                Logger.Write(this, LogLevel.Debug, "Releasing stream for file \"{0}\": {1}", scannerItem.FileName, stream.ChannelHandle);
                Bass.StreamFree(stream.ChannelHandle);
            }
            if (this.CancellationToken.IsCancellationRequested)
            {
                scannerItem.Status = ScannerItemStatus.Cancelled;
            }
            else
            {
                scannerItem.Status = ScannerItemStatus.Complete;
            }
        }

        protected virtual void Scan(ScannerItem scannerItem, IBassStream stream)
        {
            throw new NotImplementedException();
        }

        public void Update()
        {
            //Nothing to do.
        }

        public void Cancel()
        {
            if (this.CancellationToken.IsCancellationRequested)
            {
                return;
            }
            Logger.Write(this, LogLevel.Warn, "Cancellation requested, shutting down.");
            this.CancellationToken.Cancel();
        }

        protected virtual bool CheckInput(string fileName)
        {
            if (!string.IsNullOrEmpty(Path.GetPathRoot(fileName)) && !File.Exists(fileName))
            {
                //TODO: Bad .Result
                if (!NetworkDrive.IsRemotePath(fileName) || !NetworkDrive.ConnectRemotePath(fileName).Result)
                {
                    throw new FileNotFoundException(string.Format("File not found: {0}", fileName), fileName);
                }
            }
            return true;
        }

        protected virtual IBassStream CreateStream(string fileName, BassFlags flags)
        {
            const int INTERVAL = 5000;
            var streamFactory = ComponentRegistry.Instance.GetComponent<IBassStreamFactory>();
            var playlistItem = new PlaylistItem()
            {
                FileName = fileName
            };
        retry:
            if (this.CancellationToken.IsCancellationRequested)
            {
                return BassStream.Empty;
            }
            //TODO: Bad .Result
            var stream = streamFactory.CreateStream(
                playlistItem,
                false,
                flags
            ).Result;
            if (stream.IsEmpty)
            {
                if (stream.Errors == Errors.Already)
                {
                    Logger.Write(this, LogLevel.Trace, "Failed to create stream for file \"{0}\": Device is already in use.", fileName);
                    Thread.Sleep(INTERVAL);
                    goto retry;
                }
                throw new InvalidOperationException(string.Format("Failed to create stream for file \"{0}\".", fileName));
            }
            return stream;
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
            //Nothing to do.
        }

        ~BassReplayGainScanner()
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
    }
}
