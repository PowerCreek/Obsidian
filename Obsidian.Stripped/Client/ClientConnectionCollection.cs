using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Obsidian.Stripped.EventPackets;
using Obsidian.Stripped.Host;
using Obsidian.Stripped.Utilities;
using Obsidian.Stripped.Utilities.Collections;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading.Tasks.Dataflow;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Obsidian.Stripped.Client;

public record ClientConnectionCollection(
    ILogger<ClientConnectionCollection> Logger,
    ClientPacketInitFactory PacketInitFactory
    )
{
    public static ICompoundService<ClientConnectionCollection>.RegisterServices Register = services => services
            .With(ClientPacketInitFactory.Register)
            .WithSingleton<ClientConnectionCollection>();

    private ILogger<ClientConnectionCollection> Logger { get; } = Logger;
    private ClientPacketInitFactory PacketInitFactory { get; } = PacketInitFactory;

    public Indexer ClientIdIndexer = new Indexer();

    public record ClientInstanceCreationInput(Socket Socket, ClientPacketQueue PacketQueue)
    {
        public ClientStreamInterop ClientStreamInterop => new(Socket);
        public BufferBlock<T> GetBufferBlock<T>(PacketAction<T> action)
        {
            return PacketQueue.SetupPacketQueue<T>(data =>
            {
                if (Socket.Connected)
                {
                    action(data);
                }
            });
        } 
    }

    public IClientInstance CreateClientInstance(ClientInstanceCreationInput input)
    {
        var clientId = ClientIdIndexer.GetAvailableTag();

        var block = input.GetBufferBlock(PacketInitFactory.PerformPacketSend<object>());

        var instance = IClientInstance.CreateClientInstance(Id: clientId, ClientStreamInterop: input.ClientStreamInterop, block);

        return instance;
    }
}
