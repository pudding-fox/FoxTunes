using FoxDb;
using System.Collections.Generic;
using System.IO;

namespace FoxTunes
{
    public static class FileSystemHelper
    {
        public static IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOption)
        {
            var stack = new Stack<string>();
            stack.Push(path);
            while (stack.Count > 0)
            {
                path = stack.Pop();
                if (searchOption == SearchOption.AllDirectories)
                {
                    try
                    {
                        stack.PushRange(Directory.EnumerateDirectories(path, "*", SearchOption.TopDirectoryOnly));
                    }
                    catch
                    {
                        continue;
                    }
                }
                var fileNames = new List<string>();
                try
                {
                    fileNames.AddRange(Directory.EnumerateFiles(path, searchPattern, SearchOption.TopDirectoryOnly));
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
    }
}
