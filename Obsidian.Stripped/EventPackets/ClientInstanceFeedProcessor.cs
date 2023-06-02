using Microsoft.Extensions.Logging;
using Obsidian.Stripped.Host;
using Obsidian.Stripped.Utilities.Collections;

using ClientMapKey = string;
using ClientMapValue = (Obsidian.Stripped.Host.IClientInstance, Obsidian.Stripped.EventPackets.PacketDataConsumer);
using ClientMapType = System.Collections.Concurrent.ConcurrentDictionary<string, (Obsidian.Stripped.Host.IClientInstance, Obsidian.Stripped.EventPackets.PacketDataConsumer)>;
namespace Obsidian.Stripped.EventPackets;

public record ClientInstanceFeedProcessor(
    ApplicationLifetimeStoppingTokenSource TokenSource,
    ILogger<ClientInstanceFeedProcessor> Logger,
    ClientMapType ClientMap,
    AsyncQueueFeed<IClientInstance> ClientCreationFeed
    )
{
    public static ICompoundService<ClientInstanceFeedProcessor>.RegisterServices Register = services => services
        .WithSingleton(ApplicationLifetimeStoppingTokenSource.AddServiceItem)
        .WithSingleton<AsyncQueueFeed<IClientInstance>>()
        .WithSingleton<ClientMapType>()
        .WithSingleton<ClientInstanceFeedProcessor>();

    private ApplicationLifetimeStoppingTokenSource TokenSource { get; } = TokenSource;
    private ILogger<ClientInstanceFeedProcessor> Logger { get; } = Logger;
    private ClientMapType ClientMap { get; } = ClientMap;
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
        ClientMap.TryAdd(instance.UUID, (instance, new PacketDataConsumer()));
        Logger.LogInformation($"Connected {instance.UUID}:");
        await Task.CompletedTask;
    }
}

public record PacketDataConsumerItem(int Id, byte[] Data);
public class PacketDataConsumer
{
    public AsyncQueue<PacketDataConsumerItem> PacketItemQueue { get; private set; } = new();
}
