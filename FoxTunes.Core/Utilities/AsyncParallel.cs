using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes
{
    public static class AsyncParallel
    {
        public static async Task ForEach<T>(IEnumerable<T> sequence, Func<T, Task> factory, CancellationToken cancellationToken, ParallelOptions options)
        {
            var tasks = new List<Task>(options.MaxDegreeOfParallelism);
            foreach (var element in sequence.Select(element => Task.Run(() => factory(element))))
            {
                tasks.Add(element);
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                if (tasks.Count == tasks.Capacity)
                {
                    await Task.WhenAny(tasks);
                    tasks.RemoveAll(task => task.IsCompleted);
                }
            }
            await Task.WhenAll(tasks);
        }
    }
}
