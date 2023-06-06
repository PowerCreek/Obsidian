global using StateAction = System.Action<System.Memory<byte>>;
using Microsoft.Extensions.Logging;
using Obsidian.API;
using Obsidian.Net;
using Obsidian.Net.Packets;
using Obsidian.Net.Packets.Handshaking;
using Obsidian.Net.Packets.Login;
using Obsidian.Stripped.DataInformation;
using Obsidian.Stripped.EventPackets;
using Obsidian.Stripped.Host;
using Obsidian.Utilities;

namespace Obsidian.Stripped.Interop;

public record StateActions(
    ILogger<StateActions> Logger,
    NetworkMetadata NetworkMetadata,
    ConfigureStatus ConfigureStatus,
    PacketSender PacketSender
    )
{
    public record struct ClientStateValue(ClientState NextState, bool ShouldDisconnect);
    public static ICompoundService<StateActions>.RegisterServices Register => services =>
    services.With(PacketSender.Register)
    .WithSingleton<ConfigureStatus>()
    .WithTransient<StateActions>();

    public ConfigureStatus ConfigureStatus { get; set; } = ConfigureStatus;
    public PacketSender PacketSender { get; set; } = PacketSender;
    public ClientMapValue Client { get; set; }
    public PacketProcessor Processor { get; set; } = null;

    public byte[] ValidationToken;

    public StateActions Init(PacketProcessor processor)
    {
        PacketSender = PacketSender with
        {
            Map = processor.Client
        };

        return this with 
        {
            Processor = processor,
            Client = processor.Client
        };
    }

    public StateAction OnStatus0x00 => data=> 
    {
        Logger.LogInformation($"Status 0x00");


    };

    public StateAction OnStatus0x01 => data =>
    {
        Logger.LogInformation($"Status 0x01");

        PacketSender.SendPacketToBufferAsync(data);
    };

    public StateAction OnHandshake0x00 => data => 
    {
        var result = Handshake.Deserialize(data.ToArray());
        var cases = result switch
        {
            { NextState: ClientState.Login, Version: var clientProtocol } when clientProtocol is API.ProtocolVersion.v1_19_4  => new ClientStateValue(NextState: result.NextState, ShouldDisconnect: false),
            { NextState: ClientState.Login, Version: var clientProtocol } => GetProtocolVersion(API.ProtocolVersion.v1_19_4, clientProtocol),
            { NextState: ClientState.Status or ClientState.Handshaking } => new (NextState: result.NextState, ShouldDisconnect: true),
            _ => new (NextState: ClientState.Closed, ShouldDisconnect: true)
        };

        Processor.State = cases!.Value!.NextState;

        if (result is {NextState: ClientState.Login } && cases is { ShouldDisconnect: true })
        {
            Logger.LogInformation($"Disconnecting client: {cases}");
            return;
        } 

        if (cases is { NextState: ClientState.Closed, ShouldDisconnect: true })
        {
            Logger.LogWarning("Client sent unexpected state ({RedText}{ClientState}{WhiteText}), forcing it to disconnect.", ChatColor.Red, result.NextState, ChatColor.White);
            return;
        }

        Logger.LogInformation("Handshaking with client (protocol: {YellowText}{VersionDescription}{WhiteText} [{YellowText}{Version}{WhiteText}], server: {YellowText}{ServerAddress}:{ServerPort}{WhiteText})", ChatColor.Yellow, result.Version.GetDescription(), ChatColor.White, ChatColor.Yellow, result.Version, ChatColor.White, ChatColor.Yellow, result.ServerAddress, result.ServerPort, ChatColor.White);
    };

    public ClientStateValue? GetProtocolVersion(ProtocolVersion Server, ProtocolVersion Client)
    {
        var message = ((int)Client > (int)Server) ? "Outdated Server" : "Outdated Client";
        Logger.LogError($"Your version is {Client}, {message}! Join with Client Version: {Server}");
        return new (ClientState.Closed, true);
    }

    public StateAction OnHandshake__ { get; set; } = a => 
    {
    
    };

    public StateAction OnLogin0x00 => data => 
    {
        var result = LoginStart.Deserialize(data.ToArray());

        var username = result.Username;
        //validate user

        var playerUUID = result.PlayerUuid;

        var (publicKey, randomToken) = new PacketCryptography().GeneratePublicKeyAndToken();

        ValidationToken = randomToken;
    };

    public StateAction OnLogin0x01 { get; set; } = a => 
    {
    
    };

    public StateAction OnLogin0x02 { get; set; } = a => 
    { 
    
    };

    public StateAction OnPlay__ { get; set; } = a => 
    {
    
    };

    public StateAction OnClose__{ get; set; } = a => 
    {
    
    };
}
