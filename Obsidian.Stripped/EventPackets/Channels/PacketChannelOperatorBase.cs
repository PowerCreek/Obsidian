using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;
using System.Threading.Channels;
using System.Threading.Tasks.Dataflow;

namespace Obsidian.Stripped.EventPackets.Channels;

public interface IPacketChannel<T, U>
{
    public Task SendAsync(T item, CancellationToken cancellation = default);
}

public abstract record PacketChannelOperatorBase<T, U>(U Channel) : IPacketChannel<T, U>
{
    protected U Channel { get; } = Channel;

    public abstract Task SendAsync(T item, CancellationToken cancellationToken = default);
}

public record BufferBlockOperator<T>(BufferBlock<T> Channel) : PacketChannelOperatorBase<T, BufferBlock<T>>(Channel)
{
    public override async Task SendAsync(T item, CancellationToken cancellationToken = default)
        => await Channel.SendAsync(item, cancellationToken);
}

public record ChannelOperatorProcessor<T>(Channel<T> Channel)
{
    public readonly TaskCompletionSource<Action<T>> ReaderDelegate = new TaskCompletionSource<Action<T>>();

    public void SetReaderCallback(Action<T> a) =>
        ReaderDelegate.SetResult(a);

    public async void StartReaderAsync()
    {
        var action = await ReaderDelegate.Task;
        await foreach (var item in Channel.Reader.ReadAllAsync())
            action(item);
    }
}

public record ChannelOperator<T>(ChannelOperatorProcessor<T> Processor) : PacketChannelOperatorBase<T, Channel<T, T>>(Processor.Channel)
{
    public ChannelOperatorProcessor<T> Processor { get; } = Processor;

    public void SetupChannel(Action<T> action) =>
        (this as ChannelOperator<T>)!.Processor.SetReaderCallback(action);

    public override async Task SendAsync(T item, CancellationToken cancellationToken = default)
        => await Channel.Writer.WriteAsync(item, cancellationToken);
}

public static class ChannelOperatorExt
{
    public static IServiceCollection AddChannelOperator(this IServiceCollection services, Type t)
    {
        Type channelOperatorType = t;
        Type itemType = channelOperatorType.GetGenericArguments()[0]; // Extract the item type

        Type channelType = typeof(Channel);
        MethodInfo createUnboundedMethod = channelType
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .FirstOrDefault(method => method.Name == nameof(Channel.CreateUnbounded) && method.GetParameters().Length == 0);

        MethodInfo createUnboundedGenericMethod = createUnboundedMethod.MakeGenericMethod(itemType);
        services.TryAddTransient(typeof(Channel<>).MakeGenericType(itemType), s => 
        {
            var result = createUnboundedGenericMethod.Invoke(null, null)!;
            return result;
        });

        services.TryAddTransient(typeof(ChannelOperatorProcessor<>).MakeGenericType(itemType));
        services.TryAddTransient(channelOperatorType);
        return services;
    }
}
