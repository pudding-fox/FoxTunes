using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FoxTunes
{
    public class TagLibFileAbstraction : global::TagLib.File.IFileAbstraction
    {
        public TagLibFileAbstraction(IFileAbstraction fileAbstraction)
        {
            this.FileAbstraction = fileAbstraction;
        }

        public IFileAbstraction FileAbstraction { get; private set; }

        public string Name
        {
            get
            {
                return this.FileAbstraction.FileName;
            }
        }

        public Stream ReadStream
        {
            get
            {
                return this.FileAbstraction.ReadStream;
            }
        }

        public Stream WriteStream
        {
            get
            {
                return this.FileAbstraction.WriteStream;
            }
        }

        public void CloseStream(Stream stream)
        {
            this.FileAbstraction.CloseStream(stream);
        }

        public static TagLibFileAbstraction Create(IFileAbstraction fileAbstraction)
        {
            return new TagLibFileAbstraction(fileAbstraction);
        }
    }
}
