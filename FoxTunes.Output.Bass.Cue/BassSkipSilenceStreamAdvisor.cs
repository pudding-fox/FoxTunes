using FoxTunes.Interfaces;
using ManagedBass;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class BassSkipSilenceStreamAdvisor : BassStreamAdvisor
    {
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
            if (!FileSystemHelper.IsLocalPath(playlistItem.FileName))
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
            lock (playlistItem.MetaDatas)
            {
                var metaDatas = playlistItem.MetaDatas.ToDictionary(
                    metaDataItem => metaDataItem.Name,
                    StringComparer.OrdinalIgnoreCase
                );
                if (this.TryGetSilence(metaDatas, out leadIn, out leadOut))
                {
                    return true;
                }
            }
            if (!this.TryCalculateSilence(playlistItem, out leadIn, out leadOut))
            {
                return false;
            }
            lock (playlistItem.MetaDatas)
            {
                playlistItem.MetaDatas.Add(
                    new MetaDataItem(
                        CustomMetaData.LeadIn,
                        MetaDataItemType.Tag
                    )
                    {
                        Value = leadIn.ToString()
                    }
                );
                playlistItem.MetaDatas.Add(
                    new MetaDataItem(
                        CustomMetaData.LeadOut,
                        MetaDataItemType.Tag
                    )
                    {
                        Value = leadOut.ToString()
                    }
                );
            }
#if NET40
            var task = TaskEx.Run(() => this.UpdateMetaData(playlistItem));
#else
            var task = Task.Run(() => this.UpdateMetaData(playlistItem));
#endif
            return true;
        }

        protected virtual Task UpdateMetaData(PlaylistItem playlistItem)
        {
            return this.MetaDataManager.Save(
                new[] { playlistItem },
                false, //These tags cannot be "written".
                false,
                new[] { CustomMetaData.LeadIn, CustomMetaData.LeadOut }
            );
        }

        protected virtual bool TryGetSilence(IDictionary<string, MetaDataItem> metaDatas, out TimeSpan leadIn, out TimeSpan leadOut)
        {
            var leadInMetaDataItem = default(MetaDataItem);
            var leadOutMetaDataItem = default(MetaDataItem);
            if (!metaDatas.TryGetValue(CustomMetaData.LeadIn, out leadInMetaDataItem) && !metaDatas.TryGetValue(CustomMetaData.LeadOut, out leadOutMetaDataItem))
            {
                leadIn = default(TimeSpan);
                leadOut = default(TimeSpan);
                return false;
            }

            if (leadInMetaDataItem == null || !TimeSpan.TryParse(leadInMetaDataItem.Value, out leadIn))
            {
                leadIn = TimeSpan.Zero;
            }
            if (leadOutMetaDataItem == null || !TimeSpan.TryParse(leadOutMetaDataItem.Value, out leadOut))
            {
                leadOut = TimeSpan.Zero;
            }
            return true;
        }

        protected virtual bool TryCalculateSilence(PlaylistItem playlistItem, out TimeSpan leadIn, out TimeSpan leadOut)
        {
            var channelHandle = Bass.CreateStream(playlistItem.FileName, 0, 0, BassFlags.Decode | BassFlags.Byte);
            if (channelHandle == 0)
            {
                leadIn = default(TimeSpan);
                leadOut = default(TimeSpan);
                return false;
            }
            try
            {
                var leadInBytes = this.GetLeadIn(channelHandle);
                var leadOutBytes = this.GetLeadOut(channelHandle);
                if (leadInBytes == -1 || leadOutBytes == -1)
                {
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
                return true;
            }
            finally
            {
                Bass.StreamFree(channelHandle); //Not checking result code as it contains an error if the application is shutting down.
            }
        }

        protected virtual long GetLeadIn(int channelHandle)
        {
            var bytesPerSecond = Bass.ChannelSeconds2Bytes(
                channelHandle,
                1
            );
            var buffer = new byte[bytesPerSecond];
            var leadIn = default(long);
            do
            {
                if (Bass.ChannelIsActive(channelHandle) != PlaybackState.Playing)
                {
                    break;
                }
                var count = Bass.ChannelGetData(
                    channelHandle,
                    buffer,
                    buffer.Length
                );
                if (count <= 0)
                {
                    break;
                }
                for (var position = 0; position < count; position++)
                {
                    if (buffer[position] != 0)
                    {
                        return leadIn;
                    }
                    leadIn++;
                }
            } while (true);
            //Track was silent?
            return -1;
        }

        protected virtual long GetLeadOut(int channelHandle)
        {
            var bytesPerSecond = Bass.ChannelSeconds2Bytes(
               channelHandle,
               1
            );
            var buffer = new byte[bytesPerSecond];
            var leadOut = default(long);
            BassUtils.OK(
                Bass.ChannelSetPosition(
                    channelHandle,
                    Bass.ChannelGetLength(
                        channelHandle,
                        PositionFlags.Bytes
                    ) - bytesPerSecond,
                    PositionFlags.Bytes
                )
            );
            do
            {
                if (Bass.ChannelIsActive(channelHandle) != PlaybackState.Playing)
                {
                    break;
                }
                var count = Bass.ChannelGetData(
                    channelHandle,
                    buffer,
                    buffer.Length
                );
                if (count <= 0)
                {
                    break;
                }
                for (var position = count - 1; position > 0; position--)
                {
                    if (buffer[position] != 0)
                    {
                        return leadOut;
                    }
                    leadOut++;
                }
                BassUtils.OK(
                    Bass.ChannelSetPosition(
                        channelHandle,
                        Bass.ChannelGetPosition(
                            channelHandle,
                            PositionFlags.Bytes
                        ) - (bytesPerSecond * 2),
                        PositionFlags.Bytes
                    )
                );
            } while (true);
            //Track was silent?
            return -1;
        }
    }
}
