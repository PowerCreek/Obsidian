using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Obsidian.Stripped.Host;

namespace Obsidian.Stripped.EventPackets;
public record ClientPacketInitFactory(
    Func<ClientPacketInit> CreatePacketInit
    )
{
    public static Func<IServiceProvider, ClientPacketInitFactory> AddServiceItem => service =>
        new ClientPacketInitFactory(() => service.GetRequiredService<ClientPacketInit>());

    public static ICompoundService<ClientPacketInitFactory>.RegisterServices Register = services => services
            .With(ClientPacketInit.Register)
            .WithSingleton(AddServiceItem);

    private Func<ClientPacketInit> CreatePacketInit { get; } = CreatePacketInit;
    public PacketAction<T> PerformPacketSend<T>() => CreatePacketInit().DoCreate<T>();
}

public delegate int PacketAction<T>(T t);

public record ClientPacketInit(
    ILogger<ClientPacketInit> Logger
    )

{

    public static ICompoundService<ClientPacketInit>.RegisterServices Register = services => services
            .WithTransient<ClientPacketInit>();

    private ILogger<ClientPacketInit> Logger { get; } = Logger;

    public string UUID = Guid.NewGuid().ToString();
    public PacketAction<T> DoCreate<T>() => (t) =>
    {
        Logger.LogInformation("Connected: ClientPacketInit: " + UUID);

        return 1;
    };
}
