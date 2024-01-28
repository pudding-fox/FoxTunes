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
            await Task.Delay(TimeSpan.FromSeconds(10));
            Assert.AreEqual(1, count);
        }
    }
}
