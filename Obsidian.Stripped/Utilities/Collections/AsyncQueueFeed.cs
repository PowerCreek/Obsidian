using System.Collections.Concurrent;
using System.Runtime.CompilerServices;


namespace Obsidian.Stripped.Utilities.Collections;
public delegate Task<T?> QueueProcessorAsync<T>(bool cancelIfEmpty = false, int? timeout = null, CancellationToken token = default);
public class AsyncQueue<T>
{
    public record struct AsyncDequeueResult(bool SucessfulResult, bool Cancelled, T? Item);
    public static readonly AsyncDequeueResult EmptyResult = new(false, false, default);
    public static readonly AsyncDequeueResult CancelledResult = new(false, true, default);

    private ConcurrentQueue<T?> Queue = new();
    private SemaphoreSlim _semaphore = new(0);
    private ManualResetEventSlim _resetEvent = new(false);

    public void Enqueue(T t)
    {
        Queue.Enqueue(t);
        _semaphore.Release();
        _resetEvent.Set();
    }

    public async Task<AsyncDequeueResult> DequeueAsync(bool cancelIfEmpty = false, CancellationToken token = default)
    {
        try
        {
            token.ThrowIfCancellationRequested();

            await foreach (var item in ConsumeFeedAsync(cancelIfEmpty).WithCancellation(token))
            {
                return new(SucessfulResult: true, false, item);
            }
        }
        catch (OperationCanceledException)
        {
            return CancelledResult;
        }

        return EmptyResult;
    }

    private async IAsyncEnumerable<T?> ConsumeFeedAsync(bool cancelIfEmpty = false, [EnumeratorCancellation] CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();

        await foreach ((bool Succeeded, T? Item) in FeedAsync().WithCancellation(token))
        {
            switch ((Succeeded, cancelIfEmpty))
            {
                case { Succeeded: true }:
                    yield return Item;

                    OnItemConsumed(token);
                    break;

                case { cancelIfEmpty: false }:
                    break;

                default:
                    yield break;
            }
            token.ThrowIfCancellationRequested();
        }

        token.ThrowIfCancellationRequested();
    }

    private async IAsyncEnumerable<(bool Succeeded, T?)> FeedAsync([EnumeratorCancellation] CancellationToken token = default)
    {
        while (!token.IsCancellationRequested)
        {
            do
            {
                if(!Queue.TryDequeue(out var instance))
                    break;
                
                yield return (true, instance);

            } while (!token.IsCancellationRequested);

            yield return default;

            _resetEvent.Reset();

            await _semaphore.WaitAsync(token);
        }
    }

    private void OnItemConsumed(CancellationToken token)
    {
        _resetEvent.Wait(token);
    }
}

public class AsyncQueueFeed<T>
{
    private ConcurrentQueue<T?> Queue = new();
    private SemaphoreSlim _semaphore = new(0);

    public void Enqueue(T t)
    {
        Queue.Enqueue(t);
        _semaphore.Release();
    }

    public async IAsyncEnumerable<T?> ConsumeFeedAsync([EnumeratorCancellation] CancellationToken token = default)
    {
        await foreach (var item in FeedAsync().WithCancellation(token))
        {
            token.ThrowIfCancellationRequested();

            yield return item;
        }
    }

    private async IAsyncEnumerable<T?> FeedAsync([EnumeratorCancellation] CancellationToken token = default)
    {
        while (true)
        {
            while (Queue.TryDequeue(out var instance))
                yield return instance;
            
            await _semaphore.WaitAsync(token);
            
            token.ThrowIfCancellationRequested();
        }
    }
}
