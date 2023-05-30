using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Obsidian.Utilities;
using System.Net.Sockets;
using System.Net;
using System.Collections.Concurrent;

namespace Obsidian.PrimaryServer.Server;

public class ServerConfigurationService
{
    public ServerConfiguration GetConfigurationInstance()
    {
        var config = new ServerConfiguration();
        return config;
    }
}

public class ClientFactory
{
    private ServerConfigurationService _configService;

    public List<int> ClientIds = new ();
    public List<int> AvailableIds = new ();

    public ConcurrentDictionary<int, Client> ClientMap = new();
    public Action<int> OnClientDisconnect { get; set; }
    
    public void RenewId(int id)
    {
        lock (AvailableIds) {
            ClientIds.Remove(id);
            AvailableIds.Add(id);
        }
    }
    
    public int GetAvailableId()
    {
        lock (AvailableIds)
        {           
            switch(AvailableIds)
            {
                case []:
                    var id = ClientIds.Count();
                    ClientIds.Add(id);
                    return id;
                case [var i,..]:
                    AvailableIds.Remove(i);
                    ClientIds.Add(i);
                    return i;
                default: return -1;
            };
        }
    }

    public ClientFactory(ServerConfigurationService configService) 
    {
        _configService = configService;
    }

    public Client CreateClient(Socket socket)
    {
        var clientId = GetAvailableId();
        var client = new Client(socket, _configService.GetConfigurationInstance(), clientId, null);
        ClientMap.TryAdd(clientId, client);

        client.Disconnected += client =>
        {
            ClientMap.TryRemove(clientId, out var value);
            OnClientDisconnect(clientId);
        };

        return client;
    }

    public void RegisterClientAtSocket(Socket socket)
    {
        var client = CreateClient(socket);
        _ = client.StartConnectionAsync();
    }
}

public class ProcessRunnerClientListenerAdapter
{
    private readonly ILogger _logger;

    private TcpListener _tcpListener;
    private CancellationTokenSource _cancellationTokenSource;
    private ServerIpAddressAcl _ipAddressAcessControl;
    private ClientFactory _clientLinker;

    public ProcessRunnerClientListenerAdapter(
        IHostApplicationLifetime lifetime,
        ILogger<ProcessRunnerClientListenerAdapter> logger,
        ServerIpAddressAcl ipAddressAcessControl,
        ClientFactory clientLinker
        )
    {
        _logger = logger;
        _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(lifetime.ApplicationStopping);
        _ipAddressAcessControl = ipAddressAcessControl;
        _clientLinker = clientLinker;
    }

    public void Initialize(RunnerMetaData metaData)
    {
        _tcpListener = new TcpListener(IPAddress.Any, metaData.ServerPort);
    }

    private async Task AcceptClientsAsync()
    {
        _tcpListener.Start();

        while (!_cancellationTokenSource.Token.IsCancellationRequested)
        {
            Socket socket;
            try
            {
                socket = await _tcpListener.AcceptSocketAsync(_cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                // No longer accepting clients.
                break;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Listening for clients encountered an exception");
                break;
            }

            _logger.LogDebug("New connection from client with IP {ip}", socket.RemoteEndPoint);

            string ip = ((IPEndPoint)socket.RemoteEndPoint!).Address.ToString();

            if (_ipAddressAcessControl.IpClientIpAddressAllowed(ip))
            {
                _logger.LogInformation("{ip} is not whitelisted. Closing connection", ip);
                socket.Disconnect(false);
                return;
            }

            //throttling is how often someone is trying to join too quickly

            // TODO Entity ids need to be unique on the entire server, not per world
            
            _clientLinker.RegisterClientAtSocket(socket);
        }

        _logger.LogInformation("No longer accepting new clients");
        _tcpListener.Stop();
    }
}

public class ProcessRunner
{
    private RunnerMetaData _metaData;

    private readonly ILogger<ProcessRunner> _logger;

    private CancellationTokenSource _cancellationTokenSource;
    public bool IsRconEnabled { get; init; } = true;
    public Task GetShutdownTask { get; init; } = Task.CompletedTask;
    public Task GetListenerStartTask { get; init; } = Task.CompletedTask;
    public Task GetLoopTask { get; init; } = Task.CompletedTask;
    public Task GetRconServerTask { get; init; } = Task.CompletedTask;

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

        //CommandsRegistry.Register(new CommandHandler());

        IEnumerable<Task> GetServerTasks()
        {
            yield return GetListenerStartTask;
            yield return GetLoopTask;

            if (IsRconEnabled)
                yield return GetRconServerTask;
        }

        try
        {
            await Task.WhenAll(GetServerTasks());
        }
        catch (Exception e)
        {

        }
        finally
        {
            await GetShutdownTask;
            _logger.LogInformation("The server has been shut down");
        }
    }
    
    public bool MustStopServerDueToIncompatibleConfig() => false;

    public void LogShutdownOnTokenCancellation(CancellationToken token) => token.Register(() => _logger.LogWarning("Shutting down the server..."));

    public void LogStart() => _logger.LogInformation($"Launching Server {_metaData}");
}
