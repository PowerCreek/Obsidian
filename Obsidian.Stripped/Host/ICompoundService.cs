using Microsoft.Extensions.DependencyInjection;

namespace Obsidian.Stripped.Host;

public interface ICompoundService<T> where T : class
{
    public delegate IServiceCollection RegisterServices(IServiceCollection services);
}
