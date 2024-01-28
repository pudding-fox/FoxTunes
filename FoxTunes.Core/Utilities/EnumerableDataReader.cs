using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;

namespace FoxTunes
{
    public class EnumerableDataReader : IEnumerable<EnumerableDataReader.EnumerableDataReaderRow>, IDisposable
    {
        public EnumerableDataReader(IDataReader reader)
        {
            this.Reader = reader;
        }

        public IDataReader Reader { get; private set; }

        public IEnumerator<EnumerableDataReader.EnumerableDataReaderRow> GetEnumerator()
        {
            while (this.Reader.Read())
            {
                yield return new EnumerableDataReader.EnumerableDataReaderRow(this.Reader);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
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
            this.Reader.Dispose();
        }

        public static EnumerableDataReader Create(IDataReader reader)
        {
            return new EnumerableDataReader(reader);
        }

        public class EnumerableDataReaderRow : Dictionary<string, object>
        {
            public EnumerableDataReaderRow(IDataReader reader)
            {
                for (var a = 0; a < reader.FieldCount; a++)
                {
                    var key = reader.GetName(a);
                    var value = reader.GetValue(a);
                    this.Add(key, value);
                }
            }
        }
    }
}
