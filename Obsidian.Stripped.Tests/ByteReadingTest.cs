using Obsidian.Net;
using Shouldly;
using Obsidian.Utilities;
using System.Net.Sockets;
using System.Net;
using Obsidian.Stripped.EventPackets.Channels;

namespace Obsidian.Stripped.Tests;
public class ByteReadingTest
{
    [Fact(DisplayName = "VarIntWriteMatchesMinecraftStream")]
    public async Task WriteVarInt()
    {
        const int dataSize = 1000;
        byte[] dataA = new byte[dataSize];
        var stream = new MinecraftStream(dataA);

        for (int i = 0; i < 10; i++)
            await stream.WriteVarIntAsync(i * 3000000);

        stream.Seek(0, SeekOrigin.Begin);

        int[] dataCheckA = new int[10];
        for (int i = 0; i < 10; i++)
        {
            dataCheckA[i] = await stream.ReadVarIntAsync();
        }

        var dataB = new byte[dataSize];
        using var memoryStream = new MemoryStream(dataB);
        for (int i = 0; i < 10; i++)
        {
            var data = DataHandling.ConvertVarIntAsync(i * 3000000).ToArray();
            await memoryStream.WriteAsync(data);
        }

        dataA.ShouldBeEquivalentTo(dataB);
    }

    [Fact(DisplayName = "VarIntReadMatchesMinecraftStream")]
    public async Task ReadVarInt()
    {
        const int dataSize = 1000;
        byte[] dataA = new byte[dataSize];
        var stream = new MinecraftStream(dataA);

        for (int i = 0; i < 10; i++)
            await stream.WriteVarIntAsync(i * 3000000 + 400);

        stream.Seek(0, SeekOrigin.Begin);

        int dataCheckA = -1;
        for (int i = 0; i < 10; i++)
        {
            dataCheckA = await stream.ReadVarIntAsync();
            break;
        }

        int dataCheckB = -1;

        for (int i = 0; i < 10; i++)
        {
            dataCheckB = (await DataHandling.ReadVarIntAsync(dataA.AsMemory())).Value;
            break;
        }

        dataCheckA.ShouldBeEquivalentTo(dataCheckB);
    }

    [Fact(DisplayName = "TestMatchingPacketRead")]
    public async Task TestMatchingPacketRead()
    {
        var SourceData = new byte[]
        {
            0,
            250,
            5,
            9,
            49,
            50,
            55,
            46,
            48,
            46,
            48,
            46,
            49,
            48,
            156,
            1
        };
        var usedData = new byte[SourceData.Length];
        Array.Copy(SourceData, usedData, usedData.Length);

        var listener = new TcpListener(IPAddress.Loopback, 12444);
        listener.Start();
        var socket = await listener.AcceptSocketAsync();
        var stream = new MinecraftStream(new NetworkStream(socket));

        var minecraftStream = new MinecraftStream(stream);

        async Task<(int Id, byte[] ReceivedData, byte[] PacketData)> DoOperation()
        {
            var length = await minecraftStream.ReadVarIntAsync();
            var receivedData = new byte[length];

            _ = await minecraftStream.ReadAsync(receivedData.AsMemory(0, length));

            var packetId = 0;
            var packetData = Array.Empty<byte>();

            using (var packetStream = new MinecraftStream(receivedData))
            {
                try
                {
                    packetId = await packetStream.ReadVarIntAsync();
                    var arlen = 0;

                    if (length - packetId.GetVarIntLength() > -1)
                        arlen = length - packetId.GetVarIntLength();

                    packetData = new byte[arlen];
                    _ = await packetStream.ReadAsync(packetData.AsMemory(0, packetData.Length));

                    return (packetId, receivedData, packetData);
                }
                catch
                {
                    throw;
                }
            }
        }

        var resultToMatch = await DoOperation();

        var compareResult = await DataHandling.SlicePacketSegment(resultToMatch.ReceivedData);

        var resultA = resultToMatch;
        var resultB = compareResult;

        resultA.Id.ShouldBeEquivalentTo(resultB.Item1);
        resultA.PacketData.ShouldBeEquivalentTo(resultB.Item2.ToArray());
    }
}
