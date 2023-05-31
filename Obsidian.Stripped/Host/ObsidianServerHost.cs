using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Obsidian.Stripped.Host;
internal class ObsidianServerHost : BackgroundService
{
    public static ICompoundService<ObsidianServerHost>.RegisterServices Register = services =>
        services.WithSingleton<SocketHandler>()
        .WithHostedService<ObsidianServerHost>();

    private IHostApplicationLifetime _lifetime;
    private SocketHandler _socketHandler;
    public ObsidianServerHost(
        IHostApplicationLifetime lifetime,
        SocketHandler SockerHandler
        )
    {
        _lifetime = lifetime;
        _socketHandler = SockerHandler;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await RunAsync();
        }
        catch (Exception e)
        {

        }
    }

    public async Task RunAsync()
    {
        await _socketHandler.AcceptSocketsAsync();
    }
}
