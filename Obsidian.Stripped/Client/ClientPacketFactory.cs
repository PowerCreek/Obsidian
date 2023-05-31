using Microsoft.Extensions.DependencyInjection;
using Obsidian.Stripped.Host;

namespace Obsidian.Stripped.Client;

public record ClientPacketFactory(Func<ClientPacketQueue> CreateQueueInstance)
{
    public static Func<IServiceProvider, ClientPacketFactory> AddServiceItem => service =>
        new ClientPacketFactory(()=> service.GetRequiredService<ClientPacketQueue>());

    public static ICompoundService<ClientPacketFactory>.RegisterServices Register = services =>
        services
            .With(ClientPacketQueue.Register)
            .WithSingleton(AddServiceItem);
}
