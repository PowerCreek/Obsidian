using Obsidian.Stripped.Host;
using System.Threading.Tasks.Dataflow;

namespace Obsidian.Stripped.Client;

public record PacketQueueBufferBlockOptions(
    DataflowLinkOptions LinkOptions,
    ExecutionDataflowBlockOptions BlockOptions
);

public record ClientPacketQueue(ApplicationLifetimeStoppingTokenSource LifetimeTokenSource)
    : ICompoundService<ClientPacketQueue>
{
    public static ICompoundService<ClientPacketQueue>.RegisterServices Register => services =>
        services.WithSingleton(ApplicationLifetimeStoppingTokenSource.AddServiceDelegate)
        .WithSingleton<ClientPacketQueue>();

    private ApplicationLifetimeStoppingTokenSource LifetimeTokenSource { get; } = LifetimeTokenSource;
    
    private PacketQueueBufferBlockOptions _bufferBlockOptions { get; } = new PacketQueueBufferBlockOptions(
            LinkOptions: new DataflowLinkOptions { PropagateCompletion = true },
            BlockOptions: new ExecutionDataflowBlockOptions { CancellationToken = LifetimeTokenSource.Token, EnsureOrdered = true }
        );

    public BufferBlock<T> SetupPacketQueue<T>(Action<T> action)
    {
        var packetQueue = new BufferBlock<T>(_bufferBlockOptions.BlockOptions);
        var sendPacketBlock = new ActionBlock<T>(action, _bufferBlockOptions.BlockOptions);

        packetQueue.LinkTo(sendPacketBlock, _bufferBlockOptions.LinkOptions);

        return packetQueue;
    }
}
