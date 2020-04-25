using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace FoxTunes
{
    public class MetaDataItem : PersistableComponent, INamedValue
    {
        public MetaDataItem()
        {

        }

        public MetaDataItem(string name, MetaDataItemType type)
        {
            this.Name = name;
            this.Type = type;
        }

        public string Name { get; set; }

        public MetaDataItemType Type { get; set; }

        private string _Value { get; set; }

        public string Value
        {
            get
            {
                return this._Value;
            }
            set
            {
                this._Value = value;
                this.OnValueChanged();
            }
        }

        protected virtual void OnValueChanged()
        {
            if (this.ValueChanged != null)
            {
                this.ValueChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Value");
        }

        public event EventHandler ValueChanged;

        public bool IsNumeric
        {
            get
            {
                if (string.IsNullOrEmpty(this.Value))
                {
                    return false;
                }
                for (var position = 0; position < this.Value.Length; position++)
                {
                    if (this.Value[position] >= '0' && this.Value[position] <= '9')
                    {
                        continue;
                    }
                    if (position == 0 && this.Value[position] == '-')
                    {
                        continue;
                    }
                    if (position > 0 && position < this.Value.Length - 1 && this.Value[position] == '.')
                    {
                        continue;
                    }
                    return false;
                }
                return true;
            }
        }

        public bool IsFile
        {
            get
            {
                if (string.IsNullOrEmpty(this.Value))
                {
                    return false;
                }
                var regex = new Regex(@"((?:[a-zA-Z]\:(\\|\/)|file\:\/\/|\\\\|\.(\/|\\))([^\\\/\:\*\?\<\>\""\|]+(\\|\/){0,1})+)");
                var matches = regex.Matches(this.Value);
                for (var a = 0; a < matches.Count; a++)
                {
                    var match = matches[a];
                    if (match.Success)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public static void Update(IFileData source, IFileData destination)
        {
            Update(source, new[] { destination });
        }

        public static void Update(IFileData source, IEnumerable<IFileData> destinations)
        {
            lock (source.MetaDatas)
            {
                var sourceMetaData = source.MetaDatas.ToDictionary(
                    metaDataItem => metaDataItem.Name,
                    StringComparer.OrdinalIgnoreCase
                );
                foreach (var destination in destinations)
                {
                    lock (destination.MetaDatas)
                    {
                        var destinationMetaData = destination.MetaDatas.ToDictionary(
                            metaDataItem => metaDataItem.Name,
                            StringComparer.OrdinalIgnoreCase
                        );
                        foreach (var sourceMetaDataItem in source.MetaDatas)
                        {
                            var destinationMetaDataItem = default(MetaDataItem);
                            if (destinationMetaData.TryGetValue(sourceMetaDataItem.Name, out destinationMetaDataItem))
                            {
                                destinationMetaDataItem.Value = sourceMetaDataItem.Value;
                            }
                            else
                            {
                                destination.MetaDatas.Add(sourceMetaDataItem);
                            }
                        }
                        foreach (var pair in destinationMetaData)
                        {
                            if (!sourceMetaData.ContainsKey(pair.Key))
                            {
                                pair.Value.Value = string.Empty;
                            }
                        }
                    }
                }
            }
        }

        public static readonly MetaDataItem Empty = new MetaDataItem();
    }

    public enum MetaDataItemType : byte
    {
        None = 0,
        Tag = 1,
        Property = 2,
        Image = 4,
        Statistic = 8,
        All = Tag | Property | Image | Statistic
    }
}
