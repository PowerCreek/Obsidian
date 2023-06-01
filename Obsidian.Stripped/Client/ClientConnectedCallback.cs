using Microsoft.Extensions.Logging;
using Obsidian.Stripped.Host;
using Obsidian.Stripped.Utilities.Collections;
using System.Net.Sockets;

namespace Obsidian.Stripped.Client;

public record ClientConnectedCallback(
    ILogger<ClientConnectedCallback> Logger,
    ClientConnectionCollection ClientCollection,
    ClientPacketFactory Factory,
    AsyncQueueFeed<IClientInstance> ClientCreationFeed) : IClientConnectedCallback
{
    public static ICompoundService<ClientConnectedCallback>.RegisterServices Register = services => services
            .With(ClientConnectionCollection.Register)
            .WithSingleton<ClientPacketFactory>()
            .WithSingleton<AsyncQueueFeed<IClientInstance>>()
            .WithSingleton<ClientConnectedCallback>();

    private ClientConnectionCollection? ClientCollection { get; } = ClientCollection;
    private AsyncQueueFeed<IClientInstance>? ClientCreationFeed { get; } = ClientCreationFeed;
    private ClientPacketFactory? Factory { get; }
    public Action<Socket> Callback { get; } = socket =>
    {
        Logger.LogInformation("Socket Info: " + socket.RemoteEndPoint);
        var result = ClientCollection.CreateClientInstance(new
        (
                Socket: socket,
                PacketQueue: Factory.CreateQueueInstance()
        ));

        Logger.LogInformation("Collection: " + ClientCollection.ClientIdIndexer.GetContents());

        ClientCreationFeed.Enqueue(result);
    };
}
