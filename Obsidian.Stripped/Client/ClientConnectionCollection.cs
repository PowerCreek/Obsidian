using Obsidian.Stripped.Host;
using Obsidian.Stripped.Utilities;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading.Tasks.Dataflow;

namespace Obsidian.Stripped.Client;

public class ClientConnectionCollection
{
    public Indexer ClientIdIndexer = new Indexer();
    public ConcurrentDictionary<int, ClientInstance> ClientInstanceMap { get; init; } = new();

    //TODO
    //add action in the input properties here.
    public record ClientInstanceCreationInput(Socket Socket, ClientPacketQueue PacketQueue)
    {
        public ClientStreamInterop ClientStreamInterop => new(Socket);
        public BufferBlock<T> GetBufferBlock<T>(Action<T> action) => PacketQueue.SetupPacketQueue<T>(action);
    }

    public ClientInstance CreateClientInstance(ClientInstanceCreationInput input)
    {
        var clientId = ClientIdIndexer.GetAvailableTag();

        var block = input.GetBufferBlock<object>(data =>
        {
            if (input.Socket.Connected)
            {

            }
        });

        var instance = new ClientInstance(Id: clientId, ClientStreamInterop: input.ClientStreamInterop, block);

        ClientInstanceMap.TryAdd(clientId, instance);

        return instance;
    }
}
