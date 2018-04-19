using System;
using System.Collections.Generic;
using System.Linq;

namespace FoxTunes
{
    public static partial class Extensions
    {
        public static T GetMetaDataValue<T>(this IEnumerable<MetaDataItem> sequence, MetaDataItemType type, string name)
        {
            var match = sequence.FirstOrDefault(element => element.Type == type && string.Equals(element.Name, name, StringComparison.OrdinalIgnoreCase));
            if (match == null)
            {
                return default(T);
            }
            if (match.Value is T)
            {
                return (T)match.Value;
            }
            try
            {
                return (T)Convert.ChangeType(match.Value, typeof(T));
            }
            catch
            {
                return default(T);
            }
        }
    }
}
