using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Obsidian.Stripped.Client;
using Obsidian.Stripped.Host;
using Obsidian.Stripped.Utilities.Collections;
using System.Collections.Concurrent;

namespace Obsidian.Stripped.EventPackets;


/// <summary>
/// Needs to gather the connection collection
/// </summary>
public record EventPacketFeed(
    AsyncQueueFeed<ClientInstance> ClientCreationFeed,
    ConcurrentDictionary<int, ClientStreamInterop> ClientMap,
    ILogger<EventPacketFeed> Logger
    ) : IHostedService, IDisposable
{
    public static ICompoundService<EventPacketFeed>.RegisterServices Register = services =>
        services.WithSingleton<AsyncQueueFeed<ClientInstance>>()
        .WithSingleton<ConcurrentDictionary<int, ClientStreamInterop>>()
        .WithHostedService<EventPacketFeed>();

    private AsyncQueueFeed<ClientInstance> ClientCreationFeed { get; } = ClientCreationFeed;
    private  ConcurrentDictionary<int, ClientStreamInterop> ClientMap {get;} = ClientMap;

    public void Dispose() { }

    public async Task StartAsync(CancellationToken cancellationToken) 
    {

        await foreach(var client in ClientCreationFeed.ConsumeFeedAsync()){
            Logger.LogInformation($"Connected {client.Id}:");
        }

        await Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
    }
}
