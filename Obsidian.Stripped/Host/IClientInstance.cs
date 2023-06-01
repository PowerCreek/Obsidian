using Obsidian.Stripped.Client;
using Obsidian.Stripped.EventPackets;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks.Dataflow;

namespace Obsidian.Stripped.Host;

public interface IClientInstance 
{
    public int Id { get; }
}

public record ClientInstance<T>(int Id, ClientStreamInterop ClientStreamInterop, BufferBlock<T> PacketQueue) : IClientInstance;

public record GetClientInstance<T>(
    Func<ClientPacketInit> CreatePacketInit
)
{
    public static ICompoundService<GetClientInstance<T>>.RegisterServices Register = services => services
            .With(ClientPacketInit.Register)
            .WithSingleton<Func<ClientPacketInit>>(s => () => s.GetRequiredService<ClientPacketInit>())
            .WithSingleton<GetClientInstance<T>>();

    private Func<ClientPacketInit> CreatePacketInit { get; } = CreatePacketInit;
    public ClientInstance<T> ClientInstance(int Id, ClientStreamInterop ClientStreamInterop)
    {
        var packetInit = CreatePacketInit();
        var bufferBlock = packetInit.GetBufferBlock(ClientStreamInterop.Socket, packetInit.DoPacketOperation<T>());
        var instance = new ClientInstance<T>(Id, ClientStreamInterop, bufferBlock);

        return instance;
    }
}
