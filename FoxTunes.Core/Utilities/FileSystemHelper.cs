using FoxDb;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FoxTunes
{
    public static class FileSystemHelper
    {
        const int CACHE_SIZE = 128;

        public static HashSet<string> IgnoredDirectories = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            //TODO: Coupling to some other random component?
            FileMetaDataStore.DataStoreDirectoryName
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
                var paths = default(Lazy<IEnumerable<string>>);
                if (Store.TryGetValue(path, searchPattern, searchOption, out paths))
                {
                    return paths.Value;
                }
                Store.Add(
                    path,
                    searchPattern,
                    searchOption,
                    new Lazy<IEnumerable<string>>(
                        //TODO: Warning: Buffering a potentially large sequence.
                        () => EnumerateFilesCore(path, searchPattern, searchOption).ToArray()
                    )
                );
                //Second iteration will always hit cache.
                return EnumerateFiles(path, searchPattern, searchOption);
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
                            directoryNames = directoryNames.Except(IgnoredDirectories, StringComparer.OrdinalIgnoreCase);
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
                var fileNames = new List<string>();
                try
                {
                    fileNames.AddRange(
                        Directory.EnumerateFiles(path, searchPattern, global::System.IO.SearchOption.TopDirectoryOnly)
                    );
                    if (searchOption.HasFlag(SearchOption.Sort))
                    {
                        //The results are already sorted (if using NTFS)
                        //The underlying API is https://docs.microsoft.com/en-gb/windows/win32/api/fileapi/nf-fileapi-findnextfilea
                        //.NET doesn't specify any order though so here we are..
                        fileNames.Sort();
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

        public class Cache
        {
            public Cache(int capacity)
            {
                this.Store = new CappedDictionary<Key, Lazy<IEnumerable<string>>>(capacity);
            }

            public CappedDictionary<Key, Lazy<IEnumerable<string>>> Store { get; private set; }

            public void Add(string path, string searchPattern, SearchOption searchOption, Lazy<IEnumerable<string>> paths)
            {
                var key = new Key(path, searchPattern, searchOption);
                this.Store.Add(key, paths);
            }

            public bool TryGetValue(string path, string searchPattern, SearchOption searchOption, out Lazy<IEnumerable<string>> paths)
            {
                var key = new Key(path, searchPattern, searchOption);
                return this.Store.TryGetValue(key, out paths);
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
