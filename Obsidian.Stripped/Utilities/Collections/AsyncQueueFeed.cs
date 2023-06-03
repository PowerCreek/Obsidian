using Obsidian.API;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;


namespace Obsidian.Stripped.Utilities.Collections;
public delegate Task<T?> QueueProcessorAsync<T>(bool cancelIfEmpty = false, int? timeout = null, CancellationToken token = default);
public class AsyncQueue<T>
{
    public record struct AsyncDequeueResult(bool SuccessfulResult, bool Cancelled, T? Item);
    public static readonly AsyncDequeueResult EmptyResult = new(false, false, default);
    public static readonly AsyncDequeueResult CancelledResult = new(false, true, default);

    private ConcurrentQueue<T?> Queue = new();
    private SemaphoreSlim _waitTask = new(0);
    public void Enqueue(T t)
    {
        Queue.Enqueue(t);
        _waitTask.Release();
    }

    public async Task<AsyncDequeueResult> DequeueAsync(bool cancelIfEmpty = false, CancellationToken token = default)
    {
        var dequeueResult = default(AsyncDequeueResult?);

        try
        {
            token.ThrowIfCancellationRequested();

            while ((dequeueResult = (Success: Queue.TryDequeue(out var result), cancelIfEmpty) switch
            {
                { Success: true } => new(SuccessfulResult: true, false, result),
                { cancelIfEmpty: true } => EmptyResult,
                _ => null
            }) is null)
            {
                await _waitTask.WaitAsync(token);
                token.ThrowIfCancellationRequested();
            }
        }
        catch (OperationCanceledException)
        {
            return CancelledResult;
        }

        return dequeueResult.Value;
    }
}

public class AsyncQueueFeed<T>
{
    private ConcurrentQueue<T?> Queue = new();
    private SemaphoreSlim _semaphore = new(0);
    private ManualResetEventSlim _resetEvent = new(false);

    public void Enqueue(T t)
    {
        Queue.Enqueue(t);
        _semaphore.Release();
        _resetEvent.Set();
    }

    public async IAsyncEnumerable<T?> ConsumeFeedAsync([EnumeratorCancellation] CancellationToken token = default)
    {
        await foreach (var item in FeedAsync().WithCancellation(token))
        {
            token.ThrowIfCancellationRequested();

            yield return item;

            OnItemConsumed();
        }
    }

    private async IAsyncEnumerable<T?> FeedAsync([EnumeratorCancellation] CancellationToken token = default)
    {
        while (true)
        {
            while (Queue.TryDequeue(out var instance))
                yield return instance;

            _resetEvent.Reset();
            
            await _semaphore.WaitAsync(token);
            
            token.ThrowIfCancellationRequested();
        }
    }

    private void OnItemConsumed()
    {
        _resetEvent.Wait();
    }
}
