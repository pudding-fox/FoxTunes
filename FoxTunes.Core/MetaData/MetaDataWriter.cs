using FoxDb;
using FoxDb.Interfaces;
using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class MetaDataWriter : Disposable
    {
        const int CACHE_SIZE = 5120;

        private MetaDataWriter()
        {
            this.Store = new Cache(CACHE_SIZE);
        }

        public MetaDataWriter(IDatabaseComponent database, IDatabaseQuery query, ITransactionSource transaction) : this()
        {
            this.AddCommand = CreateCommand(database, database.Queries.GetOrAddMetaDataItem, transaction);
            this.UpdateCommand = CreateCommand(database, query, transaction);
        }

        public Cache Store { get; private set; }

        public IDatabaseCommand AddCommand { get; private set; }

        public IDatabaseCommand UpdateCommand { get; private set; }

        public async Task Write(int itemId, IEnumerable<MetaDataItem> metaDataItems)
        {
            foreach (var metaDataItem in metaDataItems)
            {
                await this.Write(itemId, metaDataItem).ConfigureAwait(false);
            }
        }

        public async Task Write(int itemId, MetaDataItem metaDataItem)
        {
            if (!this.HasValue(metaDataItem.Value))
            {
                return;
            }
            var metaDataItemId = default(int);
            if (!this.Store.TryGetValue(metaDataItem.Name, metaDataItem.Type, metaDataItem.Value, out metaDataItemId))
            {
                this.AddCommand.Parameters["name"] = metaDataItem.Name;
                this.AddCommand.Parameters["type"] = metaDataItem.Type;
                this.AddCommand.Parameters["value"] = metaDataItem.Value;
                metaDataItemId = Convert.ToInt32(await this.AddCommand.ExecuteScalarAsync().ConfigureAwait(false));
                this.Store.Add(metaDataItem.Name, metaDataItem.Type, metaDataItem.Value, metaDataItemId);
            }
            this.UpdateCommand.Parameters["itemId"] = itemId;
            this.UpdateCommand.Parameters["metaDataItemId"] = metaDataItemId;
            await this.UpdateCommand.ExecuteNonQueryAsync().ConfigureAwait(false);
        }

        private bool HasValue(string value)
        {
            return !string.IsNullOrEmpty(value) && !string.Equals(value, 0.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        protected override void OnDisposing()
        {
            this.AddCommand.Dispose();
            this.UpdateCommand.Dispose();
            base.OnDisposing();
        }

        private static IDatabaseCommand CreateCommand(IDatabase database, IDatabaseQuery query, ITransactionSource transaction)
        {
            return database.CreateCommand(query, DatabaseCommandFlags.NoCache, transaction);
        }

        public class Cache
        {
            public static HashSet<string> NAMES = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                //Tags
                CommonMetaData.Album,
                CommonMetaData.Artist,
                CommonMetaData.Composer,
                CommonMetaData.Conductor,
                CommonMetaData.Disc,
                CommonMetaData.DiscCount,
                CommonMetaData.Genre,
                CommonMetaData.Performer,
                CommonMetaData.Title,
                CommonMetaData.Track,
                CommonMetaData.TrackCount,
                CommonMetaData.Year,
                //Properties
                CommonProperties.AudioBitrate,
                CommonProperties.AudioChannels,
                CommonProperties.AudioSampleRate,
                CommonProperties.BitsPerSample,
                //Images
                CommonImageTypes.FrontCover
        };

            public Cache(int capacity)
            {
                this.Store = new CappedDictionary<Key, int>(capacity);
            }

            public CappedDictionary<Key, int> Store { get; private set; }

            public void Add(string name, MetaDataItemType type, string value, int metaDataItemId)
            {
                if (!NAMES.Contains(name))
                {
                    return;
                }
                var key = new Key(name, type, value);
                this.Store.Add(key, metaDataItemId);
            }

            public bool TryGetValue(string name, MetaDataItemType type, string value, out int metaDataItemId)
            {
                if (!NAMES.Contains(name))
                {
                    metaDataItemId = default(int);
                    return false;
                }
                var key = new Key(name, type, value);
                return this.Store.TryGetValue(key, out metaDataItemId);
            }

            public class Key : IEquatable<Key>
            {
                public Key(string name, MetaDataItemType type, string value)
                {
                    this.Name = name;
                    this.Type = type;
                    this.Value = value;
                }

                public string Name { get; set; }

                public MetaDataItemType Type { get; set; }

                public string Value { get; set; }

                public virtual bool Equals(Key other)
                {
                    if (other == null)
                    {
                        return false;
                    }
                    if (object.ReferenceEquals(this, other))
                    {
                        return true;
                    }
                    if (!string.Equals(this.Name, other.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }
                    if (this.Type != other.Type)
                    {
                        return false;
                    }
                    if (!string.Equals(this.Value, other.Value, StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }
                    return true;
                }

                public override bool Equals(object obj)
                {
                    return this.Equals(obj as Key);
                }

                public override int GetHashCode()
                {
                    var hashCode = default(int);
                    unchecked
                    {
                        if (!string.IsNullOrEmpty(this.Name))
                        {
                            hashCode += this.Name.ToLower().GetHashCode();
                        }
                        hashCode += this.Type.GetHashCode();
                        if (!string.IsNullOrEmpty(this.Value))
                        {
                            hashCode += this.Value.ToLower().GetHashCode();
                        }
                    }
                    return hashCode;
                }

                public static bool operator ==(Key a, Key b)
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

                public static bool operator !=(Key a, Key b)
                {
                    return !(a == b);
                }
            }
        }
    }
}
