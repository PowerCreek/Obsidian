using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Obsidian.Stripped.Client;
using Obsidian.Stripped.Host;

namespace Obsidian.Stripped;

public static class StartupExt
{
    public static IServiceCollection AddStrippedObsidian(this IServiceCollection services)
    {
        services.AddLogging();

        services.AddTransient((s) => new NetworkMetadata(Port: 12444))
        .With(ClientPacketFactory.Register)
        .With(ClientConnectedCallback.Register)
        .With(SocketHandler.Register)
        .With(ObsidianServerHost.Register);

        return services;
    }

    public static IServiceCollection With<TDelegate>(this IServiceCollection services, TDelegate registerServices)
        where TDelegate : Delegate => (IServiceCollection) registerServices.DynamicInvoke(services)!;

    public static IServiceCollection WithHostedService<TService>(this IServiceCollection services)
        where TService : class, IHostedService => services.AddHostedService<TService>();

    public static IServiceCollection WithSingleton<TDelegate>(this IServiceCollection services, TDelegate func)
        where TDelegate : Delegate
    {
        var type = func.Method.ReturnType;
        services.TryAddSingleton(type, sp => func.DynamicInvoke(sp)!);
        return services;
    }

    public static IServiceCollection WithScoped<TDelegate>(this IServiceCollection services, TDelegate func)
        where TDelegate : Delegate
    {
        var type = func.Method.ReturnType;
        services.TryAddSingleton(type, sp => func.DynamicInvoke(sp)!);
        return services;
    }

    public static IServiceCollection WithTransient<TDelegate>(this IServiceCollection services, TDelegate func)
        where TDelegate : Delegate
    {
        var type = func.Method.ReturnType;
        services.TryAddSingleton(type, sp => func.DynamicInvoke(sp)!);
        return services;
    }

    public static IServiceCollection WithSingleton<TService>(this IServiceCollection services)
        where TService : class
    {
        services.TryAddSingleton<TService>();
        return services;
    }

    public static IServiceCollection WithScoped<TService>(this IServiceCollection services)
        where TService : class 
    {
        services.TryAddScoped<TService>();
        return services;
    }

    public static IServiceCollection WithTransient<TService>(this IServiceCollection services)
        where TService : class
    {
        services.TryAddTransient<TService>();
        return services;
    }

    public static IServiceCollection WithSingleton<TService, TImplementation>(this IServiceCollection services)
        where TService : class
        where TImplementation : class, TService
    {
        services.TryAddSingleton<TService, TImplementation>();
        return services;
    }

    public static IServiceCollection WithScoped<TService, TImplementation>(this IServiceCollection services)
        where TService : class
        where TImplementation : class, TService 
    {
        services.TryAddScoped<TService, TImplementation>();
        return services;
    } 

    public static IServiceCollection WithTransient<TService, TImplementation>(this IServiceCollection services)
        where TService : class
        where TImplementation : class, TService
    {
        services.TryAddTransient<TService, TImplementation>();
        return services;
    }
}




