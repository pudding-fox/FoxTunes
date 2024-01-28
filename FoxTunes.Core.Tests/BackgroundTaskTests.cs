using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoxTunes
{
    [TestFixture]
    public class BackgroundTaskTests : TestBase
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
        public void BackgroundTaskRespectsConcurrency()
        {
            var tasks = Enumerable.Range(0, 1024).Select(index => Task.Run(async () =>
            {
                var task = new Task001();
                task.InitializeComponent(this.Core);
                await task.Run();
            })).ToArray();
            Task.WaitAll(tasks);
            Assert.AreEqual(1024, Task001.Counter);
        }

        public class Task001 : BackgroundTask
        {
            public const string ID = "9353EE88-67FC-4BB7-ADFA-FFA195AAAE91";

            public static volatile int Counter = 0;

            public Task001() : base(ID)
            {
            }

            protected override Task OnRun()
            {
                Counter++;
                return Task.CompletedTask;
            }
        }
    }
}
