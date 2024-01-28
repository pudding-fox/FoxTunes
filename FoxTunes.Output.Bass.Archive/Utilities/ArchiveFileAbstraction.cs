using FoxTunes.Interfaces;
using ManagedBass.ZipStream;
using System;
using System.IO;

namespace FoxTunes
{
    public class ArchiveFileAbstraction : BaseComponent, IFileAbstraction
    {
        private IntPtr Entry;

        private ArchiveFileAbstraction()
        {
            this.Entry = IntPtr.Zero;
        }

        public ArchiveFileAbstraction(string fileName, string entryName, int index) : this()
        {
            this.FileName = fileName;
            this.EntryName = entryName;
            this.Index = index;
        }

        string IFileAbstraction.FileName
        {
            get
            {
                return this.EntryName;
            }
        }

        public string FileName { get; private set; }

        public string EntryName { get; private set; }

        public int Index { get; private set; }

        public Stream ReadStream { get; private set; }

        public Stream WriteStream { get; private set; }

        public bool IsOpen { get; private set; }

        public long Result
        {
            get
            {
                var result = default(long);
                if (!IntPtr.Zero.Equals(this.Entry))
                {
                    ArchiveEntry.GetEntryResult(this.Entry, out result);
                }
                return result;
            }
        }

        public void Open()
        {
            if (!ArchiveEntry.OpenEntry(this.FileName, this.Index, out this.Entry))
            {
                return;
            }
            this.ReadStream = new ArchiveEntryStream(this.Entry);
            this.IsOpen = true;
        }

        public void Close()
        {
            if (this.ReadStream != null)
            {
                this.ReadStream.Dispose();
                this.ReadStream = null;
            }
            if (!IntPtr.Zero.Equals(this.Entry))
            {
                ArchiveEntry.CloseEntry(this.Entry);
                this.Entry = IntPtr.Zero;
            }
            this.IsOpen = false;
        }

        public void CloseStream(Stream stream)
        {
            stream.Close();
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
            this.Close();
        }

        ~ArchiveFileAbstraction()
        {
            Logger.Write(this.GetType(), LogLevel.Error, "Component was not disposed: {0}", this.GetType().Name);
            try
            {
                this.Dispose(true);
            }
            catch
            {
                //Nothing can be done, never throw on GC thread.
            }
        }

        public static ArchiveFileAbstraction Create(string fileName, string entryName, int index)
        {
            var fileAbstraction = new ArchiveFileAbstraction(fileName, entryName, index);
            fileAbstraction.Open();
            return fileAbstraction;
        }
    }
}
