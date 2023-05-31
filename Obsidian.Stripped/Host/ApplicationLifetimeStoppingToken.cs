using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Obsidian.Stripped.Host;

public record ApplicationLifetimeStoppingTokenSource(CancellationTokenSource TokenSource)
    : ISimpleService<ApplicationLifetimeStoppingTokenSource>
{
    public static Func<IServiceProvider,ApplicationLifetimeStoppingTokenSource> AddServiceItem => service => 
        new ApplicationLifetimeStoppingTokenSource(CancellationTokenSource.CreateLinkedTokenSource(service.GetRequiredService<IHostApplicationLifetime>().ApplicationStopping));
    
    private CancellationTokenSource? TokenSource { get; } = null;
    public CancellationToken Token { get; } = TokenSource.Token;
}
