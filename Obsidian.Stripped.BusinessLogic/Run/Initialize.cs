using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Obsidian.Stripped.BusinessLogic.Run;
public static class BusinessLogicSetup
{
    public static IServiceCollection AddGameLogic(this IServiceCollection services)
    {
        return services;
    }

    public static IServiceCollection With<TDelegate>(this IServiceCollection services, TDelegate registerServices)
        where TDelegate : Delegate => (IServiceCollection)registerServices.DynamicInvoke(services)!;

    public static IServiceCollection WithHostedService<TService>(this IServiceCollection services)
        where TService : class, IHostedService => services.AddHostedService<TService>();

    public static IServiceCollection TryWrapSingleton(this IServiceCollection services, Action<IServiceCollection> action)
    {
        action(services);
        return services;
    }

    public static IServiceCollection WithSingleton<TService>(this IServiceCollection services, Func<IServiceProvider, TService> func)
        where TService : class
    {
        services.TryAddSingleton(func);
        return services;
    }

    public static IServiceCollection WithScoped<TService>(this IServiceCollection services, Func<IServiceProvider, TService> func)
        where TService : class
    {
        services.TryAddScoped(func);
        return services;
    }

    public static IServiceCollection WithTransient<TService>(this IServiceCollection services, Func<IServiceProvider, TService> func)
        where TService : class
    {
        services.TryAddTransient(func);
        return services;
    }

    public static IServiceCollection WithSingleton<TService, TImplementation>(this IServiceCollection services, Func<IServiceProvider, TImplementation> func)
        where TService : class
        where TImplementation : class, TService
    {
        services.TryAddSingleton<TService>(func);
        return services;
    }

    public static IServiceCollection WithScoped<TService, TImplementation>(this IServiceCollection services, Func<IServiceProvider, TImplementation> func)
        where TService : class
        where TImplementation : class, TService
    {
        services.TryAddScoped<TService>(func);
        return services;
    }
    public static IServiceCollection WithTransient<TService, TImplementation>(this IServiceCollection services, Func<IServiceProvider, TImplementation> func)
        where TService : class
        where TImplementation : class, TService
    {
        services.TryAddTransient<TService>(func);
        return services;
    }

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
