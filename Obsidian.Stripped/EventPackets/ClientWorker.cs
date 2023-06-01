using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Obsidian.Stripped.Host;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Obsidian.Stripped.EventPackets;

public record ClientWorker(
    ClientInstanceFeedProcessor ClientInstanceFeedProcessor,
    ClientInstanceUpdateLoop ClientInstanceUpdateLoop,
    ILogger<ClientWorker> Logger
    ) : IHostedService, IDisposable
{
    public static ICompoundService<ClientWorker>.RegisterServices Register = services => services
        .With(ClientInstanceUpdateLoop.Register)
        .With(ClientInstanceFeedProcessor.Register)
        .WithHostedService<ClientWorker>();

    private ClientInstanceFeedProcessor ClientInstanceFeedProcessor { get; } = ClientInstanceFeedProcessor;
    private ClientInstanceUpdateLoop ClientInstanceUpdateLoop { get; } = ClientInstanceUpdateLoop;


    public async Task StartAsync(CancellationToken cancellationToken)
    {
        Task[] tasks = new[] {
            ClientInstanceUpdateLoop.LoopInstances(),
            ClientInstanceFeedProcessor.LoopFeed()
        };

        await Task.WhenAll(tasks);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
    }
    public void Dispose() { }
}
