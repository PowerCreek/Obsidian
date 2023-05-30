using Obsidian.Distributed.Backend;

namespace Obsidian.Distributed.Tests;

public class Hosting
{
    [Fact(DisplayName = "ServiceHostRuns")]
    public async Task TestServiceHost()
    {
        var initCheck = new InitCheck();

        _ = Startup.Start(initCheck);

        await initCheck.Proceed();

        Assert.True(true);
    }

}
