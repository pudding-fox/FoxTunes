using System;
using System.Collections.Generic;
using TagLib;

namespace FoxTunes
{
    public class FileSystemManager
    {
        public static void Read(TagLibMetaDataSource source, IList<MetaDataItem> metaData, File file)
        {
            var info = new global::System.IO.FileInfo(file.Name);
            if (!info.Exists)
            {
                return;
            }
            source.AddTag(metaData, FileSystemProperties.FileName, info.Name);
            source.AddTag(metaData, FileSystemProperties.DirectoryName, info.DirectoryName);
            source.AddTag(metaData, FileSystemProperties.FileExtension, info.Extension);
            source.AddTag(metaData, FileSystemProperties.FileSize, Convert.ToString(info.Length));
            source.AddTag(metaData, FileSystemProperties.FileCreationTime, DateTimeHelper.ToString(info.CreationTime));
            source.AddTag(metaData, FileSystemProperties.FileModificationTime, DateTimeHelper.ToString(info.LastWriteTime));
        }

        public static void Write(TagLibMetaDataSource source, MetaDataItem metaDataItem, File file)
        {
            //Nothing to do.
        }
    }
}
