using Microsoft.Extensions.Logging;
using Obsidian.Stripped.Host;
using Obsidian.Stripped.Utilities.Collections;
using System.Net.Sockets;
using Obsidian.Net;
using Obsidian.Stripped.EventPackets.Channels;
using Microsoft.Extensions.DependencyInjection;
using Obsidian.Stripped.Interop;

namespace Obsidian.Stripped.EventPackets;

public record ClientInstanceFeedProcessor(
    ApplicationLifetimeStoppingTokenSource TokenSource,
    ILogger<ClientInstanceFeedProcessor> Logger,
    ClientMapType ClientMap,
    AsyncQueueFeed<IClientInstance> ClientCreationFeed,
    Func<ChannelOperatorImpl> ChannelOperatorFactory,
    Func<ClientMapValue, PacketProcessor> PacketProcessorFatcory
    )
{
    public static ICompoundService<ClientInstanceFeedProcessor>.RegisterServices Register = services => services
        .With(PacketProcessor.Register)
        .AddChannelOperator(typeof(ChannelOperatorImpl))
        .WithSingleton(ApplicationLifetimeStoppingTokenSource.AddServiceItem)
        .WithSingleton<AsyncQueueFeed<IClientInstance>>()
        .WithSingleton<ClientMapType>()
        .WithSingleton<Func<ChannelOperatorImpl>>(s => () => s.GetRequiredService<ChannelOperatorImpl>())
        .WithSingleton<ClientInstanceFeedProcessor>();

    private ApplicationLifetimeStoppingTokenSource TokenSource { get; } = TokenSource;
    private ILogger<ClientInstanceFeedProcessor> Logger { get; } = Logger;
    private ClientMapType ClientMap { get; } = ClientMap;
    private AsyncQueueFeed<IClientInstance> ClientCreationFeed { get; } = ClientCreationFeed;
    private Func<ChannelOperatorImpl> ChannelOperatorFactory { get; } = ChannelOperatorFactory;
    private Func<ClientMapValue, PacketProcessor> PacketProcessorFactory { get; } = PacketProcessorFatcory;

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
        var channelOperator = ChannelOperatorFactory();
        var clientValue = (instance, channelOperator);

        ClientMap.TryAdd(instance.UUID, clientValue);
        Logger.LogInformation($"Connected {instance.UUID}:");

        PacketProcessorFactory(clientValue);
        await Task.CompletedTask;
    }
}
