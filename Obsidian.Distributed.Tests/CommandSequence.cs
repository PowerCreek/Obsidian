using Obsidian.Distributed.Backend.World;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Shouldly;

namespace Obsidian.Distributed.Tests;

public class CommandSequenceTest
{
    [Fact(DisplayName = "HandleClientAsync")]
    public async Task HandleClientAsync_ShouldInvokeOnElementRead()
    {
        int port = 12443;

        var onDataReadInvoked = 0;
        var command = new ServerWorldCommand { WorldValue = 42 };

        var mre = new ManualResetEvent(false);

        var listenerInterop = new ServerWorldInterop()
        {
            Listener = new TcpListener(new IPEndPoint(IPAddress.Any, port)),
            OnElementRead = (c) =>
            {
                onDataReadInvoked++;
                Debug.WriteLine($"Found {c.WorldValue}");
                if(onDataReadInvoked > 4)
                    mre.Set();
            }
        };

        _ = listenerInterop.StartListening();

        CancellationTokenSource src = new CancellationTokenSource();

        src.CancelAfter(4000);

        await Task.Run(async () =>
        {
            using (var tcpClient = new TcpClient())
            {
                await tcpClient.ConnectAsync(IPAddress.Loopback, port);
                var networkStream = tcpClient.GetStream();
                try
                {
                    for (int i = 0; i < 20; i++)
                    {
                        await Task.Delay(6000 / 10);
                        var serializedCommand = new ServerWorldCommand { WorldValue = new Random().Next() % 100 }.Serialize();
                    
                        await networkStream.WriteAsync(serializedCommand, 0, serializedCommand.Length, src.Token);
                    }
                }
                catch (TaskCanceledException ex)
                {
                    
                }
            }
        });

        mre.WaitOne();
        // Assert
        onDataReadInvoked.ShouldBeGreaterThan(4);
    }
}
