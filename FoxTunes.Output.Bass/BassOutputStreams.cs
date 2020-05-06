using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace FoxTunes
{
    public static class BassOutputStreams
    {
        static BassOutputStreams()
        {
            Streams = new ConcurrentDictionary<string, WeakReference<BassOutputStream>>(StringComparer.OrdinalIgnoreCase);
        }

        private static ConcurrentDictionary<string, WeakReference<BassOutputStream>> Streams { get; set; }

        public static IEnumerable<BassOutputStream> Active
        {
            get
            {
                Prune();
                return Streams
                    .Select(pair => pair.Value)
                    .Where(reference => reference.IsAlive)
                    .Select(reference => reference.Target);
            }
        }

        public static bool Contains(string fileName)
        {
            Prune();
            var reference = default(WeakReference<BassOutputStream>);
            return Streams.TryGetValue(fileName, out reference);
        }

        public static bool Add(BassOutputStream stream)
        {
            Prune();
            return Streams.TryAdd(stream.FileName, new WeakReference<BassOutputStream>(stream));
        }

        public static bool Remove(BassOutputStream stream)
        {
            Prune();
            var reference = default(WeakReference<BassOutputStream>);
            return Streams.TryRemove(stream.FileName, out reference);
        }

        public static bool Clear()
        {
            Prune();
            foreach (var stream in Active)
            {
                stream.Dispose();
            }
            return !Active.Any();
        }

        private static void Prune()
        {
            foreach (var pair in Streams)
            {
                if (pair.Value.IsAlive)
                {
                    continue;
                }
                var reference = default(WeakReference<BassOutputStream>);
                Streams.TryRemove(pair.Key, out reference);
            }
        }
    }
}
