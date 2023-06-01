using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Obsidian.Stripped.Host;
using Obsidian.Stripped.Utilities.Collections;
using System.Collections.Concurrent;

namespace Obsidian.Stripped.EventPackets;


/// <summary>
/// Needs to gather the connection collection
/// </summary>
public record ClientWorker(
    AsyncQueueFeed<IClientInstance> ClientCreationFeed,
    ConcurrentDictionary<int, IClientInstance> ClientMap,
    ILogger<ClientWorker> Logger
    ) : IHostedService, IDisposable
{
    public static ICompoundService<ClientWorker>.RegisterServices Register = services =>
        services.WithSingleton<AsyncQueueFeed<IClientInstance>>()
        .WithSingleton<ConcurrentDictionary<int, IClientInstance>>()
        .WithHostedService<ClientWorker>();

    private AsyncQueueFeed<IClientInstance> ClientCreationFeed { get; } = ClientCreationFeed;
    private ConcurrentDictionary<int, IClientInstance> ClientMap { get; } = ClientMap;

    public void Dispose() { }

    public async Task StartAsync(CancellationToken cancellationToken)
    {

        await foreach (var client in ClientCreationFeed.ConsumeFeedAsync())
        {
            Logger.LogInformation($"Connected {client.Id}:");
        }

        await Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
    }
}
