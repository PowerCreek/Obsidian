using Microsoft.Extensions.Logging;
using Obsidian.Stripped.Host;
using Obsidian.Stripped.Utilities;
using Obsidian.Stripped.Utilities.Collections;
using System.Net.Sockets;

namespace Obsidian.Stripped.Client;

public record ClientConnectedCallback(
    ILogger<ClientConnectedCallback> Logger,
    Indexer Indexer,
    AsyncQueueFeed<IClientInstance> ClientCreationFeed,
    GetClientInstance<object> GetClientInstance
    ) : IClientConnectedCallback
{
    public static ICompoundService<ClientConnectedCallback>.RegisterServices Register = services => services
            .WithSingleton<Indexer>()
            .With(GetClientInstance<object>.Register)
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

            var instance = GetClientInstance.ClientInstance(Guid.NewGuid().ToString(), new ClientStreamInterop(socket));

            Logger.LogInformation("Collection: " + Indexer.GetContents());

            return instance;
        }
    };
}
