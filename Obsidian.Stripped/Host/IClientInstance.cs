using Obsidian.Stripped.Client;
using Obsidian.Stripped.EventPackets;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks.Dataflow;
using Obsidian.Stripped.Utilities.Collections;
using Obsidian.Stripped.Utilities.EventSystem;
using Microsoft.Extensions.Logging;

namespace Obsidian.Stripped.Host;

public interface IClientInstance 
{
    public string UUID { get; }
    public ClientStreamInterop ClientStreamInterop { get; }
}

public record ClientInstance<T>(string UUID, ClientStreamInterop ClientStreamInterop, BufferBlock<T> PacketQueue) : IClientInstance;

public record GetClientInstance<T>(
    Func<ClientPacketInit> CreatePacketInit,
    INotify<ClientDisconnectArgs> DisconnectListener,
    ILogger<GetClientInstance<T>> Logger
)
{
    public static ICompoundService<GetClientInstance<T>>.RegisterServices Register = services => services
            .With(ClientDisconnectHandler.Register)
            .With(ClientPacketInit.Register)
            .WithSingleton<Func<ClientPacketInit>>(s => () => s.GetRequiredService<ClientPacketInit>())
            .WithSingleton<GetClientInstance<T>>();

    private Func<ClientPacketInit> CreatePacketInit { get; } = CreatePacketInit;
    private ILogger<GetClientInstance<T>> Logger { get; } = Logger;
    private INotify<ClientDisconnectArgs> DisconnectListener { get; } = DisconnectListener;
    public ClientInstance<T> ClientInstance(string UUID, ClientStreamInterop ClientStreamInterop)
    {
        var packetInit = CreatePacketInit();
        var bufferBlock = packetInit.GetBufferBlock(ClientStreamInterop.Socket, packetInit.DoPacketOperation<T>());
        var instance = new ClientInstance<T>(UUID, ClientStreamInterop, bufferBlock);
        DisconnectListener.AddListener(a => Logger.LogError($"Disconnected: {UUID}"));
        return instance;
    }
}

