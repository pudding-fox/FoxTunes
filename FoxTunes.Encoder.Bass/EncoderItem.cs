using System;
using System.Collections.Generic;
using System.Linq;

namespace FoxTunes
{
    [Serializable]
    public class EncoderItem : IEquatable<EncoderItem>
    {
        const int ERROR_CAPACITY = 10;

        private EncoderItem()
        {
            this._Errors = new List<string>(ERROR_CAPACITY);
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
            if (this._Errors.Count > ERROR_CAPACITY)
            {
                this._Errors.RemoveAt(0);
            }
        }

        public virtual bool Equals(EncoderItem other)
        {
            if (other == null)
            {
                return false;
            }
            if (object.ReferenceEquals(this, other))
            {
                return true;
            }
            if (!string.Equals(this.InputFileName, other.InputFileName, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            return true;
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as EncoderItem);
        }

        public override int GetHashCode()
        {
            var hashCode = default(int);
            unchecked
            {
                if (!string.IsNullOrEmpty(this.InputFileName))
                {
                    hashCode += this.InputFileName.ToLower().GetHashCode();
                }
            }
            return hashCode;
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

        public static bool operator ==(EncoderItem a, EncoderItem b)
        {
            if ((object)a == null && (object)b == null)
            {
                return true;
            }
            if ((object)a == null || (object)b == null)
            {
                return false;
            }
            if (object.ReferenceEquals((object)a, (object)b))
            {
                return true;
            }
            return a.Equals(b);
        }

        public static bool operator !=(EncoderItem a, EncoderItem b)
        {
            return !(a == b);
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
