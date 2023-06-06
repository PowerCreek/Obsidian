using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Obsidian.Stripped.EventPackets.PacketData;

public interface IPacketData
{
    public int Id { get; }
    object GetData();
}

public record struct PacketData<T>(int Id) : IPacketData
{
    public T Data { get; }
    int IPacketData.Id => Id;
    object IPacketData.GetData() => Data;
}
