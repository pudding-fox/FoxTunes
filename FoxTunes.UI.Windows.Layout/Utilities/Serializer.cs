using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Xml;

namespace FoxTunes
{
    public static class Serializer
    {
        private static ILogger Logger
        {
            get
            {
                return LogManager.Logger;
            }
        }

        public static void Save(Stream stream, IEnumerable<ToolWindowConfiguration> configs)
        {
            using (var writer = new XmlTextWriter(stream, Encoding.Default))
            {
                writer.Formatting = Formatting.Indented;
                writer.WriteStartDocument();
                writer.WriteStartElement(Publication.Product);
                foreach (var config in configs)
                {
                    Logger.Write(typeof(Serializer), LogLevel.Trace, "Saving tool window configuration: \"{0}\".", config.Id);
                    writer.WriteStartElement(nameof(ToolWindowConfiguration));
                    writer.WriteAttributeString(nameof(ToolWindowConfiguration.Id), config.Id);
                    writer.WriteAttributeString(nameof(ToolWindowConfiguration.Title), config.Title);
                    writer.WriteAttributeString(nameof(ToolWindowConfiguration.Left), Convert.ToString(config.Left));
                    writer.WriteAttributeString(nameof(ToolWindowConfiguration.Top), Convert.ToString(config.Top));
                    writer.WriteAttributeString(nameof(ToolWindowConfiguration.Width), Convert.ToString(config.Width));
                    writer.WriteAttributeString(nameof(ToolWindowConfiguration.Height), Convert.ToString(config.Height));
                    writer.WriteAttributeString(nameof(ToolWindowConfiguration.ShowWithMainWindow), Convert.ToString(config.ShowWithMainWindow));
                    writer.WriteAttributeString(nameof(ToolWindowConfiguration.ShowWithMiniWindow), Convert.ToString(config.ShowWithMiniWindow));
                    writer.WriteAttributeString(nameof(ToolWindowConfiguration.AlwaysOnTop), Convert.ToString(config.AlwaysOnTop));
                    SaveComponent(writer, config.Component);
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
        }

        public static void Save(Stream stream, UIComponentConfiguration config)
        {
            using (var writer = new XmlTextWriter(stream, Encoding.Default))
            {
                writer.Formatting = Formatting.Indented;
                writer.WriteStartDocument();
                writer.WriteStartElement(Publication.Product);
                SaveComponent(writer, config);
                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
        }

        private static void SaveComponent(XmlTextWriter writer, UIComponentConfiguration config)
        {
            writer.WriteStartElement(nameof(UIComponentConfiguration));
            if (config != null)
            {
                Logger.Write(typeof(Serializer), LogLevel.Trace, "Saving component configuration: \"{0}\".", config.Id);
                writer.WriteAttributeString(nameof(UIComponentConfiguration.Id), config.Id);
                writer.WriteAttributeString(nameof(UIComponentConfiguration.Component), config.Component);
                foreach (var child in config.Children)
                {
                    SaveComponent(writer, child);
                }
                foreach (var metaData in config.MetaData)
                {
                    SaveMetaData(writer, metaData);
                }
            }
            writer.WriteEndElement();
        }

        private static void SaveMetaData(XmlTextWriter writer, UIComponentConfiguration.MetaDataEntry metaData)
        {
            Logger.Write(typeof(Serializer), LogLevel.Trace, "Saving meta data: \"{0}\": \"{1}\" = \"{2}\".", metaData.Id, metaData.Name, metaData.Value);
            writer.WriteStartElement(nameof(UIComponentConfiguration.MetaDataEntry));
            writer.WriteAttributeString(nameof(UIComponentConfiguration.MetaDataEntry.Id), metaData.Id);
            writer.WriteAttributeString(nameof(UIComponentConfiguration.MetaDataEntry.Name), metaData.Name);
            writer.WriteAttributeString(nameof(UIComponentConfiguration.MetaDataEntry.Value), metaData.Value);
            writer.WriteEndElement();
        }

        public static IEnumerable<ToolWindowConfiguration> LoadWindows(Stream stream)
        {
            using (var reader = new XmlTextReader(stream))
            {
                reader.WhitespaceHandling = WhitespaceHandling.Significant;
                reader.ReadStartElement(Publication.Product);
                while (reader.IsStartElement(nameof(ToolWindowConfiguration)))
                {
                    var id = reader.GetAttribute(nameof(ToolWindowConfiguration.Id));
                    var title = reader.GetAttribute(nameof(ToolWindowConfiguration.Title));
                    var left = reader.GetAttribute(nameof(ToolWindowConfiguration.Left));
                    var top = reader.GetAttribute(nameof(ToolWindowConfiguration.Top));
                    var width = reader.GetAttribute(nameof(ToolWindowConfiguration.Width));
                    var height = reader.GetAttribute(nameof(ToolWindowConfiguration.Height));
                    var showWithMainWindow = reader.GetAttribute(nameof(ToolWindowConfiguration.ShowWithMainWindow));
                    var showWithMiniWindow = reader.GetAttribute(nameof(ToolWindowConfiguration.ShowWithMiniWindow));
                    var alwaysOnTop = reader.GetAttribute(nameof(ToolWindowConfiguration.AlwaysOnTop));
                    reader.ReadStartElement(nameof(ToolWindowConfiguration));
                    Logger.Write(typeof(Serializer), LogLevel.Trace, "Loading tool window configuration: \"{0}\".", id);
                    yield return new ToolWindowConfiguration(id)
                    {
                        Title = title,
                        Left = Convert.ToInt32(left),
                        Top = Convert.ToInt32(top),
                        Width = Convert.ToInt32(width),
                        Height = Convert.ToInt32(height),
                        ShowWithMainWindow = Convert.ToBoolean(showWithMainWindow),
                        ShowWithMiniWindow = Convert.ToBoolean(showWithMiniWindow),
                        AlwaysOnTop = Convert.ToBoolean(alwaysOnTop),
                        Component = LoadComponent(reader)
                    };
                    if (reader.NodeType == XmlNodeType.EndElement && string.Equals(reader.Name, nameof(ToolWindowConfiguration)))
                    {
                        reader.ReadEndElement();
                    }
                }
                if (reader.NodeType == XmlNodeType.EndElement && string.Equals(reader.Name, Publication.Product))
                {
                    reader.ReadEndElement();
                }
            }
        }

        public static UIComponentConfiguration LoadComponent(Stream stream)
        {
            var component = default(UIComponentConfiguration);
            using (var reader = new XmlTextReader(stream))
            {
                reader.WhitespaceHandling = WhitespaceHandling.Significant;
                reader.ReadStartElement(Publication.Product);
                component = LoadComponent(reader);
                if (reader.NodeType == XmlNodeType.EndElement && string.Equals(reader.Name, Publication.Product))
                {
                    reader.ReadEndElement();
                }
            }
            return component;
        }

        private static UIComponentConfiguration LoadComponent(XmlReader reader)
        {
            if (!reader.IsStartElement(nameof(UIComponentConfiguration)))
            {
                return null;
            }
            var id = reader.GetAttribute(nameof(UIComponentConfiguration.Id));
            var component = reader.GetAttribute(nameof(UIComponentConfiguration.Component));
            var children = new List<UIComponentConfiguration>();
            var metaData = new List<UIComponentConfiguration.MetaDataEntry>();
            var isEmptyElement = reader.IsEmptyElement;
            reader.ReadStartElement(nameof(UIComponentConfiguration));
            Logger.Write(typeof(Serializer), LogLevel.Trace, "Loading component configuration: \"{0}\".", id);
            if (!isEmptyElement)
            {
                while (reader.IsStartElement())
                {
                    if (reader.IsStartElement(nameof(UIComponentConfiguration)))
                    {
                        children.Add(LoadComponent(reader));
                    }
                    else if (reader.IsStartElement(nameof(UIComponentConfiguration.MetaDataEntry)))
                    {
                        metaData.Add(LoadMetaData(reader));
                    }
                    else
                    {
                        Logger.Write(typeof(Serializer), LogLevel.Warn, "Element \"{0}\" was not recognized.", reader.Name);
                        break;
                    }
                }
            }
            if (reader.NodeType == XmlNodeType.EndElement && string.Equals(reader.Name, nameof(UIComponentConfiguration)))
            {
                reader.ReadEndElement();
            }
            return new UIComponentConfiguration(id)
            {
                Component = component,
                Children = new ObservableCollection<UIComponentConfiguration>(children),
                MetaData = new ObservableCollection<UIComponentConfiguration.MetaDataEntry>(metaData)
            };
        }

        private static UIComponentConfiguration.MetaDataEntry LoadMetaData(XmlReader reader)
        {
            if (!reader.IsStartElement(nameof(UIComponentConfiguration.MetaDataEntry)))
            {
                return null;
            }
            var id = reader.GetAttribute(nameof(UIComponentConfiguration.MetaDataEntry.Id));
            var name = reader.GetAttribute(nameof(UIComponentConfiguration.MetaDataEntry.Name));
            var value = reader.GetAttribute(nameof(UIComponentConfiguration.MetaDataEntry.Value));
            reader.ReadStartElement(nameof(UIComponentConfiguration.MetaDataEntry));
            Logger.Write(typeof(Serializer), LogLevel.Trace, "Loading meta data: \"{0}\": \"{1}\" = \"{2}\".", id, name, value);
            if (reader.NodeType == XmlNodeType.EndElement && string.Equals(reader.Name, nameof(UIComponentConfiguration.MetaDataEntry)))
            {
                reader.ReadEndElement();
            }
            return new UIComponentConfiguration.MetaDataEntry(id)
            {
                Name = name,
                Value = value
            };
        }
    }
}
