using Microsoft.Extensions.DependencyInjection;
using Obsidian.Stripped.Host;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Obsidian.Stripped.EventHandling;
public class EventDefinitions
{
    public static ICompoundService<EventDefinitions>.RegisterServices RegisterOnPlayerJoin => services =>
    {
        return services;
    };

    public static ICompoundService<EventDefinitions>.RegisterServices RegisterOnPacketReceived => services =>
    {
        return services;
    };
}

public static class EventDefinitionsExt
{
    public static IServiceCollection AddEventDefinitions(this IServiceCollection services)
    {
        services

            .With(EventDefinitions.RegisterOnPlayerJoin);

        return services;
    }
}
