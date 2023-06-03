using Microsoft.Extensions.Logging;
using Obsidian.Net;
using Obsidian.Stripped.Host;
using System.Threading.Channels;
using Obsidian.Stripped.EventPackets.Channels;
using SharpNoise.Modules;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Sockets;
using Obsidian.Net.Packets;

namespace Obsidian.Stripped.Interop;
public class PacketProcessor(
    ILogger<PacketProcessor> Logger,
    StateActions StateActions
    )
{
    public static ICompoundService<PacketProcessor>.RegisterServices Register = services => services
        .WithSingleton<Func<ClientMapValue, PacketProcessor>>(s => c => s.GetRequiredService<PacketProcessor>().CreateProcessor(c))
        .WithTransient<StateActions>()
        .WithTransient<PacketProcessor>();

    public ClientMapValue Client { get; set; } = new ClientMapValue();

    public PacketProcessor CreateProcessor(ClientMapValue value)
    {
        Init(value);
        return this;
    }

    public Socket Socket { get; set; }

    public StateActions StateActions { get; private set; } = StateActions;

    public async void Init(ClientMapValue value)
    {
        StateActions = StateActions.Init(this);
        Client = value;
        var channelOperator = value.Channel;
        channelOperator.SetupChannel(t => OnRead(t));
        channelOperator.Processor.StartReaderAsync();

        var interop = value.Instance.ClientStreamInterop;
        Socket = interop.Socket;

        var minecraftStream = interop.MinecraftStream;
        var rawPacketDataChannel = Channel.CreateUnbounded<(int Id, Memory<byte>)>();

        ReadPackets(rawPacketDataChannel);
        Execute(minecraftStream);
    }

    public async void Execute(MinecraftStream stream)
    {
        var memoryChannel = Channel.CreateUnbounded<Memory<byte>>();
        ReadMemory(memoryChannel.Reader);

        GatherPacketBytes(memoryChannel.Writer);
    }

    public async void ReadMemory(ChannelReader<Memory<byte>> memoryChannel)
    {
        await foreach (var data in memoryChannel.ReadAllAsync())
        {
            (int Id, Memory<byte> Data) = await DataHandling.SlicePacketSegment(data);

            //do something here
            StateLoop(Id, Data);

            Logger.LogInformation(string.Join(",", Data.ToArray().Prepend((byte)Id)));
        };
    }

    public async void ReadPackets(ChannelReader<(int Id, Memory<byte> Data)> rawPacketChannel)
    {
        await foreach ((var Id, var Data) in rawPacketChannel.ReadAllAsync())
        {
            Logger.LogInformation(string.Join(",", Data.ToArray().Prepend((byte)Id)));
        };
    }

    public async void GatherPacketBytes(ChannelWriter<Memory<byte>> writer)
    {
        //Deal with packet timeout
        try
        {
            using var netStream = new NetworkStream(Socket);
            await foreach (var bytes in ProducePacketBytes(Socket, netStream))
            {
                _ = writer.WriteAsync(bytes);
            };
        }
        catch (Exception ex)
        {
            Logger.LogError(ex.ToString());
        }
    }

    public async IAsyncEnumerable<Memory<byte>> ProducePacketBytes(Socket socket, Stream stream)
    {
        var byteIntake = new byte[1];
        while (await socket.ReceiveAsync(byteIntake = new byte[1]) is int count and > 0)
        {
            var packetSize = await stream.GetBytesWithStartAsync(byteIntake[0])
                .ReadVarIntFromAsyncEnumerable();

            var receivedData = new byte[packetSize].AsMemory();

            var len = 0;

            len = await stream.ReadAsync(receivedData);

            if (len != 0 && len == packetSize)
                yield return receivedData;
        }
    }

    public async void OnRead<T>(T t)
    {
        Logger.LogInformation($"Read Client Data: {t}");
    }

    public ClientState State { get; set; } = ClientState.Handshaking;


    public async void StateLoop(int id, Memory<byte> data)
    {
        (await GetStateAction(id))?.Invoke(data);

        async Task<Action<Memory<byte>>> GetStateAction(int packetId)
        {
            return (State, id) switch
            {
                { State: ClientState.Status, id: 0x00 } =>
                    a =>
                    {
                        Logger.LogWarning(nameof(ClientState.Status));
                    }
                ,
                { State: ClientState.Status, id: 0x01 } =>
                    a =>
                    {
                        Logger.LogWarning(nameof(ClientState.Status));
                    }
                ,
                { State: ClientState.Handshaking, id: 0x00 } =>
                    data =>
                    {
                        Logger.LogWarning(nameof(ClientState.Handshaking));
                        StateActions.OnHandshake0x00(data);
                    }
                ,
                { State: ClientState.Handshaking } =>
                    a =>
                    {
                        Logger.LogWarning(nameof(ClientState.Handshaking));
                    }
                ,
                { State: ClientState.Login, id: 0x00 } =>
                    a =>
                    {
                        Logger.LogWarning($"{nameof(ClientState.Login)} 0x00");
                        StateActions.OnLogin0x00(data);
                    }
                ,
                { State: ClientState.Login, id: 0x01 } =>
                    a =>
                    {
                        Logger.LogWarning($"{nameof(ClientState.Login)} 0x01");
                        StateActions.OnLogin0x00(data);
                    }
                ,
                { State: ClientState.Login, id: 0x02 } =>
                    a =>
                    {
                        Logger.LogWarning($"{nameof(ClientState.Login)} 0x02");
                        StateActions.OnLogin0x00(data);
                    }
                ,
                { State: ClientState.Play } =>
                    a =>
                    {
                        Logger.LogWarning(nameof(ClientState.Play));
                    }
                ,
                { State: ClientState.Closed } =>
                    a =>
                    {
                        Logger.LogWarning(nameof(ClientState.Closed));
                    }
                ,
                _ => default!
            };
        }
    }
}

public static class MinecraftStreamExt
{
    public static async IAsyncEnumerable<byte> GetBytesWithStartAsync(this Stream stream, byte startValue)
    {
        yield return startValue;
        while (true)
        {
            var result = stream.ReadByte();
            if (result is -1)
                yield break;
            yield return (byte)stream.ReadByte();
        }
    }

    public static async Task<int> ReadVarIntFromAsyncEnumerable(this IAsyncEnumerable<byte> bytes)
    {
        int numRead = 0;
        int result = 0;
        await foreach (var read in bytes)
        {
            int value = read & 0b01111111;
            result |= value << (7 * numRead);
            numRead++;
            if (numRead > 5)
                throw new InvalidOperationException("VarInt is too big");
            if ((read & 0b10000000) == 0)
                break;
        };

        return result;
    }

    public static async Task<int> ReadVarIntCheckAsync(this Stream stream)
    {
        int numRead = 0;
        int result = 0;
        byte read;
        do
        {
            read = await stream.ReadUnsignedByteCheckAsync();

            int value = read & 0b01111111;
            result |= value << (7 * numRead);

            numRead++;
            if (numRead > 5)
            {
                throw new InvalidOperationException("VarInt is too big");
            }
        } while ((read & 0b10000000) != 0);

        return result;
    }

    public static async Task<byte> ReadUnsignedByteCheckAsync(this Stream stream)
    {
        var buffer = new byte[1];
        await stream.ReadExactlyAsync(buffer);
        return buffer[0];
    }

}

