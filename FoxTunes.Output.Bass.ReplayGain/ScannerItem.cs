using System;
using System.Collections.Generic;

namespace FoxTunes
{
    [Serializable]
    public class ScannerItem
    {
        const int ERROR_CAPACITY = 10;

        private ScannerItem()
        {
            this.Id = Guid.NewGuid();
            this._Errors = new List<string>(ERROR_CAPACITY);
        }

        public const int PROGRESS_NONE = 0;

        public const int PROGRESS_COMPLETE = 100;

        public Guid Id { get; private set; }

        public string FileName { get; private set; }

        private IList<string> _Errors { get; set; }

        public IEnumerable<string> Errors
        {
            get
            {
                return this._Errors;
            }
        }

        public int Progress { get; set; }

        public ScannerItemStatus Status { get; set; }

        public void AddError(string error)
        {
            this._Errors.Add(error);
            if (this._Errors.Count > ERROR_CAPACITY)
            {
                this._Errors.RemoveAt(0);
            }
        }

        public static ScannerItem FromPlaylistItem(PlaylistItem playlistItem)
        {
            var encoderItem = new ScannerItem()
            {
                FileName = playlistItem.FileName
            };
            return encoderItem;
        }
    }

    public enum ScannerItemStatus : byte
    {
        None,
        Processing,
        Complete,
        Cancelled,
        Failed
    }
}
