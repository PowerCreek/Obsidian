using Obsidian.Net;
using Obsidian.Utilities;

namespace Obsidian.Stripped.EventPackets;
public record PacketHandler(
    )
{
    public record struct PacketResult(int Id, byte[] Data);

    public async Task<PacketResult> GetNextPacketAsync()
    {
        var mcDefault = default(MinecraftStream);
        
        var length = await mcDefault.ReadVarIntAsync();
        var bytes = new byte[length];

        _ = await mcDefault.ReadAsync(bytes.AsMemory(0, length));

        var id = 0;
        var data = Array.Empty<byte>();

        using var packetStream = new MinecraftStream(bytes);
        try
        {
            id = await packetStream.ReadVarIntAsync();
            var arlen = 0;

            if (length - id.GetVarIntLength() > - 1)
                arlen = length - id.GetVarIntLength();

            data = new byte[arlen];
            _ = await packetStream.ReadAsync(data.AsMemory(0, data.Length));
        }
        catch(Exception ex)
        {
            throw;
        }

        return new (id, data);
    }
}
