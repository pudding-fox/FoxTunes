using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace FoxTunes
{
    public class FileNameMetaDataExtractor : IFileNameMetaDataExtractor
    {
        public FileNameMetaDataExtractor(string pattern)
        {
            this.Pattern = Compile(pattern);
        }

        public Regex Pattern { get; private set; }

        public bool Extract(string value, out IDictionary<string, string> metaData)
        {
            var match = this.Pattern.Match(value);
            if (!match.Success)
            {
                metaData = default(IDictionary<string, string>);
                return false;
            }

            metaData = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (var a = 0; a < match.Groups.Count; a++)
            {
                var group = match.Groups[a];
                var name = this.Pattern.GroupNameFromNumber(a);

                if (name.Length <= 2 || string.IsNullOrEmpty(group.Value))
                {
                    continue;
                }

                metaData.Add(name, group.Value.Trim());
            }

            return true;
        }

        public static Regex Compile(string pattern)
        {
            return new Regex(
                pattern
                    .Replace(@"DIR", Regex.Escape(Path.DirectorySeparatorChar.ToString()))
                    .Replace(@"SEP", @"[\s-]+"),
                RegexOptions.Compiled | RegexOptions.ExplicitCapture
            );
        }
    }
}
