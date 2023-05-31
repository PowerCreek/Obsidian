using Microsoft.Extensions.Hosting;
using Obsidian.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Obsidian.Stripped.Client;
public class ClientStreamInterop
{
    private NetworkStream _stream;
    private MinecraftStream _minecraftStream;

    public ClientStreamInterop(Socket socket)
    {
        _stream = new (socket);
        _minecraftStream = new MinecraftStream(_stream);
    }
}
