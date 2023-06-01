using Microsoft.Extensions.Logging;
using Obsidian.Stripped.Host;
using System.Collections.Concurrent;

namespace Obsidian.Stripped.EventPackets;

public record ClientInstanceUpdateLoop(
    ApplicationLifetimeStoppingTokenSource TokenSource,
    ILogger<ClientInstanceUpdateLoop> Logger,
    ConcurrentDictionary<int, IClientInstance> ClientMap
    )
{
    public static ICompoundService<ClientInstanceUpdateLoop>.RegisterServices Register = services => services
        .WithSingleton(ApplicationLifetimeStoppingTokenSource.AddServiceItem)
        .WithSingleton<ConcurrentDictionary<int, IClientInstance>>()
        .WithSingleton<ClientInstanceUpdateLoop>();

    private ILogger<ClientInstanceUpdateLoop> Logger { get; } = Logger;
    private ApplicationLifetimeStoppingTokenSource TokenSource { get; } = TokenSource;
    private ConcurrentDictionary<int, IClientInstance> ClientMap { get; } = ClientMap;

    public async Task LoopInstances()
    {
        Logger.LogInformation(string.Join(Environment.NewLine, new string[10].Select(e=>"TEST").ToArray()));
    }
}
