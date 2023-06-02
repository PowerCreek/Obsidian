using Microsoft.Extensions.Logging;
using Obsidian.Stripped.Host;
using System.Collections.Concurrent;

using ClientMapKey = string;
using ClientMapValue = (Obsidian.Stripped.Host.IClientInstance ClientInstance, 
    Obsidian.Stripped.EventPackets.PacketDataConsumer PacketConsumer);
using ClientMapType = System.Collections.Concurrent.ConcurrentDictionary<string, (Obsidian.Stripped.Host.IClientInstance, Obsidian.Stripped.EventPackets.PacketDataConsumer)>;
using System.Threading;


namespace Obsidian.Stripped.EventPackets;

public record ClientInstanceUpdateLoop(
    ApplicationLifetimeStoppingTokenSource TokenSource,
    ILogger<ClientInstanceUpdateLoop> Logger,
    ClientMapType ClientMap
    )
{
    public static ICompoundService<ClientInstanceUpdateLoop>.RegisterServices Register = services => services
        .WithSingleton(ApplicationLifetimeStoppingTokenSource.AddServiceItem)
        .WithSingleton<ClientMapType>()
        .WithSingleton<ClientInstanceUpdateLoop>();

    private ILogger<ClientInstanceUpdateLoop> Logger { get; } = Logger;
    private ApplicationLifetimeStoppingTokenSource TokenSource { get; } = TokenSource;
    private ClientMapType ClientMap { get; } = ClientMap;

    public async Task LoopInstances()
    {
        Logger.LogInformation(string.Join(Environment.NewLine, new string[10].Select(e=>"TEST").ToArray()));

        var gatherClients = default(Task<ClientMapKey[]>);

        lock(ClientMap)
            gatherClients = Task.Run(()=>ClientMap.Keys.ToArray());

        await Parallel.ForEachAsync(await gatherClients, new ParallelOptions(), async (client, token) => 
        {

        });
    }

    public async void PerformPacketReading(ClientMapValue clientItem, CancellationToken token)
    {
        var item = await clientItem.PacketConsumer.PacketItemQueue.DequeueAsync(cancellationToken: token);
    }
}
