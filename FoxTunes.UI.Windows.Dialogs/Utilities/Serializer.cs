using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace FoxTunes
{
    public static class Serializer
    {
        public static readonly IDictionary<Type, XmlSerializer> Formatters = new Dictionary<Type, XmlSerializer>();

        public static XmlSerializer GetFormatter<T>()
        {
            return Formatters.GetOrAdd(typeof(T), type => new XmlSerializer(type));
        }

        public static string SaveValue<T>(T value)
        {
            var formatter = GetFormatter<T>();
            using (var stream = new MemoryStream())
            {
                formatter.Serialize(stream, value);
                return Encoding.Default.GetString(stream.ToArray());
            }
        }

        public static T LoadValue<T>(string value)
        {
            var formatter = GetFormatter<T>();
            using (var stream = new MemoryStream(Encoding.Default.GetBytes(value)))
            {
                return (T)formatter.Deserialize(stream);
            }
        }
    }
}
