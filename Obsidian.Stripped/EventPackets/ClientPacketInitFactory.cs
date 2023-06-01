using Microsoft.Extensions.Logging;
using Obsidian.Stripped.Client;
using Obsidian.Stripped.Host;
using Obsidian.Stripped.Utilities.EventSystem;
using System.Net.Sockets;
using System.Threading.Tasks.Dataflow;

namespace Obsidian.Stripped.EventPackets;

public delegate int PacketAction<T>(T t);

public record ClientPacketInit(
    ILogger<ClientPacketInit> Logger,
    IDispatch<ClientDisconnectArgs> DisconnectFromClient,
    ClientPacketQueue PacketQueue
    )
{
    public static ICompoundService<ClientPacketInit>.RegisterServices Register = services => services
            .With(ClientPacketQueue.Register)
            .With(ClientDisconnectHandler.Register)
            .WithTransient<ClientPacketInit>();

    private ILogger<ClientPacketInit> Logger { get; } = Logger;
    private IDispatch<ClientDisconnectArgs> DisconnectFromClient { get; } = DisconnectFromClient;
    private ClientPacketQueue PacketQueue { get; } = PacketQueue;

    public string UUID = Guid.NewGuid().ToString();

    public BufferBlock<T> GetBufferBlock<T>(Socket socket, PacketAction<T> action)
    {
        return PacketQueue.SetupPacketQueue<T>(data =>
        {
            if (socket.Connected)
            {
                action(data);
            }
        });
    }

    public PacketAction<T> DoPacketOperation<T>() => (t) =>
    {
        Logger.LogInformation("Connected: ClientPacketInit: " + UUID);
        SendPacket(t);
        return 1;
    };

    public void SendPacket<T>(T t)
    {
        try
        {

        }
        catch(Exception ex)
        {

        }
    }
}
