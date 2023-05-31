using Microsoft.Extensions.Hosting;
using Obsidian.Stripped;

await Host.CreateDefaultBuilder()
    .ConfigureServices(services =>
    {
        services.AddStrippedObsidian();
    }).Build().RunAsync();
