using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using TagLib;

namespace FoxTunes
{
    public static class DocumentManager
    {
        const string MIME_TYPE_JSON = "application/json";

        static readonly Regex Base64 = new Regex("^([a-z0-9+/]{4})*([a-z0-9+/]{3}=|[a-z0-9+/]{2}==)?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static ILogger Logger
        {
            get
            {
                return LogManager.Logger;
            }
        }

        public static void Read(TagLibMetaDataSource source, IList<MetaDataItem> metaDatas, File file)
        {
            try
            {
                var pictures = file.Tag.Pictures;
                foreach (var picture in pictures)
                {
                    if (string.IsNullOrEmpty(picture.Description))
                    {
                        //We need the desciption for the meta data name.
                        continue;
                    }
                    try
                    {
                        if (string.Equals(picture.MimeType, MIME_TYPE_JSON, StringComparison.OrdinalIgnoreCase))
                        {
                            metaDatas.Add(new MetaDataItem(picture.Description, MetaDataItemType.Document)
                            {
                                Value = string.Format("{0}:{1}", MIME_TYPE_JSON, ReadJsonDocument(picture.Data.Data))
                            });
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Write(typeof(DocumentManager), LogLevel.Warn, "Failed to read document: {0} => {1} => {2}", file.Name, picture.Description, e.Message);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Write(typeof(DocumentManager), LogLevel.Warn, "Failed to read documents: {0} => {1}", file.Name, e.Message);
            }
        }

        private static string ReadJsonDocument(byte[] data)
        {
            var text = Encoding.UTF8.GetString(data);
            if (Base64.IsMatch(text))
            {
                text = Encoding.UTF8.GetString(Convert.FromBase64String(text));
            }
            return text;
        }

        public static void Write(TagLibMetaDataSource source, MetaDataItem metaDataItem, File file)
        {
            //Not yet implemented.
        }
    }
}
