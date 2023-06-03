using Obsidian.Net;
using Obsidian.Utilities;

namespace Obsidian.Stripped.EventPackets.Channels;

public static class DataHandling
{
    public static async Task<(int, Memory<byte>)> SlicePacketSegment(Memory<byte> data)
    {
        using var stream = new MinecraftStream(data.ToArray());
        var id = await stream.ReadVarIntAsync();
        var size = id.GetVarIntLength();
        if(data.IsEmpty) 
            return (id, Memory<byte>.Empty);
        var remaining = data.Slice(size);

        return (id, remaining);
    }


    public static Task<(int NumRead, int Value)> ReadVarIntAsync(Memory<byte> data)
    {
        int numRead = 0;
        int result = 0;
        byte read;
        int dataIndex = 0;

        do
        {
            read = data.Span[dataIndex];
            int value = read & 0b01111111;
            result |= value << (7 * numRead);

            numRead++;
            if (numRead > 5)
            {
                return Task.FromException<(int NumRead, int Value)>(new InvalidOperationException("VarInt is too big"));
            }
            dataIndex++;
        } while ((read & 0b10000000) != 0);

        return Task.FromResult((numRead, result));
    }

    public static IEnumerable<byte> ConvertVarIntAsync(int value)
    {
        var unsigned = (uint)value;

        if (unsigned is 0)
        {
            yield return (byte)unsigned;
            yield break;
        }

        for (; unsigned != 0; unsigned >>= 7)
        {
            var temp = (byte)(unsigned & 127);
            temp |= (byte)(unsigned > 127 ? 128 : 0);

            yield return temp;
        }
    }
}

