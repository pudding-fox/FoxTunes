using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;

namespace FoxTunes
{
    public class EncoderItem : MarshalByRefObject
    {
        private EncoderItem()
        {
            this._Errors = new List<string>();
        }

        public const int PROGRESS_NONE = 0;

        public const int PROGRESS_COMPLETE = 100;

        public string InputFileName { get; private set; }

        public string OutputFileName { get; set; }

        public int Bitrate { get; private set; }

        public int Channels { get; private set; }

        public int SampleRate { get; private set; }

        public int BitsPerSample { get; private set; }

        private IList<string> _Errors { get; set; }

        public IEnumerable<string> Errors
        {
            get
            {
                return this._Errors;
            }
        }

        public int Progress { get; set; }

        public EncoderItemStatus Status { get; set; }

        public void AddError(string error)
        {
            this._Errors.Add(error);
        }

        public static EncoderItem FromPlaylistItem(PlaylistItem playlistItem)
        {
            var encoderItem = new EncoderItem()
            {
                InputFileName = playlistItem.FileName
            };
            if (playlistItem.MetaDatas != null)
            {
                var metaData = playlistItem.MetaDatas.ToDictionary(metaDataItem => metaDataItem.Name, metaDataItem => metaDataItem.Value, StringComparer.OrdinalIgnoreCase);
                encoderItem.Bitrate = Convert.ToInt32(metaData.GetValueOrDefault(CommonProperties.AudioBitrate));
                encoderItem.Channels = Convert.ToInt32(metaData.GetValueOrDefault(CommonProperties.AudioChannels));
                encoderItem.SampleRate = Convert.ToInt32(metaData.GetValueOrDefault(CommonProperties.AudioSampleRate));
                encoderItem.BitsPerSample = Convert.ToInt32(metaData.GetValueOrDefault(CommonProperties.BitsPerSample));
            }
            return encoderItem;
        }

        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.Infrastructure)]
        public override object InitializeLifetimeService()
        {
            //Disable the 5 minute lease default.
            return null;
        }
    }

    public enum EncoderItemStatus
    {
        None,
        Processing,
        Complete,
        Cancelled,
        Failed
    }
}
