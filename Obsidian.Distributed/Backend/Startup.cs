using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using Microsoft.Extensions.DependencyInjection;
using Obsidian.Distributed.Backend.Dependencies;

namespace Obsidian.Distributed.Backend;
public class Startup
{
    public static Task Start(InitCheck check = null) =>
        Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddSingleton(check ?? new ());

                services.AddWorldServices();
            })
            .ConfigureServices((context, services) =>
            {
                // Configure the service host
                services.AddHostedService<ObsidianHost>();
            }).Build().RunAsync();
}
