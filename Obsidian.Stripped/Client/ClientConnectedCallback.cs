using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Obsidian.Stripped.EventPackets;
using Obsidian.Stripped.Host;
using Obsidian.Stripped.Utilities;
using Obsidian.Stripped.Utilities.Collections;
using System.Net.Sockets;

namespace Obsidian.Stripped.Client;

public record ClientConnectedCallback(
    ILogger<ClientConnectedCallback> Logger,
    Indexer Indexer,
    AsyncQueueFeed<IClientInstance> ClientCreationFeed,
    Func<ClientPacketInit> CreatePacketInit,
    GetClientInstance<object> GetClientInstance
    ) : IClientConnectedCallback
{
    public static ICompoundService<ClientConnectedCallback>.RegisterServices Register = services => services
            .WithSingleton<Indexer>()
            .With(ClientPacketInit.Register)
            .With(GetClientInstance<object>.Register)
            .WithSingleton<Func<ClientPacketInit>>(s => () => s.GetRequiredService<ClientPacketInit>())
            .WithSingleton<AsyncQueueFeed<IClientInstance>>()
            .WithSingleton<ClientConnectedCallback>();

    private Indexer Indexer { get; } = Indexer;
    private AsyncQueueFeed<IClientInstance>? ClientCreationFeed { get; } = ClientCreationFeed;
    
    private GetClientInstance<object> GetClientInstance { get; } = GetClientInstance;
    public Action<Socket> Callback { get; } = socket =>
    {
        Logger.LogInformation("Socket Info: " + socket.RemoteEndPoint);

        var result = InitializeClient(socket);

        ClientCreationFeed.Enqueue(result);

        IClientInstance InitializeClient(Socket socket)
        {
            var NextId = Indexer.GetAvailableTag();

            var instance = GetClientInstance.ClientInstance(NextId, new ClientStreamInterop(socket));

            Logger.LogInformation("Collection: " + Indexer.GetContents());

            return instance;
        }
    };
}
