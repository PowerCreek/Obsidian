using Obsidian.Net;
using System.Net.Sockets;

namespace Obsidian.Stripped.Client;
public record ClientStreamInterop
{
    private NetworkStream _stream { get; }
    private MinecraftStream _minecraftStream { get; }
    public Socket Socket { get; }
    
    public ClientStreamInterop(Socket socket)
    {
        _stream = new (socket);
        _minecraftStream = new MinecraftStream(_stream);
        Socket = socket;
    }
}
