using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Obsidian.Stripped.BusinessLogic.Run;

namespace Obsidian.Stripped.BusinessLogic;

public class ServicesSetupTest
{

}

public class TestHost : IHostedService
{
    public static Action<IServiceCollection> Register => services => services.WithHostedService<TestHost>();
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
    }
}
