namespace Obsidian.Distributed.Backend;

public class InitCheck
{
    public ManualResetEvent MRE = new ManualResetEvent(false);
    public async Task Proceed()
    {
        MRE.WaitOne();
        await Task.CompletedTask;
    }
}
