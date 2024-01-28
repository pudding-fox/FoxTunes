using NUnit.Framework;
using System.Threading.Tasks;

namespace FoxTunes
{
    [TestFixture]
    public class ReentrantTaskTests
    {
        [Test]
        public async Task SingletonReentrantTaskRespectsPriority()
        {
            var counter1 = 0;
            var counter2 = 0;
            var id = "5C8A060E-885C-4FF1-ABEB-DB046D2D8D1C";
            var task1 = new SingletonReentrantTask(id, 10, SingletonReentrantTask.PRIORITY_LOW, async cancellationToken =>
            {
                for (; counter1 + counter2 < 10; counter1++)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }
                    await Task.Delay(100);
                }
            });
            var task2 = new SingletonReentrantTask(id, 10, SingletonReentrantTask.PRIORITY_HIGH, async cancellationToken =>
            {
                for (; counter1 + counter2 < 10; counter2++)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }
                    await Task.Delay(100);
                }
            });
            await Task.WhenAll(task1.Run(), task2.Run());
            Assert.AreEqual(10, counter1 + counter2);
            Assert.Less(counter1, counter2);
        }
    }
}
