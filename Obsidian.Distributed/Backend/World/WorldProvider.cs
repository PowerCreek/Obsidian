using Obsidian.Distributed.Backend.ListenerInterop.cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Obsidian.Distributed.Backend.World;
public class WorldProvider
{
    public WorldProvider()
    {

    }
}

public class WorldProviderListener
{
    public object CommandQueue { get; set; } = new ();
    public object FulfillmentQueue { get; set; } = new ();
    public WorldProviderListenerConfig Config { get; init; } = new ();
    public WorldProvider Provider { get; init; }
    public ListenerInterop<ICommandSequence> Listener { get; init; }
}
