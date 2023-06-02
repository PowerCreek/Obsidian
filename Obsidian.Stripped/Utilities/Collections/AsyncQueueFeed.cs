using Microsoft.CodeAnalysis;
using Obsidian.Stripped.Host;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;


namespace Obsidian.Stripped.Utilities.Collections;

public class AsyncQueue<T>
{
    public record struct AsyncDequeuResult(bool SucessfulResult, bool Cancelled, T? Item);
    public static readonly AsyncDequeuResult EmptyResult = new(false, false, default);
    public static readonly AsyncDequeuResult CancelledResult = new(false, true, default);

    private ConcurrentQueue<T?> Queue = new ();
    private SemaphoreSlim _semaphore = new(0);
    private ManualResetEventSlim _resetEvent = new(false);

    public void Enqueue(T t)
    {
        Queue.Enqueue(t);
        _semaphore.Release();
        _resetEvent.Set();
    }

    public async Task<AsyncDequeuResult?> DequeueAsync(bool cancelIfEmpty = false, int? timeout = null, CancellationToken cancellationToken = default)
    {
        var cancellationTask = Task.Run(() =>
        {
            _ = (timeout) switch
            {
                int time => cancellationToken.WaitHandle.WaitOne(time),
                _=> cancellationToken.WaitHandle.WaitOne()
            };

            cancellationToken.ThrowIfCancellationRequested();
        });

        var enumerator = ConsumeFeedAsync(cancelIfEmpty).GetAsyncEnumerator();
        var dequeueResult = enumerator.MoveNextAsync().AsTask();

        await Task.WhenAny(dequeueResult, cancellationTask);

        switch((
            Cancellation: cancellationTask.IsFaulted,
            DequeueResult: dequeueResult.IsCompletedSuccessfully,
            Value: dequeueResult.IsCompleted? (bool?)await dequeueResult : default
            ))
        {
            case { Cancellation: true }:
                return CancelledResult;
            case { DequeueResult: true, Value: true }:
                return new(true, false, enumerator.Current);
            default:
                return EmptyResult;
        };
    }

    private async IAsyncEnumerable<T?> ConsumeFeedAsync(bool cancelIfEmpty)
    {
        await foreach ((bool Succeeded, T? Item) in FeedAsync())
        {
            switch((Succeeded,cancelIfEmpty))
            {
                case { Succeeded: true }:
                    yield return Item;
                        OnItemConsumed();
                    break;
                case { cancelIfEmpty: false}:
                    break;
                default:
                    yield break;
            }            
        }
    }
    
    private async IAsyncEnumerable<(bool Succeeded,T?)> FeedAsync()
    {
        while (true)
        {
            while (Queue.TryDequeue(out var instance))
                yield return (true, instance);
            yield return default;
            _resetEvent.Reset();
            await _semaphore.WaitAsync();
        }
    }

    private void OnItemConsumed()
    {
        _resetEvent.Wait();
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

    public async IAsyncEnumerable<T?> ConsumeFeedAsync()
    {
        await foreach (var item in FeedAsync())
        {
            yield return item;
            OnItemConsumed();
        }
    }

    private async IAsyncEnumerable<T?> FeedAsync()
    {
        while (true)
        {
            while (Queue.TryDequeue(out var instance))
                yield return instance;
            _resetEvent.Reset();
            await _semaphore.WaitAsync();
        }
    }

    private void OnItemConsumed()
    {
        _resetEvent.Wait();
    }
}
