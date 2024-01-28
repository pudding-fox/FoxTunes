using FoxDb;
using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FoxTunes
{
    public static class FileSystemHelper
    {
        const int CACHE_SIZE = 128;

        private static ILogger Logger
        {
            get
            {
                return LogManager.Logger;
            }
        }

        public static HashSet<string> IgnoredNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "FoxTunes.Launcher.exe",
            "FoxTunes.Launcher.x86.exe",
            //TODO: Coupling to some other random component?
            "DataStore",
            "x86",
            "x64",
            //TODO: Translations should be better managed.
            "fr",
            //TODO: Coupling to some other random component?
            "encoders"
        };

        static FileSystemHelper()
        {
            Store = new Cache(CACHE_SIZE);
        }

        public static Cache Store { get; private set; }

        public static IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOption)
        {
            if (searchOption.HasFlag(SearchOption.UseSystemCache))
            {
                //TODO: Warning: Buffering a potentially large sequence.
                return Store.GetOrAdd(path, searchPattern, searchOption, () => EnumerateFilesCore(path, searchPattern, searchOption).ToArray());
            }
            return EnumerateFilesCore(path, searchPattern, searchOption);
        }

        private static IEnumerable<string> EnumerateFilesCore(string path, string searchPattern, SearchOption searchOption)
        {
            var queue = new Queue<string>();
            queue.Enqueue(path);
            while (queue.Count > 0)
            {
                path = queue.Dequeue();
                if (searchOption.HasFlag(SearchOption.Recursive))
                {
                    try
                    {
                        var directoryNames = Directory.EnumerateDirectories(path, "*", global::System.IO.SearchOption.TopDirectoryOnly);
                        if (searchOption.HasFlag(SearchOption.UseSystemExclusions))
                        {
                            directoryNames = WithSystemExclusions(directoryNames);
                        }
                        if (searchOption.HasFlag(SearchOption.Sort))
                        {
                            //The results are already sorted (if using NTFS)
                            //The underlying API is https://docs.microsoft.com/en-gb/windows/win32/api/fileapi/nf-fileapi-findnextfilea
                            //.NET doesn't specify any order though so here we are..
                            directoryNames = directoryNames.OrderBy();
                        }
                        queue.EnqueueRange(directoryNames);
                    }
                    catch
                    {
                        continue;
                    }
                }
                var fileNames = default(IEnumerable<string>);
                try
                {
                    fileNames = Directory.EnumerateFiles(path, searchPattern, global::System.IO.SearchOption.TopDirectoryOnly);
                    if (searchOption.HasFlag(SearchOption.UseSystemExclusions))
                    {
                        fileNames = WithSystemExclusions(fileNames);
                    }
                    if (searchOption.HasFlag(SearchOption.Sort))
                    {
                        //The results are already sorted (if using NTFS)
                        //The underlying API is https://docs.microsoft.com/en-gb/windows/win32/api/fileapi/nf-fileapi-findnextfilea
                        //.NET doesn't specify any order though so here we are..
                        fileNames = fileNames.OrderBy();
                    }
                }
                catch
                {
                    continue;
                }
                foreach (var fileName in fileNames)
                {
                    yield return fileName;
                }
            }
        }

        private static IEnumerable<string> WithSystemExclusions(IEnumerable<string> paths)
        {
            return paths.Where(path =>
            {
                if (IgnoredNames.Contains(Path.GetFileName(path), StringComparer.OrdinalIgnoreCase))
                {
                    return false;
                }
                return true;
            }).ToArray();
        }

        public static bool IsLocalPath(string fileName)
        {
            try
            {
                var uri = default(Uri);
                if (!Uri.TryCreate(fileName, UriKind.Absolute, out uri))
                {
                    return false;
                }
                if (string.Equals(uri.Scheme, Uri.UriSchemeFile, StringComparison.OrdinalIgnoreCase))
                {
                    //Normal path.
                    return true;
                }
                //Some kind of abstraction.
                return false;
            }
            catch (Exception e)
            {
                Logger.Write(typeof(FileSystemHelper), LogLevel.Warn, "Failed to determine whether \"{0}\" is a local path: {1}", fileName, e.Message);
                return false;
            }
        }

        /// <summary>
        /// This routine was based on PathUtility.GetRelativePath: https://source.dot.net/#Microsoft.DotNet.Cli.Utils/PathUtility.cs,
        /// </summary>
        /// <param name="path1"></param>
        /// <param name="path2"></param>
        /// <returns></returns>
        public static string GetRelativePath(string path1, string path2)
        {
            path1 = Path.GetFullPath(path1);
            path2 = Path.GetFullPath(path2);

            if (string.Equals(path1, path2, StringComparison.OrdinalIgnoreCase))
            {
                return path2;
            }

            var index = 0;
            var path1Segments = path1.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            var path2Segments = path2.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            var len1 = path1Segments.Length - 1;
            var len2 = path2Segments.Length;

            var min = Math.Min(len1, len2);
            while (min > index)
            {
                if (!string.Equals(path1Segments[index], path2Segments[index], StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }
                else if ((len1 == index && len2 > index + 1) || (len1 > index && len2 == index + 1))
                {
                    break;
                }
                index++;
            }

            var result = new StringBuilder();
            for (var a = index; len1 > a; a++)
            {
                result.Append("..");
                result.Append(Path.DirectorySeparatorChar);
            }

            for (var a = index; len2 - 1 > a; a++)
            {
                result.Append(path2Segments[a]);
                result.Append(Path.DirectorySeparatorChar);
            }

            if (!string.IsNullOrEmpty(path2Segments[len2 - 1]))
            {
                result.Append(path2Segments[len2 - 1]);
            }

            return result.ToString();
        }

        public class Cache
        {
            public Cache(int capacity)
            {
                this.Store = new CappedDictionary<Key, IEnumerable<string>>(capacity);
            }

            public CappedDictionary<Key, IEnumerable<string>> Store { get; private set; }

            public IEnumerable<string> GetOrAdd(string path, string searchPattern, SearchOption searchOption, Func<IEnumerable<string>> factory)
            {
                var key = new Key(path, searchPattern, searchOption);
                return this.Store.GetOrAdd(key, factory);
            }

            public class Key : IEquatable<Key>
            {
                public Key(string path, string searchPattern, SearchOption searchOption)
                {
                    this.Path = path;
                    this.SearchPattern = searchPattern;
                    this.SearchOption = searchOption;
                }

                public string Path { get; private set; }

                public string SearchPattern { get; private set; }

                public SearchOption SearchOption { get; private set; }

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
                    if (!string.Equals(this.Path, other.Path, StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }
                    if (!string.Equals(this.SearchPattern, other.SearchPattern, StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }
                    if (this.SearchOption != other.SearchOption)
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
                        if (!string.IsNullOrEmpty(this.Path))
                        {
                            hashCode += this.Path.ToLower().GetHashCode();
                        }
                        if (!string.IsNullOrEmpty(this.SearchPattern))
                        {
                            hashCode += this.SearchPattern.ToLower().GetHashCode();
                        }
                        hashCode += this.SearchOption.GetHashCode();
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

        [Flags]
        public enum SearchOption : byte
        {
            None = 0,
            Recursive = 1,
            UseSystemExclusions = 2,
            UseSystemCache = 4,
            Sort = 8
        }
    }
}
