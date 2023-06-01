using Microsoft.Extensions.Logging;
using Obsidian.Stripped.Host;
using Obsidian.Stripped.Utilities.Collections;
using System.Collections.Concurrent;

namespace Obsidian.Stripped.EventPackets;

public record ClientInstanceFeedProcessor(
    ApplicationLifetimeStoppingTokenSource TokenSource,
    ILogger<ClientInstanceFeedProcessor> Logger,
    ConcurrentDictionary<int, IClientInstance> ClientMap,
    AsyncQueueFeed<IClientInstance> ClientCreationFeed
    )
{
    public static ICompoundService<ClientInstanceFeedProcessor>.RegisterServices Register = services => services
        .WithSingleton(ApplicationLifetimeStoppingTokenSource.AddServiceItem)
        .WithSingleton<AsyncQueueFeed<IClientInstance>>()
        .WithSingleton<ConcurrentDictionary<int, IClientInstance>>()
        .WithSingleton<ClientInstanceFeedProcessor>();

    private ApplicationLifetimeStoppingTokenSource TokenSource { get; } = TokenSource;
    private ILogger<ClientInstanceFeedProcessor> Logger { get; } = Logger;
    private ConcurrentDictionary<int, IClientInstance> ClientMap { get; } = ClientMap;
    private AsyncQueueFeed<IClientInstance> ClientCreationFeed { get; } = ClientCreationFeed;

    public async Task LoopFeed()
    {
        var feed = ClientCreationFeed.ConsumeFeedAsync().WithCancellation(TokenSource.Token);

        await foreach (var instance in feed)
        {
            AddClient(instance);
        }
        await Task.CompletedTask;
    }

    public async void AddClient(IClientInstance instance)
    {
        ClientMap.TryAdd(instance.Id, instance);
        Logger.LogInformation($"Connected {instance.Id}:");
        await Task.CompletedTask;
    }
}
