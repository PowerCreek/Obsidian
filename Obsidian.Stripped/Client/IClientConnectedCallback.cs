using System.Net.Sockets;

namespace Obsidian.Stripped.Client;

public interface IClientConnectedCallback
{
    public Action<Socket> Callback { get; }
}
