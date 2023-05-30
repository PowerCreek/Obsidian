using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Obsidian.PrimaryServer.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Obsidian.PrimaryServer.Definition;

public static class ObsidianPrimaryHostServiceExt
{
    public static IServiceCollection AddObsidianPrimaryHost(this IServiceCollection services)
    {
        services.AddSingleton<ProcessRunner>();

        services.AddHostedService<ObsidianPrimaryHostService>();

        return services;
    }
}

public class ObsidianPrimaryHostService : BackgroundService
{
    private readonly IHostApplicationLifetime _lifetime;
    private readonly ILogger _logger;
    private readonly ProcessRunner _runner;

    public bool ExitOnServerShutdown => true;

    public Func<ILogger,Task> OnServerStoppedGracefullyAsync = async _ => await Task.CompletedTask;
    public Func<ILogger, Exception,Task> OnServerCrashedAsync = async (_,_) => await Task.CompletedTask;

    public ObsidianPrimaryHostService(
        IHostApplicationLifetime lifetime,
        ILogger<ObsidianPrimaryHostService> logger,
        ProcessRunner runner
        )
    {
        _lifetime = lifetime;
        _logger = logger;
        _runner = runner;
    }

    protected async override Task ExecuteAsync(CancellationToken cToken)
    {
        try
        {
            await _runner.RunAsync();
            await OnServerStoppedGracefullyAsync(_logger);
        }
        catch (Exception e)
        {
            await OnServerCrashedAsync(_logger, e);
        }
        if (ExitOnServerShutdown)
            _lifetime.StopApplication();
    }
}
