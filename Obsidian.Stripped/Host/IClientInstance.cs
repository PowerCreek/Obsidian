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
    public int Id { get; }
}

public record ClientInstance<T>(int Id, ClientStreamInterop ClientStreamInterop, BufferBlock<T> PacketQueue) : IClientInstance;

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
    public ClientInstance<T> ClientInstance(int Id, ClientStreamInterop ClientStreamInterop)
    {
        var packetInit = CreatePacketInit();
        var bufferBlock = packetInit.GetBufferBlock(ClientStreamInterop.Socket, packetInit.DoPacketOperation<T>());
        var instance = new ClientInstance<T>(Id, ClientStreamInterop, bufferBlock);
        DisconnectListener.AddListener(a => Logger.LogError($"Disconnected: {Id}"));
        return instance;
    }
}

public record PacketDataConsumerItem(int Id, byte[] Data);
public class PacketDataConsumer
{
    public AsyncQueue<PacketDataConsumerItem> PacketItemQueue { get; private set; } = new();
}
