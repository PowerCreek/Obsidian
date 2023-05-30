using Microsoft.Extensions.DependencyInjection;
using Obsidian.Distributed.Backend.World;

namespace Obsidian.Distributed.Backend.Dependencies;

public static class ConfigureWorldServices
{
    public static IServiceCollection AddWorldServices(this IServiceCollection services)
    {
        services.AddSingleton<WorldProvider>();
        return services;
    }
}
