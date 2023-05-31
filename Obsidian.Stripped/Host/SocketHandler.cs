using Obsidian.Stripped.Client;
using System.Net;
using System.Net.Sockets;

namespace Obsidian.Stripped.Host;

public record SocketHandler(
    NetworkMetadata Metadata,
    ApplicationLifetimeStoppingTokenSource TokenSource,
    IClientConnectedCallback ClientConnectedCallback
    ) : ICompoundService<SocketHandler>
{
    public static ICompoundService<SocketHandler>.RegisterServices Register = services => services
        .WithSingleton(ApplicationLifetimeStoppingTokenSource.AddServiceDelegate)
        .WithSingleton<IClientConnectedCallback, ClientConnectedCallback>()
        .WithSingleton<SocketHandler>();

    private NetworkMetadata? Metadata { get; } = Metadata;
    private ApplicationLifetimeStoppingTokenSource TokenSource { get; } = TokenSource;
    private IClientConnectedCallback ClientConnectedCallback { get; } = ClientConnectedCallback;

    private readonly TcpListener _listener = new TcpListener(IPAddress.Any, Metadata.Port);

    public async Task AcceptSocketsAsync()
    {
        _listener.Start();

        while (!TokenSource.Token.IsCancellationRequested)
        {
            Socket socket;
            try
            {
                socket = await _listener.AcceptSocketAsync(TokenSource.Token);
                ClientConnectedCallback.Callback(socket);
            }
            catch (OperationCanceledException e)
            {
                break;
            }
            catch (Exception e)
            {
                break;
            }
        }

        _listener.Stop();
    }
}
