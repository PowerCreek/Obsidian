using Microsoft.Extensions.Logging;
using Obsidian.Stripped.Host;
using Obsidian.Stripped.Utilities.Collections;
using System.Net.Sockets;
using Obsidian.Net;

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
            AddClient(instance!);
        }
        await Task.CompletedTask;
    }
    /// <summary>
    /// Move the code to ClientInstanceUpdateLoop class for processing.
    /// </summary>
    /// <param name="instance"></param>
    public async void AddClient(IClientInstance instance)
    {
        var packetDataConsumer = new PacketDataConsumer();
        ClientMapValue clientValue = (instance, packetDataConsumer);

        ClientMap.TryAdd(instance.UUID, clientValue);
        Logger.LogInformation($"Connected {instance.UUID}:");
        WaitForPackets(packetDataConsumer);
        ProcessPackets(clientValue);
        await Task.CompletedTask;
    }

    public async void ProcessPackets(ClientMapValue clientValue)
    {
        var interop = clientValue.Instance.ClientStreamInterop;
        var minecraftServer = interop.MinecraftStream;


    }

    public async void WaitForPackets(PacketDataConsumer packetConsumer)
    {
        const int PACKET_WAIT_TIMEOUT = 1000;

        var tokenSource = new CancellationTokenSource();
        var manualReset = new ManualResetEventSlim(false);

        var waitTask = Task.Run(() =>
        {
            manualReset.Wait(PACKET_WAIT_TIMEOUT);
            tokenSource.Cancel();
        });

        var timer = new Timer(_ => manualReset.Set(), null, PACKET_WAIT_TIMEOUT, Timeout.Infinite);

        while (!tokenSource.IsCancellationRequested)
        {
            timer.Change(PACKET_WAIT_TIMEOUT, Timeout.Infinite);
            var packetData = await packetConsumer.PacketItemQueue.DequeueAsync(token: tokenSource.Token);
            CheckPacketResult(packetData);
        }

        async void CheckPacketResult(AsyncQueue<PacketDataConsumerItem>.AsyncDequeueResult packetData)
        {            
            if (packetData is not { Cancelled: false, Item: var Item })
            {
                Logger.LogError("Exited due to inactivity");
                return;
            }

            Logger.LogInformation($"Test: {Item.UUID}");
        }
    }
}

public record PacketDataConsumerItem(string UUID, byte[] Data);
public class PacketDataConsumer
{
    public AsyncQueue<PacketDataConsumerItem> PacketItemQueue { get; private set; } = new();
}
