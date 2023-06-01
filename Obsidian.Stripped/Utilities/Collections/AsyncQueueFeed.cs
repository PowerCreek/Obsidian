using Obsidian.Stripped.Host;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;


namespace Obsidian.Stripped.Utilities.Collections;
public record struct AsyncDequeuResult<T>(bool SucessfulResult, T? Item);

public class AsyncQueue<T>
{
    private ConcurrentQueue<T> Queue = new ();
    private SemaphoreSlim _semaphore = new(0);
    private ManualResetEventSlim _resetEvent = new(false);

    public void Enqueue(T t)
    {
        Queue.Enqueue(t);
        _semaphore.Release();
        _resetEvent.Set();
    }

    public async Task<AsyncDequeuResult<T>> DequeueAsync()
    {
        var enumerator = ConsumeFeedAsync().GetAsyncEnumerator();

        return (await enumerator.MoveNextAsync()) switch
        {
            true => new (true, enumerator.Current),
            false => default
        };
    }

    private async IAsyncEnumerable<T> ConsumeFeedAsync()
    {
        await foreach (var item in FeedAsync())
        {
            yield return item;
            OnItemConsumed();
        }
    }

    private async IAsyncEnumerable<T> FeedAsync()
    {
        while (true)
        {
            await _semaphore.WaitAsync();

            while (Queue.TryDequeue(out var instance))
                yield return instance;

            _resetEvent.Reset();
        }
    }

    private void OnItemConsumed()
    {
        _resetEvent.Wait();
    }
}

public class AsyncQueueFeed<T>
{
    private ConcurrentQueue<T> Queue = new();
    private SemaphoreSlim _semaphore = new(0);
    private ManualResetEventSlim _resetEvent = new(false);

    public void Enqueue(T t)
    {
        Queue.Enqueue(t);
        _semaphore.Release();
        _resetEvent.Set();
    }

    public async IAsyncEnumerable<T> ConsumeFeedAsync()
    {
        await foreach (var item in FeedAsync())
        {
            yield return item;
            OnItemConsumed();
        }
    }

    private async IAsyncEnumerable<T> FeedAsync()
    {
        while (true)
        {
            await _semaphore.WaitAsync();

            while (Queue.TryDequeue(out var instance))
                yield return instance;

            _resetEvent.Reset();
        }
    }

    private void OnItemConsumed()
    {
        _resetEvent.Wait();
    }
}
