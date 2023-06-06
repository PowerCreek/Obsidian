using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Obsidian.Stripped.Host;
using Obsidian.Stripped.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Obsidian.Stripped.EventPackets;
public record PacketSender(
    ILogger<PacketSender> Logger,
    BufferSlab BufferSlab    
    )
{
    public static ICompoundService<PacketSender>.RegisterServices Register => services =>
    services.AddTransient<BufferSlab>()
    .AddTransient<PacketSender>();

    private ILogger<PacketSender> Logger { get; } = Logger;
    public ClientMapValue Map { get; set; }

    private BufferSlab BufferSlab { get; } = BufferSlab;

    public async void SendPacketToBufferAsync(Memory<byte> data)
    {
        BufferSlab.InsertDataAsync(data.ToArray());

        Logger.LogInformation("LOGGING PACKET SERIALIZE");
    }
}
