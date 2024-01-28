﻿using NUnit.Framework;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes
{
    [TestFixture]
    public class BackgroundTaskTests : TestBase
    {
        [Test]
        public void BackgroundTaskRespectsConcurrency()
        {
            var tasks = Enumerable.Range(0, 1024).Select(index => Task.Run(async () =>
            {
                using (var task = new Task001())
                {
                    task.InitializeComponent(this.Core);
                    await task.Run();
                }
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