using Obsidian.Stripped.Client;
using Obsidian.Stripped.EventPackets;
using System.Threading.Tasks.Dataflow;

namespace Obsidian.Stripped.Host;

public interface IClientInstance 
{
    public static IClientInstance CreateClientInstance<T>(int Id, ClientStreamInterop ClientStreamInterop, BufferBlock<T> PacketQueue)
    => new ClientInstance<T>(Id, ClientStreamInterop, PacketQueue);

    public int Id { get; }
}

public record ClientInstance<T>(int Id, ClientStreamInterop ClientStreamInterop, BufferBlock<T> PacketQueue) : IClientInstance;
