using Obsidian.Stripped.Utilities.Collections;
using Shouldly;

namespace Obsidian.Stripped.Tests;

public class AsyncQueueTests
{
    [Fact(DisplayName = "DequeueEmptyUnblocksAfterTimeout")]
    public async Task DequeueUnblocksAfterTimeout()
    {
        var testQueue = new AsyncQueue<int>();

        var result = await testQueue.DequeueAsync(timeout: 100);

        (result is { SucessfulResult: false }).ShouldBeTrue();
    }

    [Fact(DisplayName = "DequeueEmptyUnblocksOnCancelIfEmpty")]
    public async Task DequeueEmptyUnblocksOnCancelIfEmpty()
    {
        var testQueue = new AsyncQueue<int>();

        var result = await testQueue.DequeueAsync(cancelIfEmpty: true);

        (result is { SucessfulResult: false }).ShouldBeTrue();
    }

    [Fact(DisplayName = "DequeueProvidesResultBeforeTimeout")]
    public async Task DequeueProvidesResultBeforeTimeout()
    {
        var testInput = new int[] {1,2};

        var testQueue = new AsyncQueue<int>();
        testQueue.Enqueue(testInput[0]);
        testQueue.Enqueue(testInput[1]);

        var result = await testQueue.DequeueAsync(timeout: 100);

        (result is { SucessfulResult: true }).ShouldBeTrue();
        testInput[0].ShouldBeEquivalentTo(result?.Item);

        result = await testQueue.DequeueAsync(timeout: 100);

        (result is { SucessfulResult: true }).ShouldBeTrue();
        testInput[1].ShouldBeEquivalentTo(result?.Item);

        result = await testQueue.DequeueAsync(timeout: 100);
        result.ShouldBe(AsyncQueue<int>.EmptyResult);
    }

    [Fact(DisplayName = "DequeueSucceedsBeforeTimeout")]
    public async Task DequeueSucceedsBeforeTimeout()
    {
        var testInput = 1;

        var testQueue = new AsyncQueue<int>();
        testQueue.Enqueue(testInput);

        var result = await testQueue.DequeueAsync(timeout: 100);

        (result is { SucessfulResult: true }).ShouldBeTrue();
        testInput.ShouldBeEquivalentTo(result?.Item);
    }

    [Fact(DisplayName = "StringTest")]
    public async Task StringTest()
    {
        var testInput = default(int?);

        var testQueue = new AsyncQueue<int?>();
        testQueue.Enqueue(testInput!);

        var result = await testQueue.DequeueAsync(timeout: 100);

        (result is { SucessfulResult: true }).ShouldBeTrue();
        testInput.ShouldBeEquivalentTo(result?.Item);
    }


    [Fact(DisplayName = "DequeueFromPopulatedQueueAndTimeout")]
    public async Task DequeueFromPopulatedQueueAndTimeout()
    {
        var timeDelay = 50;
        var timeout = timeDelay + 50;
        var testInput = 1;

        var testQueue = new AsyncQueue<int>();

        _ = Task.Delay(timeDelay).ContinueWith(t => testQueue.Enqueue(testInput));

        var result = await testQueue.DequeueAsync(timeout: timeout);

        (result is { SucessfulResult: true }).ShouldBeTrue();
        testInput.ShouldBeEquivalentTo(result?.Item);
    }

    [Fact(DisplayName = "DequeueFromPopulatedQueueAndCancellation")]
    public async Task DequeueFromPopulatedQueueAndCancellation()
    {
        var timeDelay = 50;
        var timeout = timeDelay + 50;
        var testInput = 1;

        var testQueue = new AsyncQueue<int>();

        var cancellationTokenSource = new CancellationTokenSource();

        _ = Task.Delay(timeDelay).ContinueWith(t => testQueue.Enqueue(testInput));

        var result = await testQueue.DequeueAsync(cancellationToken: cancellationTokenSource.Token);

        (result is { SucessfulResult: true }).ShouldBeTrue();
        testInput.ShouldBeEquivalentTo(result?.Item);
    }

    [Fact(DisplayName = "CancelDequeueFromPopulatedQueue")]
    public async Task CancelDequeueFromPopulatedQueue()
    {
        var timeDelay = 50;
        var timeout = timeDelay + 50;
        var testInput = 1;

        var testQueue = new AsyncQueue<int>();

        var cancellationTokenSource = new CancellationTokenSource(1000);

        var result = await testQueue.DequeueAsync(cancellationToken: cancellationTokenSource.Token);

        (result is { Cancelled: true }).ShouldBeTrue();
    }

}
