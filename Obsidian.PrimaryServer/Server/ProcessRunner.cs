using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Obsidian.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Obsidian.PrimaryServer.Server;

public class ProcessRunner
{
    private RunnerMetaData _metaData;

    private readonly ILogger<ProcessRunner> _logger;

    private CancellationTokenSource _cancellationTokenSource;

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
    }

    public bool MustStopServerDueToIncompatibleConfig() => false;

    public void LogShutdownOnTokenCancellation(CancellationToken token) => token.Register(() => _logger.LogWarning("Shutting down the server..."));

    public void LogStart() => _logger.LogInformation($"Launching Server {_metaData}");
}
