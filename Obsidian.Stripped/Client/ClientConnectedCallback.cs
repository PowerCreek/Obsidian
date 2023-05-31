using Microsoft.Extensions.Logging;
using Obsidian.Stripped.Host;
using System.Net.Sockets;

namespace Obsidian.Stripped.Client;

public record ClientConnectedCallback(
    ILogger<ClientConnectedCallback> logger,
    ClientConnectionCollection collection,
    ClientPacketFactory factory) : IClientConnectedCallback
{
    public static ICompoundService<ClientConnectedCallback>.RegisterServices Register = services => services
            .WithSingleton<ClientConnectionCollection>()
            .WithSingleton<ClientPacketFactory>()
            .WithSingleton<ClientConnectedCallback>();

    private ClientConnectionCollection? collection { get; } = collection;
    private ClientPacketFactory? factory { get; }
    public Action<Socket> Callback { get; } = socket =>
    {
        logger.LogInformation("Test");

        try
        {
            collection.CreateClientInstance(new
            (
                    Socket: socket,
                    PacketQueue: factory.CreateQueueInstance()
            ));

            logger.LogInformation("Collection: " + collection.ClientIdIndexer.GetContents());
        } catch (Exception e)
        {
            logger.LogError(e.ToString());
        }
    };
}
