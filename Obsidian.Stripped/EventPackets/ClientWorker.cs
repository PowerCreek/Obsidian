﻿using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Obsidian.Stripped.Host;
using Obsidian.Stripped.Utilities.Collections;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Obsidian.Stripped.EventPackets;

public record ClientWorker(
    ClientInstanceFeedProcessor ClientInstanceFeedProcessor,
    //ClientInstanceUpdateLoop ClientInstanceUpdateLoop,
    ILogger<ClientWorker> Logger
    ) : IHostedService, IDisposable
{
    public static ICompoundService<ClientWorker>.RegisterServices Register = services => services
        .With(ClientInstanceFeedProcessor.Register)
        //.With(ClientInstanceUpdateLoop.Register)
        .WithHostedService<ClientWorker>();

    private ClientInstanceFeedProcessor ClientInstanceFeedProcessor { get; } = ClientInstanceFeedProcessor;
    //private ClientInstanceUpdateLoop ClientInstanceUpdateLoop { get; } = ClientInstanceUpdateLoop;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        Task[] tasks = new[] {
            ClientInstanceFeedProcessor.LoopFeed(),
            //ClientInstanceUpdateLoop.LoopInstances(),
        };

        await Task.WhenAll(tasks);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
    }
    public void Dispose() { }
}
