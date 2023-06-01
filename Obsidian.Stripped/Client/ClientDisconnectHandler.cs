using Microsoft.Extensions.DependencyInjection;
using Obsidian.Stripped.Host;
using Obsidian.Stripped.Utilities.EventSystem;

namespace Obsidian.Stripped.Client;

public record ClientDisconnectArgs(int ClientId);
public record ClientDisconnectHandler() : EventDispatchBase<ClientDisconnectArgs>
{
    public static ICompoundService<ClientDisconnectHandler>.RegisterServices Register = services => services
        .WithSingleton<ClientDisconnectHandler>()
        .WithSingleton<IDispatch<ClientDisconnectArgs>, ClientDisconnectHandler>((a) => a.GetRequiredService<ClientDisconnectHandler>())
        .WithSingleton<INotify<ClientDisconnectArgs>, ClientDisconnectHandler>((a) => a.GetRequiredService<ClientDisconnectHandler>());
}

public record EventDispatchBase<T> : IDispatch<T>, INotify<T>
{
    public Action<T> Notify { get; set; } = _ => { };

    public void AddListener(Action<T> listener) => Notify += listener;

    public void Dispatch(T args) => Notify(args);
}
