using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FoxTunes
{
    [TestFixture]
    public class PendingQueueTests
    {
        public ParallelOptions ParallelOptions
        {
            get
            {
                return new ParallelOptions()
                {
                    MaxDegreeOfParallelism = Environment.ProcessorCount
                };
            }
        }

        [Test]
        public async Task CanPendValues()
        {
            var count = default(int);
            var queue = new PendingQueue<int>(TimeSpan.FromSeconds(1));
            queue.Complete += (sender, e) =>
            {
                Assert.AreEqual(100, queue.Count());
                Interlocked.Increment(ref count);
            };
            Parallel.For(0, 100, this.ParallelOptions, index => queue.Enqueue(index));
#if NET40
            await TaskEx.Delay(TimeSpan.FromSeconds(10)).ConfigureAwait(false);
#else
            await Task.Delay(TimeSpan.FromSeconds(10)).ConfigureAwait(false);
#endif
            Assert.AreEqual(1, count);
        }
    }
}
