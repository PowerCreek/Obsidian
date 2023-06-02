//using Microsoft.Extensions.Logging;
//using Obsidian.Stripped.Host;

//namespace Obsidian.Stripped.EventPackets;

//public record ClientInstanceUpdateLoop(
//    ApplicationLifetimeStoppingTokenSource TokenSource,
//    ILogger<ClientInstanceUpdateLoop> Logger,
//    ClientMapType ClientMap
//    )
//{
//    public static ICompoundService<ClientInstanceUpdateLoop>.RegisterServices Register = services => services
//        .WithSingleton(ApplicationLifetimeStoppingTokenSource.AddServiceItem)
//        .WithSingleton<ClientMapType>()
//        .WithSingleton<ClientInstanceUpdateLoop>();

//    private ILogger<ClientInstanceUpdateLoop> Logger { get; } = Logger;
//    private ApplicationLifetimeStoppingTokenSource TokenSource { get; } = TokenSource;
//    private ClientMapType ClientMap { get; } = ClientMap;

//    public async Task LoopInstances()
//    {
//        var gatherClients = default(Task<ClientMapKey[]>);

//        lock(ClientMap)
//            gatherClients = Task.Run(()=>ClientMap.Keys.ToArray());

//        await Parallel.ForEachAsync(await gatherClients, new ParallelOptions(), async (client, token) => 
//        {

//        });
//    }

//    public async void PerformPacketReading(ClientMapValue clientItem, CancellationToken token)
//    {
//        var item = await clientItem.PacketConsumer.PacketItemQueue.DequeueAsync(token: token);
//    }
//}
