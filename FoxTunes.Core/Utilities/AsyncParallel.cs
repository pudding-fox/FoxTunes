using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes
{
    public static class AsyncParallel
    {
        public static Task For(int start, int count, Func<int, Task> factory, CancellationToken cancellationToken, ParallelOptions options)
        {
            return ForEach<int>(Enumerable.Range(start, count), factory, cancellationToken, options);
        }

        public static async Task ForEach<T>(IEnumerable<T> sequence, Func<T, Task> factory, CancellationToken cancellationToken, ParallelOptions options)
        {
            var exceptions = new List<Exception>();
            var tasks = new List<Task>(options.MaxDegreeOfParallelism);
            foreach (var element in sequence)
            {
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        await factory(element);
                    }
                    catch (Exception e)
                    {
                        exceptions.Add(e);
                    }
                }));
                if (exceptions.Count > 0)
                {
                    break;
                }
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
            if (exceptions.Any())
            {
                throw new AggregateException(exceptions);
            }
        }
    }
}
