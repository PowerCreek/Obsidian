using Microsoft.Extensions.Hosting;
using System.Diagnostics;

namespace Obsidian.Distributed.Backend;

public class ObsidianHost : IHostedService
{
    public InitCheck Check { get; set; }
    public ObsidianHost(InitCheck check)
    {
        Check = check;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Check.MRE.Set();
        Debug.WriteLine("Started Service");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
