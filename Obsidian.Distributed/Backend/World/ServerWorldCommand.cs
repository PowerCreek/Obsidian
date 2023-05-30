using MessagePack;
using System.Runtime.CompilerServices;

namespace Obsidian.Distributed.Backend.World;

public struct ServerWorldCommand : ICommandSequence
{
    public int WorldValue { get; set; } 
}

