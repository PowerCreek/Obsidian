using Obsidian.Net;
using System.Net.Sockets;

namespace Obsidian.Stripped.Client;
public record ClientStreamInterop
{
    private NetworkStream _stream { get; }
    public MinecraftStream MinecraftStream { get; }
    public Socket Socket { get; }
    
    public ClientStreamInterop(Socket socket)
    {
        _stream = new (socket);
        MinecraftStream = new MinecraftStream(_stream);
        Socket = socket;
    }
}
