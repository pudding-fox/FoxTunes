﻿using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace FoxTunes
{
    [WindowsUserInterfaceDependency]
    public class WaveFormCache : StandardComponent, IConfigurableComponent
    {
        const int CACHE_SIZE = 4;

        private static readonly string PREFIX = typeof(WaveFormCache).Name;

        public WaveFormCache()
        {
            this.Store = new CappedDictionary<Key, Lazy<WaveFormGenerator.WaveFormGeneratorData>>(CACHE_SIZE);
        }

        public CappedDictionary<Key, Lazy<WaveFormGenerator.WaveFormGeneratorData>> Store { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public BooleanConfigurationElement Enabled { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Configuration = core.Components.Configuration;
            this.Enabled = this.Configuration.GetElement<BooleanConfigurationElement>(
                WaveFormCacheConfiguration.SECTION,
                WaveFormCacheConfiguration.CACHE_ELEMENT
            );
            base.InitializeComponent(core);
        }

        public WaveFormGenerator.WaveFormGeneratorData GetOrCreate(IOutputStream stream, int resolution, Func<WaveFormGenerator.WaveFormGeneratorData> factory)
        {
            var key = new Key(stream.FileName, stream.Length, resolution);
            return this.Store.GetOrAdd(
                key,
                () => new Lazy<WaveFormGenerator.WaveFormGeneratorData>(
                    () =>
                    {
                        if (this.Enabled.Value)
                        {
                            var id = this.GetDataId(stream, resolution);
                            var fileName = default(string);
                            if (FileMetaDataStore.Exists(PREFIX, id, out fileName))
                            {
                                var data = default(WaveFormGenerator.WaveFormGeneratorData);
                                if (this.TryLoad(fileName, out data))
                                {
                                    return data;
                                }
                            }
                        }
                        return factory();
                    }
                )
            ).Value;
        }

        public bool Remove(IOutputStream stream, int resolution)
        {
            var key = new Key(stream.FileName, stream.Length, resolution);
            return this.Store.TryRemove(key);
        }

        protected virtual bool TryLoad(string fileName, out WaveFormGenerator.WaveFormGeneratorData data)
        {
            try
            {
                using (var stream = File.OpenRead(fileName))
                {
                    data = Serializer.Instance.Read(stream) as WaveFormGenerator.WaveFormGeneratorData;
                    return true;
                }
            }
            catch (Exception e)
            {
                Logger.Write(this, LogLevel.Warn, "Failed to load wave form from file \"{0}\": {1}", fileName, e.Message);
            }
            data = null;
            return false;
        }

        public void Save(IOutputStream stream, WaveFormGenerator.WaveFormGeneratorData data)
        {
            if (!this.Enabled.Value)
            {
                return;
            }

            var id = this.GetDataId(stream, data.Resolution);
            this.Save(id, data);
        }

        protected virtual void Save(string id, WaveFormGenerator.WaveFormGeneratorData data)
        {
            try
            {
                using (var stream = new MemoryStream())
                {
                    Serializer.Instance.Write(stream, data);
                    stream.Seek(0, SeekOrigin.Begin);
                    FileMetaDataStore.Write(PREFIX, id, stream);
                    stream.Seek(0, SeekOrigin.Begin);
                }
            }
            catch (Exception e)
            {
                Logger.Write(this, LogLevel.Warn, "Failed to save wave form: {0}", e.Message);
            }
        }

        private string GetDataId(IOutputStream stream, int resolution)
        {
            var hashCode = default(int);
            unchecked
            {
                hashCode = (hashCode * 29) + stream.FileName.GetDeterministicHashCode();
                hashCode = (hashCode * 29) + stream.Length.GetHashCode();
                hashCode = (hashCode * 29) + resolution.GetHashCode();
            }
            return Math.Abs(hashCode).ToString();
        }

        public IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return WaveFormCacheConfiguration.GetConfigurationSections();
        }

        public static void Cleanup()
        {
            try
            {
                var instance = ComponentRegistry.Instance.GetComponent<WaveFormCache>();
                if (instance != null)
                {
                    instance.Store.Clear();
                }
                FileMetaDataStore.Clear(PREFIX);
            }
            catch (Exception e)
            {
                Logger.Write(typeof(WaveFormCache), LogLevel.Warn, "Failed to clear caches: {0}", e.Message);
            }
        }

        public class Key : IEquatable<Key>
        {
            public Key(string fileName, long length, int resolution)
            {
                this.FileName = fileName;
                this.Length = length;
                this.Resolution = resolution;
            }

            public string FileName { get; private set; }

            public long Length { get; private set; }

            public int Resolution { get; private set; }

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
                if (!string.Equals(this.FileName, other.FileName, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
                if (this.Length != other.Length)
                {
                    return false;
                }
                if (this.Resolution != other.Resolution)
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
                    if (!string.IsNullOrEmpty(this.FileName))
                    {
                        hashCode += this.FileName.ToLower().GetHashCode();
                    }
                    hashCode += this.Length.GetHashCode();
                    hashCode += this.Resolution.GetHashCode();
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
