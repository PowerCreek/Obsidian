using Obsidian.Concurrency;
using Obsidian.Stripped.Utilities.Collections;
using SharpNoise.Modules;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Obsidian.Stripped.Tests;
public class AsyncQueueFeedTests
{
    [Fact(DisplayName = "DequeueEmptyUnblocksAfterTimeout")]
    public async Task DequeueUnblocksAfterTimeout()
    {
        var testQueue = new AsyncQueueFeed<int>();

        var tokenSource = new CancellationTokenSource(100);
        try
        {
            await foreach (var item in testQueue.ConsumeFeedAsync().WithCancellation(tokenSource.Token))
            {

            }
        }
        catch (Exception ex)
        {
            ex.ShouldBeOfType<OperationCanceledException>();
        }
    }

    [Fact(DisplayName = "FeedProducesItemsWhenComplete")]
    public async Task FeedProducesItemsWhenComplete()
    {
        var testQueue = new AsyncQueueFeed<int>();

        var items = new ConcurrentHashSet<int>(Enumerable.Range(0, 10));

        _ = Task.Delay(100).ContinueWith(async a =>
        {
            foreach(var v in items)
            {
                testQueue.Enqueue(v);
                //await Task.Delay(100);
            }
        });

        string values = "";

        var tasks = new Task[2].Select((a,i) =>
            Task.Run(async () =>
            {
                await foreach (var item in testQueue.ConsumeFeedAsync())
                {
                    values += ($"Iterator: {i}, Item: {item} \n");
                    items.TryRemove(item);
                    if (items.IsEmpty) break;
                }
            }));

        await Task.WhenAny(tasks);

        Assert.Empty(items);
    }

}
