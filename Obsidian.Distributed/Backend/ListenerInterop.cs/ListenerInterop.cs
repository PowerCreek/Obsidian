using Obsidian.Distributed.Backend.World;
using System.Net.Sockets;
using MessagePack;
using System.Xml.Linq;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System;

namespace Obsidian.Distributed.Backend.ListenerInterop.cs;

public class ListenerInterop<T> where T : ICommandSequence
{
    public TcpListener Listener { get; init; }
    public Action<T> OnElementRead { get; init; }

    public async Task StartListening()
    {
        Listener.Start();

        var client = await Listener.AcceptTcpClientAsync();
        _ = HandleClientAsync(client);
    }

    public virtual async Task HandleClientAsync(TcpClient client)
    {
        var networkStream = client.GetStream();
        await foreach (T item in networkStream.ReadCommandsAsync<T>(client))
            _ = Task.Run(() => OnElementRead?.Invoke(item));
    }
}

public static class NetworkStreamExtensions
{
    public static async IAsyncEnumerable<T> ReadCommandsAsync<T>(this NetworkStream stream, TcpClient client) where T : ICommandSequence
    {
        byte[] data = new byte[Unsafe.SizeOf<T>()];
        
        int bytesRead;

        while (client.Connected && stream.CanRead)
        {
            bytesRead = await stream.ReadAsync(data, 0, data.Length);

            if (bytesRead == 0)
                continue;

            T receivedCommand = data.Deserialize<T>();
            yield return receivedCommand;
        }
    }
}
