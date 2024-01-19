namespace LdapClient.Common.Helpers;

/// <summary>
///     Facilitates processing multiple concurrent asynchronous operations returning the same type
/// </summary>
/// <remarks>
///     See article by Stefan Schranz; June 20th, 2022:
///     https://itnext.io/merging-concurrent-iasyncenumerable-t-operations-for-increased-performance-d8393005c6ae
/// </remarks>
/// <typeparam name="T">Type of object being returned</typeparam>
public class MergedAsyncEnumerable<T> : IAsyncEnumerable<T>
{
    private readonly IAsyncEnumerable<T>[] _asyncEnumerable;

    /// <summary>
    ///     Receives any number of IAsyncEnumerable tasks
    /// </summary>
    /// <param name="asyncEnumerable">Array of tasks to cycle through</param>
    public MergedAsyncEnumerable(params IAsyncEnumerable<T>[] asyncEnumerable)
    {
        _asyncEnumerable = asyncEnumerable;
    }

    /// <summary>
    ///     Required for implementation of IAsyncEnumerable
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns>Merged enumerator for all running tasks</returns>
    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        return ConsumeMergedAsyncEnumerable().GetAsyncEnumerator(cancellationToken);
    }

    /// <summary>
    ///     Cycle through each of the running tasks without blocking, allowing them to return as they have results
    /// </summary>
    /// <returns>Objects returned by task</returns>
    private async IAsyncEnumerable<T> ConsumeMergedAsyncEnumerable()
    {
        var iterators = _asyncEnumerable
            .Select((x, index) => new IndexedIterator(x, index))
            .ToArray();

        var tasks = new List<Task<IndexedIteratorResult>?>();
        tasks.AddRange(iterators
            .Select(x => x.MoveAhead())
            .ToList());

        while (tasks.Any(x => x is not null))
        {
            var winningTask = await Task.WhenAny(tasks.Where(x => x is not null).Select(x => x!));
            var (item, hasMore, index) = winningTask.Result;
            if (!hasMore)
            {
                tasks[index] = null;
                continue;
            }

            yield return item;
            tasks[index] = iterators[index].MoveAhead();
        }
    }

    private record IndexedIteratorResult(T Item, bool HasMore, int Index);

    private class IndexedIterator(IAsyncEnumerable<T> asyncEnumerable, int index)
    {
        private readonly IAsyncEnumerator<T> _asyncEnumerator = asyncEnumerable.GetAsyncEnumerator();

        public async Task<IndexedIteratorResult> MoveAhead()
        {
            var hasMoreEntries = await _asyncEnumerator.MoveNextAsync();
            return new IndexedIteratorResult(_asyncEnumerator.Current, hasMoreEntries, index);
        }
    }
}