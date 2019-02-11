using Newtonsoft.Json;
using Obsidian.Entities;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

//https://wiki.vg/Protocol#Data_types
public static class DataReader
{
    public static async Task<sbyte> ReadByteAsync(this Stream stream) => (sbyte)(await stream.ReadUnsignedByteAsync());

    public static async Task<byte> ReadUnsignedByteAsync(this Stream stream)
    {
        var buffer = new byte[1];
        await stream.ReadAsync(buffer);
        return buffer[0];
    }

    public static async Task<bool> ReadBooleanAsync(this Stream stream)
    {
        var value = (int)await stream.ReadByteAsync();
        if (value == 0x00)
        {
            return false;
        }
        else if (value == 0x01)
        {
            return true;
        }
        else
        {
            throw new ArgumentOutOfRangeException("Byte returned by stream is out of range (0x00 or 0x01)", nameof(stream));
        }
    }

    public static async Task<ushort> ReadUnsignedShortAsync(this Stream stream)
    {
        var buffer = new byte[2];
        await stream.ReadAsync(buffer);
        return BitConverter.ToUInt16(buffer);
    }

    public static async Task<short> ReadShortAsync(this Stream stream)
    {
        var buffer = new byte[2];
        await stream.ReadAsync(buffer);
        return BitConverter.ToInt16(buffer);
    }

    public static async Task<uint> ReadIntAsync(this Stream stream)
    {
        var buffer = new byte[4];
        await stream.ReadAsync(buffer);
        return BitConverter.ToUInt32(buffer);
    }

    public static async Task<ulong> ReadLongAsync(this Stream stream)
    {
        var buffer = new byte[8];
        await stream.ReadAsync(buffer);
        return BitConverter.ToUInt64(buffer);
    }

    public static async Task<float> ReadFloatAsync(this Stream stream)
    {
        var buffer = new byte[4];
        await stream.ReadAsync(buffer);
        return BitConverter.ToSingle(buffer);
    }

    public static async Task<double> ReadDoubleAsync(this Stream stream)
    {
        var buffer = new byte[8];
        await stream.ReadAsync(buffer);
        return BitConverter.ToDouble(buffer);
    }

    public static async Task<string> ReadStringAsync(this Stream stream)
    {
        var length = await stream.ReadVarIntAsync();
        var buffer = new byte[length];
        await stream.ReadAsync(buffer, 0, length);
        return Encoding.UTF8.GetString(buffer);
    }

    public static async Task<Chat> ReadChatAsync(this Stream stream)
    {
        var chat = await stream.ReadStringAsync();
        
        if (chat.Length > 32767)
        {
            throw new ArgumentException("string provided by stream exceeded maximum length", nameof(stream));
        }

        return JsonConvert.DeserializeObject<Chat>(chat);
    }

    public static async Task<string> ReadIdentifierAsync(this Stream stream)
    {
        var identifier = await stream.ReadStringAsync();
        if (identifier.Length > 32767) throw new ArgumentException("string provided by stream exceeded maximum length", nameof(stream));
        return identifier;
    }

    public static async Task<int> ReadVarIntAsync(this Stream stream)
    {
        var value = 0;
        var size = 0;
        int b;
        while (((b = await stream.ReadUnsignedByteAsync()) & 0x80) == 0x80)
        {
            value |= (b & 0x7F) << (size++*7);
            if (size > 5)
            {
                throw new IOException("This VarInt is an imposter!");
            }
        }
        return value | ((b & 0x7F) << (size*7));
    }

    public static async Task<long> ReadVarLongAsync(this Stream stream)
    {
        int numread = 0;
        int result = 0;
        sbyte read;
        do
        {
            read = await stream.ReadByteAsync();
            int value = (read & 0b0111111);
            result |= (value << (7 * numread));
            numread++;
            if(numread > 10) throw new Exception("VarLong is too big");
        }
        while ((read & 0b10000000) != 0);

        return result;
    }
}