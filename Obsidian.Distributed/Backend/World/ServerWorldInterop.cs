using Obsidian.Distributed.Backend.ListenerInterop.cs;
using System.Net.Sockets;

namespace Obsidian.Distributed.Backend.World;

public class ServerWorldInterop : ListenerInterop<ServerWorldCommand>
{
    public override async Task HandleClientAsync(TcpClient client)
    {
        await base.HandleClientAsync(client);
    }
}
