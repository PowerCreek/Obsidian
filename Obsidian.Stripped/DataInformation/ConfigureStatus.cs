using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Obsidian.API;
using Obsidian.Stripped.Host;

namespace Obsidian.Stripped.DataInformation;
public record ConfigureStatus()
{
    public static ICompoundService<ConfigureStatus>.RegisterServices Register => services => 
    services.WithSingleton<ConfigureStatus>();
    
    public List<object> ServerSample() => new List<object>{};
    public int? ServerCapacity() => 12;
    public int? PlayerCount() => 12;
    public string ServerName() => "The Server";
    public ProtocolVersion? ProtocolVersion() => API.ProtocolVersion.v1_19_4;
    public string Description() => "Some Information";
    public string Favicon() => null;

}
