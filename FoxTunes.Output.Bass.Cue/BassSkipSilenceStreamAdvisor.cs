using FoxTunes.Interfaces;
using ManagedBass;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class BassSkipSilenceStreamAdvisor : BassStreamAdvisor
    {
        //50ms
        const float WINDOW = 0.05f;

        public static readonly KeyLock<string> KeyLock = new KeyLock<string>(StringComparer.OrdinalIgnoreCase);

        public override byte Priority
        {
            get
            {
                return PRIORITY_HIGH;
            }
        }

        public BassSkipSilenceStreamAdvisorBehaviour Behaviour { get; private set; }

        public IMetaDataManager MetaDataManager { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Behaviour = ComponentRegistry.Instance.GetComponent<BassSkipSilenceStreamAdvisorBehaviour>();
            this.MetaDataManager = core.Managers.MetaData;
            base.InitializeComponent(core);
        }

        public override bool Advice(PlaylistItem playlistItem, out IBassStreamAdvice advice)
        {
            if (this.Behaviour == null || !this.Behaviour.Enabled)
            {
                advice = null;
                return false;
            }

            var leadIn = default(TimeSpan);
            var leadOut = default(TimeSpan);
            if (!this.TryGetSilence(playlistItem, out leadIn, out leadOut))
            {
                advice = null;
                return false;
            }

            advice = new BassSkipSilenceStreamAdvice(playlistItem.FileName, leadIn, leadOut);
            return true;
        }

        protected virtual bool TryGetSilence(PlaylistItem playlistItem, out TimeSpan leadIn, out TimeSpan leadOut)
        {
            if (this.TryGetMetaData(playlistItem, out leadIn, out leadOut))
            {
                return true;
            }
            if (!this.TryCalculateSilence(playlistItem, out leadIn, out leadOut))
            {
                return false;
            }
            //Have to copy as cannot pass out parameter to lambda expression.
            var _leadIn = leadIn;
            var _leadOut = leadOut;
#if NET40
            var task = TaskEx.Run(() => this.UpdateMetaData(playlistItem, _leadIn, _leadOut));
#else
            var task = Task.Run(() => this.UpdateMetaData(playlistItem, _leadIn, _leadOut));
#endif
            return true;
        }

        protected virtual bool TryGetMetaData(PlaylistItem playlistItem, out TimeSpan leadIn, out TimeSpan leadOut)
        {
            Logger.Write(this, LogLevel.Debug, "Attempting to fetch lead in/out for file \"{0}\" from meta data.", playlistItem.FileName);

            var leadInMetaDataItem = default(MetaDataItem);
            var leadOutMetaDataItem = default(MetaDataItem);
            lock (playlistItem.MetaDatas)
            {
                var metaDatas = playlistItem.MetaDatas.ToDictionary(
                    metaDataItem => metaDataItem.Name,
                    StringComparer.OrdinalIgnoreCase
                );
                metaDatas.TryGetValue(CustomMetaData.LeadIn, out leadInMetaDataItem);
                metaDatas.TryGetValue(CustomMetaData.LeadOut, out leadOutMetaDataItem);
                if (leadInMetaDataItem == null && leadOutMetaDataItem == null)
                {
                    Logger.Write(this, LogLevel.Debug, "Lead in/out meta data does not exist for file \"{0}\".", playlistItem.FileName);

                    leadIn = default(TimeSpan);
                    leadOut = default(TimeSpan);
                    return false;
                }
            }

            if (leadInMetaDataItem == null)
            {
                leadIn = TimeSpan.Zero;
            }
            else if (!this.TryParseDuration(leadInMetaDataItem.Value, out leadIn))
            {
                Logger.Write(this, LogLevel.Debug, "Lead in meta data value \"{0}\" for file \"{1}\" is not valid.", leadInMetaDataItem.Value, playlistItem.FileName);

                leadIn = default(TimeSpan);
                leadOut = default(TimeSpan);
                return false;
            }

            if (leadOutMetaDataItem == null)
            {
                leadOut = TimeSpan.Zero;
            }
            else if (!this.TryParseDuration(leadOutMetaDataItem.Value, out leadOut))
            {
                Logger.Write(this, LogLevel.Debug, "Lead out meta data value \"{0}\" for file \"{1}\" is not valid.", leadOutMetaDataItem.Value, playlistItem.FileName);

                leadIn = default(TimeSpan);
                leadOut = default(TimeSpan);
                return false;
            }

            Logger.Write(this, LogLevel.Debug, "Successfully fetched lead in/out from meta data for file \"{0}\": {1}/{2}", playlistItem.FileName, leadIn, leadOut);

            return true;
        }

        protected virtual Task UpdateMetaData(PlaylistItem playlistItem, TimeSpan leadIn, TimeSpan leadOut)
        {
            Logger.Write(this, LogLevel.Debug, "Updating lead in/out meta data for file \"{0}\".", playlistItem.FileName);

            var leadInMetaDataItem = default(MetaDataItem);
            var leadOutMetaDataItem = default(MetaDataItem);
            lock (playlistItem.MetaDatas)
            {
                var metaDatas = playlistItem.MetaDatas.ToDictionary(
                    metaDataItem => metaDataItem.Name,
                    StringComparer.OrdinalIgnoreCase
                );
                if (!metaDatas.TryGetValue(CustomMetaData.LeadIn, out leadInMetaDataItem))
                {
                    leadInMetaDataItem = new MetaDataItem(
                        CustomMetaData.LeadIn,
                        MetaDataItemType.Tag
                    );
                    playlistItem.MetaDatas.Add(leadInMetaDataItem);
                }
                if (!metaDatas.TryGetValue(CustomMetaData.LeadOut, out leadOutMetaDataItem))
                {
                    leadOutMetaDataItem = new MetaDataItem(
                        CustomMetaData.LeadOut,
                        MetaDataItemType.Tag
                    );
                    playlistItem.MetaDatas.Add(leadOutMetaDataItem);
                }
                leadInMetaDataItem.Value = string.Format("{0}:{1}", this.Behaviour.Threshold, leadIn);
                leadOutMetaDataItem.Value = string.Format("{0}:{1}", this.Behaviour.Threshold, leadOut);
            }
            return this.MetaDataManager.Save(
                new[] { playlistItem },
                false, //These tags cannot be "written".
                false,
                new[] { CustomMetaData.LeadIn, CustomMetaData.LeadOut }
            );
        }

        protected virtual bool TryParseDuration(string value, out TimeSpan duration)
        {
            if (string.IsNullOrEmpty(value))
            {
                duration = TimeSpan.Zero;
                return false;
            }
            var parts = value.Split(new[] { ':' }, 2);
            if (parts.Length != 2)
            {
                duration = TimeSpan.Zero;
                return false;
            }
            var threshold = default(int);
            if (!int.TryParse(parts[0], out threshold) || this.Behaviour.Threshold != threshold)
            {
                Logger.Write(this, LogLevel.Warn, "Ignoring lead in/out value \"{0}\": Invalid threshold.", parts[0]);

                duration = TimeSpan.Zero;
                return false;
            }
            if (!TimeSpan.TryParse(parts[1], out duration))
            {
                Logger.Write(this, LogLevel.Warn, "Ignoring lead in/out value \"{0}\": Invalid duration.", parts[1]);

                duration = TimeSpan.Zero;
                return false;
            }

            return true;
        }

        protected virtual bool TryCalculateSilence(PlaylistItem playlistItem, out TimeSpan leadIn, out TimeSpan leadOut)
        {
            Logger.Write(this, LogLevel.Debug, "Attempting to calculate lead in/out for file \"{0}\".", playlistItem.FileName);

            var channelHandle = Bass.CreateStream(playlistItem.FileName, 0, 0, BassFlags.Decode | BassFlags.Byte);
            if (channelHandle == 0)
            {
                Logger.Write(this, LogLevel.Warn, "Failed to create stream for file \"{0}\": {1}", playlistItem.FileName, Enum.GetName(typeof(Errors), Bass.LastError));

                leadIn = default(TimeSpan);
                leadOut = default(TimeSpan);
                return false;
            }
            try
            {
                var leadInBytes = this.GetLeadIn(channelHandle, this.Behaviour.Threshold);
                if (leadInBytes == -1)
                {
                    Logger.Write(this, LogLevel.Warn, "Failed to calculate lead in for file \"{0}\": Track was considered silent.", playlistItem.FileName);

                    leadIn = default(TimeSpan);
                    leadOut = default(TimeSpan);
                    return false;
                }
                var leadOutBytes = this.GetLeadOut(channelHandle, this.Behaviour.Threshold);
                if (leadOutBytes == -1)
                {
                    Logger.Write(this, LogLevel.Warn, "Failed to calculate lead out for file \"{0}\": Track was considered silent.", playlistItem.FileName);

                    leadIn = default(TimeSpan);
                    leadOut = default(TimeSpan);
                    return false;
                }
                leadIn = TimeSpan.FromSeconds(
                    Bass.ChannelBytes2Seconds(
                        channelHandle,
                        leadInBytes
                    )
                );
                leadOut = TimeSpan.FromSeconds(
                    Bass.ChannelBytes2Seconds(
                        channelHandle,
                        leadOutBytes
                    )
                );

                Logger.Write(this, LogLevel.Debug, "Successfully calculated lead in/out for file \"{0}\": {1}/{2}", playlistItem.FileName, leadIn, leadOut);

                return true;
            }
            finally
            {
                Bass.StreamFree(channelHandle); //Not checking result code as it contains an error if the application is shutting down.
            }
        }

        protected virtual long GetLeadIn(int channelHandle, int threshold)
        {
            Logger.Write(this, LogLevel.Debug, "Attempting to calculate lead in for channel {0} with window of {1} seconds and threshold of {2}dB.", channelHandle, WINDOW, threshold);

            var length = Bass.ChannelSeconds2Bytes(
                channelHandle,
                WINDOW
            );
            BassUtils.OK(
                Bass.ChannelSetPosition(
                    channelHandle,
                    0,
                    PositionFlags.Bytes
                )
            );
            do
            {
                var levels = new float[1];
                if (!Bass.ChannelGetLevel(channelHandle, levels, WINDOW, LevelRetrievalFlags.Mono | LevelRetrievalFlags.RMS))
                {
                    Logger.Write(this, LogLevel.Warn, "Failed to get levels for channel {0}: {1}", channelHandle, Enum.GetName(typeof(Errors), Bass.LastError));

                    break;
                }
                var dB = levels[0] > 0 ? 20 * Math.Log10(levels[0]) : -1000;
                if (dB > threshold)
                {
                    //TODO: Sometimes this value is less than zero so clamp it.
                    //TODO: Some problem with BASS/ManagedBass, if you have exactly N bytes available call Bass.ChannelGetLevel with Length = Bass.ChannelBytesToSeconds(N) sometimes results in Errors.Ended.
                    //TODO: Nuts.
                    var leadIn = Math.Max(Bass.ChannelGetPosition(channelHandle, PositionFlags.Bytes) - length, 0);

                    if (leadIn > 0)
                    {
                        Logger.Write(this, LogLevel.Debug, "Successfully calculated lead in for channel {0}: {1} bytes.", channelHandle, leadIn);
                    }
                    else
                    {
                        Logger.Write(this, LogLevel.Debug, "Successfully calculated lead in for channel {0}: None.", channelHandle);
                    }

                    return leadIn;
                }
            } while (true);
            //Track was silent?
            return -1;
        }

        protected virtual long GetLeadOut(int channelHandle, int threshold)
        {
            Logger.Write(this, LogLevel.Debug, "Attempting to calculate lead out for channel {0} with window of {1} seconds and threshold of {2}dB.", channelHandle, WINDOW, threshold);

            var length = Bass.ChannelSeconds2Bytes(
                channelHandle,
                WINDOW
            );
            BassUtils.OK(
                Bass.ChannelSetPosition(
                    channelHandle,
                    Bass.ChannelGetLength(
                        channelHandle,
                        PositionFlags.Bytes
                    ) - (length * 2),
                    PositionFlags.Bytes
                )
            );
            do
            {
                var levels = new float[1];
                if (!Bass.ChannelGetLevel(channelHandle, levels, WINDOW, LevelRetrievalFlags.Mono | LevelRetrievalFlags.RMS))
                {
                    Logger.Write(this, LogLevel.Warn, "Failed to get levels for channel {0}: {1}", channelHandle, Enum.GetName(typeof(Errors), Bass.LastError));

                    break;
                }
                var dB = levels[0] > 0 ? 20 * Math.Log10(levels[0]) : -1000;
                if (dB > threshold)
                {
                    //TODO: Sometimes this value is less than zero so clamp it.
                    //TODO: Some problem with BASS/ManagedBass, if you have exactly N bytes available call Bass.ChannelGetLevel with Length = Bass.ChannelBytesToSeconds(N) sometimes results in Errors.Ended.
                    //TODO: Nuts.
                    var leadOut = Math.Max(Bass.ChannelGetLength(channelHandle, PositionFlags.Bytes) - Bass.ChannelGetPosition(channelHandle, PositionFlags.Bytes) - length, 0);

                    if (leadOut > 0)
                    {
                        Logger.Write(this, LogLevel.Debug, "Successfully calculated lead out for channel {0}: {1} bytes.", channelHandle, leadOut);
                    }
                    else
                    {
                        Logger.Write(this, LogLevel.Debug, "Successfully calculated lead out for channel {0}: None.", channelHandle);
                    }

                    return leadOut;
                }
                if (!Bass.ChannelSetPosition(
                    channelHandle,
                    Bass.ChannelGetPosition(
                        channelHandle,
                        PositionFlags.Bytes
                    ) - (length * 2),
                    PositionFlags.Bytes
                ))
                {
                    break;
                }
            } while (true);
            //Track was silent?
            return -1;
        }
    }
}
