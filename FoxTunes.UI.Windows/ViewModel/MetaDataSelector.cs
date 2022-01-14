using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace FoxTunes.ViewModel
{
    public static class MetaDataSelector
    {
        public static readonly IMetaDataSourceFactory SourceFactory = ComponentRegistry.Instance.GetComponent<IMetaDataSourceFactory>();

        public static readonly IMetaDataDecoratorFactory DecoratorFactory = ComponentRegistry.Instance.GetComponent<IMetaDataDecoratorFactory>();

        public static readonly IValueConverter Converter = new EnumConverter();

        public static IEnumerable<MetaDataElement> Elements
        {
            get
            {
                var supported = SourceFactory.Supported;
                if (DecoratorFactory.CanCreate)
                {
                    supported = supported.Concat(DecoratorFactory.Supported);
                }
                return supported.Select(element => new MetaDataElement(element.Key, element.Value)).ToArray();
            }
        }

        public static readonly IEnumerable<string> Formats = GetFormats();

        private static IEnumerable<string> GetFormats()
        {
            return new[]
            {
                CommonFormats.Decibel,
                CommonFormats.Float,
                CommonFormats.Integer,
                CommonFormats.TimeSpan,
                CommonFormats.TimeStamp
            };
        }

        public class MetaDataElement
        {
            public MetaDataElement(string name, MetaDataItemType type)
            {
                this.Name = name;
                this.Type = Convert.ToString(
                    Converter.Convert(type, typeof(string), null, CultureInfo.InvariantCulture)
                );
            }

            public string Name { get; private set; }

            public string Type { get; private set; }
        }
    }
}
