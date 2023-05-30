using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Obsidian.Commands.Framework;
using Obsidian.Registries;
using Obsidian.Utilities;

namespace Obsidian.PrimaryServer.Server;

public class ProcessRunner
{
    private RunnerMetaData _metaData;

    private readonly ILogger<ProcessRunner> _logger;

    private CancellationTokenSource _cancellationTokenSource;
    public bool IsRconEnabled { get; init; } = true;
    public Task GetShutdownTask { get; init; } = Task.CompletedTask;
    public Task GetListenerStartTask { get; init; } = Task.CompletedTask;
    public Task GetLoopTask { get; init; } = Task.CompletedTask;
    public Task GetRconServerTask { get; init; } = Task.CompletedTask;

    public ProcessRunner(
        IHostApplicationLifetime lifetime,
        ILogger<ProcessRunner> logger
        )
    {
        _logger = logger;
        _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(lifetime.ApplicationStopping);
        _metaData = RunnerMetaData.SampleMetaData;

        LogShutdownOnTokenCancellation(_cancellationTokenSource.Token);
    }

    public async Task RunAsync()
    {
        LogStart();
        //time the setup.
        if (MustStopServerDueToIncompatibleConfig())
        {
            return;
        }

        //initialize recipes

        await UserCache.LoadAsync(_cancellationTokenSource.Token);

        //CommandsRegistry.Register(new CommandHandler());

        IEnumerable<Task> GetServerTasks()
        {
            yield return GetListenerStartTask;
            yield return GetLoopTask;

            if (IsRconEnabled)
                yield return GetRconServerTask;
        }

        try
        {
            await Task.WhenAll(GetServerTasks());
        }
        catch (Exception e)
        {

        }
        finally
        {
            await GetShutdownTask;
            _logger.LogInformation("The server has been shut down");
        }
    }
    
    public bool MustStopServerDueToIncompatibleConfig() => false;

    public void LogShutdownOnTokenCancellation(CancellationToken token) => token.Register(() => _logger.LogWarning("Shutting down the server..."));

    public void LogStart() => _logger.LogInformation($"Launching Server {_metaData}");
}
