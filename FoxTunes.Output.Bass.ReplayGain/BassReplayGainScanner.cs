using FoxTunes.Interfaces;
using ManagedBass;
using ManagedBass.ReplayGain;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class BassReplayGainScanner : BaseComponent, IBassReplayGainScanner
    {
        const string GROUP_NONE = "None";

        const string GROUP_EMPTY = "Empty";

        static BassReplayGainScanner()
        {
            BassPluginLoader.Instance.Load();
        }

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
            BassReplayGain.Init();
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
                var scannerItems = default(IEnumerable<ScannerItem>);
                var scannerGroups = default(IEnumerable<IEnumerable<ScannerItem>>);
                this.GetGroups(out scannerItems, out scannerGroups);
                this.ScanTracks(scannerItems);
                this.ScanGroups(scannerGroups);
                Logger.Write(this, LogLevel.Debug, "Scanning completed successfully.");
            }
            finally
            {
                Logger.Write(this, LogLevel.Debug, "Releasing BASS (NoSound).");
                BassReplayGain.Free();
                Bass.Free();
            }
        }

        protected virtual void ScanTracks(IEnumerable<ScannerItem> scannerItems)
        {
            var parallelOptions = new ParallelOptions()
            {
                MaxDegreeOfParallelism = this.Threads
            };
            Parallel.ForEach(scannerItems, parallelOptions, scannerItem =>
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
                    this.ScanTrack(scannerItem);
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
        }

        protected virtual void ScanTrack(ScannerItem scannerItem)
        {
            var success = default(bool);
            var flags = BassFlags.Decode | BassFlags.Float;
            var stream = this.CreateStream(scannerItem.FileName, flags);
            if (stream.IsEmpty)
            {
                Logger.Write(this, LogLevel.Warn, "Failed to create stream for file \"{0}\": Unknown error.", scannerItem.FileName);
                throw new InvalidOperationException(string.Format("Failed to create stream for file \"{0}\": Unknown error.", scannerItem.FileName));
            }
            Logger.Write(this, LogLevel.Debug, "Created stream for file \"{0}\": {1}", scannerItem.FileName, stream.ChannelHandle);
            var monitor = new ChannelMonitor(scannerItem, stream);
            try
            {
                monitor.Start();
                success = this.ScanTrack(scannerItem, stream);
            }
            finally
            {
                monitor.Dispose();
                Logger.Write(this, LogLevel.Debug, "Releasing stream for file \"{0}\": {1}", scannerItem.FileName, stream.ChannelHandle);
                Bass.StreamFree(stream.ChannelHandle);
            }
            if (this.CancellationToken.IsCancellationRequested)
            {
                scannerItem.Status = ScannerItemStatus.Cancelled;
            }
            else if (success)
            {
                scannerItem.Status = ScannerItemStatus.Complete;
            }
            else
            {
                scannerItem.Status = ScannerItemStatus.Failed;
            }
        }

        protected virtual bool ScanTrack(ScannerItem scannerItem, IBassStream stream)
        {
            var info = default(ReplayGainInfo);
            if (!BassReplayGain.Process(stream.ChannelHandle, out info))
            {
                return false;
            }
            scannerItem.ItemPeak = info.peak;
            scannerItem.ItemGain = info.gain;
            return true;
        }

        protected virtual void ScanGroups(IEnumerable<IEnumerable<ScannerItem>> groups)
        {
            var parallelOptions = new ParallelOptions()
            {
                MaxDegreeOfParallelism = this.Threads
            };
            Parallel.ForEach(groups, parallelOptions, group =>
            {
                var streams = new Dictionary<ScannerItem, IBassStream>();
                foreach (var scannerItem in group)
                {
                    try
                    {
                        if (this.CancellationToken.IsCancellationRequested)
                        {
                            Logger.Write(this, LogLevel.Warn, "Skipping file \"{0}\" due to cancellation.", scannerItem.FileName);
                            scannerItem.Status = ScannerItemStatus.Cancelled;
                            continue;
                        }
                        if (!this.CheckInput(scannerItem.FileName))
                        {
                            Logger.Write(this, LogLevel.Warn, "Skipping file \"{0}\" due to file \"{1}\" does not exist.", scannerItem.FileName);
                            scannerItem.Status = ScannerItemStatus.Failed;
                            scannerItem.AddError(string.Format("File \"{0}\" does not exist.", scannerItem.FileName));
                            continue;
                        }
                        var flags = BassFlags.Decode | BassFlags.Float;
                        var stream = this.CreateStream(scannerItem.FileName, flags);
                        if (stream.IsEmpty)
                        {
                            Logger.Write(this, LogLevel.Warn, "Failed to create stream for file \"{0}\": Unknown error.", scannerItem.FileName);
                            throw new InvalidOperationException(string.Format("Failed to create stream for file \"{0}\": Unknown error.", scannerItem.FileName));
                        }
                        Logger.Write(this, LogLevel.Debug, "Created stream for file \"{0}\": {1}", scannerItem.FileName, stream.ChannelHandle);
                        streams.Add(scannerItem, stream);
                        Logger.Write(this, LogLevel.Debug, "Beginning scanning file \"{0}\".", scannerItem.FileName);
                        scannerItem.Progress = ScannerItem.PROGRESS_NONE;
                        scannerItem.Status = ScannerItemStatus.Processing;
                    }
                    catch (OperationCanceledException)
                    {
                        Logger.Write(this, LogLevel.Warn, "Skipping file \"{0}\" due to cancellation.", scannerItem.FileName);
                        scannerItem.Status = ScannerItemStatus.Cancelled;
                    }
                    catch (Exception e)
                    {
                        Logger.Write(this, LogLevel.Warn, "Scanning file \"{0}\" failed: {1}", scannerItem.FileName, e.Message);
                        scannerItem.Status = ScannerItemStatus.Failed;
                        scannerItem.AddError(e.Message);
                    }
                }
                var success = default(bool);
                var monitors = new List<ChannelMonitor>();
                try
                {
                    foreach (var scannerItem in streams.Keys)
                    {
                        var stream = streams[scannerItem];
                        var monitor = new ChannelMonitor(scannerItem, stream);
                        monitor.Start();
                        monitors.Add(monitor);
                    }
                    success = this.ScanGroup(streams);
                }
                finally
                {
                    foreach (var monitor in monitors)
                    {
                        monitor.Dispose();
                    }
                    foreach (var scannerItem in streams.Keys)
                    {
                        var stream = streams[scannerItem];
                        Logger.Write(this, LogLevel.Debug, "Releasing stream for file \"{0}\": {1}", scannerItem.FileName, stream.ChannelHandle);
                        Bass.StreamFree(stream.ChannelHandle);
                    }
                }
                foreach (var scannerItem in streams.Keys)
                {
                    if (this.CancellationToken.IsCancellationRequested)
                    {
                        scannerItem.Status = ScannerItemStatus.Cancelled;
                    }
                    else if (success)
                    {
                        Logger.Write(this, LogLevel.Debug, "Scanning file \"{0}\" completed successfully.", scannerItem.FileName);
                        scannerItem.Status = ScannerItemStatus.Complete;
                    }
                    else
                    {
                        Logger.Write(this, LogLevel.Warn, "Scanning file \"{0}\" failed: Unknown error.", scannerItem.FileName);
                        scannerItem.Status = ScannerItemStatus.Failed;
                    }
                }
            });
        }

        protected virtual bool ScanGroup(IDictionary<ScannerItem, IBassStream> group)
        {
            if (group.Count == 0)
            {
                //Nothing to do.
                return true;
            }
            var channelHandles = group.Values
                .Select(stream => stream.ChannelHandle)
                .ToArray();
            var info = default(ReplayGainBatchInfo);
            if (!BassReplayGain.ProcessBatch(channelHandles, out info))
            {
                return false;
            }
            foreach (var scannerItem in group.Keys)
            {
                if (scannerItem.Status != ScannerItemStatus.Processing)
                {
                    continue;
                }
                var stream = group[scannerItem];
                foreach (var item in info.items)
                {
                    if (item.handle == stream.ChannelHandle)
                    {
                        scannerItem.ItemPeak = item.peak;
                        scannerItem.ItemGain = item.gain;
                        scannerItem.GroupPeak = info.peak;
                        scannerItem.GroupGain = info.gain;
                        break;
                    }
                }
            }
            return true;
        }

        protected virtual void GetGroups(out IEnumerable<ScannerItem> scannerItems, out IEnumerable<IEnumerable<ScannerItem>> scannerGroups)
        {
            var items = new List<ScannerItem>();
            var groups = new Dictionary<string, List<ScannerItem>>(StringComparer.OrdinalIgnoreCase);
            foreach (var scannerItem in this.ScannerItems)
            {
                if (scannerItem.Mode != ReplayGainMode.Album)
                {
                    items.Add(scannerItem);
                    continue;
                }
                var name = scannerItem.GroupName;
                if (string.IsNullOrEmpty(name))
                {
                    name = GROUP_NONE;
                }
                var group = groups.GetOrAdd(name, key => new List<ScannerItem>());
                group.Add(scannerItem);
            }
            scannerItems = items;
            scannerGroups = groups.Values;
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
