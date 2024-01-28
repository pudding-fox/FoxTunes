using System;
using System.Collections.Generic;
using System.Linq;

namespace FoxTunes
{
    [Serializable]
    public class EncoderItem
    {
        const int ERROR_CAPACITY = 10;

        private EncoderItem()
        {
            this.Id = Guid.NewGuid();
            this._Errors = new List<string>(ERROR_CAPACITY);
        }

        public const int PROGRESS_NONE = 0;

        public const int PROGRESS_COMPLETE = 100;

        public Guid Id { get; private set; }

        public string InputFileName { get; private set; }

        public string OutputFileName { get; set; }

        public string Profile { get; private set; }

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
            if (this._Errors.Count > ERROR_CAPACITY)
            {
                this._Errors.RemoveAt(0);
            }
        }

        public static EncoderItem FromPlaylistItem(PlaylistItem playlistItem, string profile)
        {
            var encoderItem = new EncoderItem()
            {
                InputFileName = playlistItem.FileName,
                Profile = profile
            };
            if (playlistItem.MetaDatas != null)
            {
                var metaData = default(IDictionary<string, string>);
                lock (playlistItem.MetaDatas)
                {
                    metaData = playlistItem.MetaDatas.ToDictionary(
                        metaDataItem => metaDataItem.Name,
                        metaDataItem => metaDataItem.Value,
                        StringComparer.OrdinalIgnoreCase
                    );
                }
                encoderItem.Bitrate = Convert.ToInt32(metaData.GetValueOrDefault(CommonProperties.AudioBitrate));
                encoderItem.Channels = Convert.ToInt32(metaData.GetValueOrDefault(CommonProperties.AudioChannels));
                encoderItem.SampleRate = Convert.ToInt32(metaData.GetValueOrDefault(CommonProperties.AudioSampleRate));
                encoderItem.BitsPerSample = Convert.ToInt32(metaData.GetValueOrDefault(CommonProperties.BitsPerSample));
            }
            return encoderItem;
        }
    }

    public enum EncoderItemStatus : byte
    {
        None,
        Processing,
        Complete,
        Cancelled,
        Failed
    }
}
