using Obsidian.Stripped.Client;
using System.Threading.Tasks.Dataflow;

namespace Obsidian.Stripped.Host;

public record ClientInstance(int Id, ClientStreamInterop ClientStreamInterop, BufferBlock<object> PacketQueue);
