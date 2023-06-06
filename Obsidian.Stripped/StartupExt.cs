using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Obsidian.Stripped.Client;
using Obsidian.Stripped.EventPackets;
using Obsidian.Stripped.Host;

namespace Obsidian.Stripped;

public static class StartupExt
{
    public static IServiceCollection AddStrippedObsidian(this IServiceCollection services)
    {
        services.AddLogging();

        services.AddTransient((s) => new NetworkMetadata(Port: 12444))
        .With(ClientConnectedCallback.Register)
        .With(SocketHandler.Register)
        .With(ObsidianServerHost.Register)
        .With(ClientWorker.Register)
        ;
        return services;
    }
}




