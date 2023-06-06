

namespace Obsidian.Stripped.BusinessLogic.Actions.Commands;

public abstract record EntityHandlerBase<O>() : IRequestHandler<IRequest<O>, O>
{
    protected virtual Func<IRequest<O>, O> HandleAction { get; } = (_) => default;

    public async Task<O> Handle(IRequest<O> request, CancellationToken cancellationToken)
    {
        return HandleAction(request);
    }
}

public abstract record VoidHandlerBase<T>() : IRequestHandler<T> where T : IRequest
{
    protected virtual Action<IRequest> HandleAction { get; } = (_) => { };

    public async Task Handle(T request, CancellationToken cancellationToken) => HandleAction(request);
}
