using Microsoft.Extensions.ObjectPool;
using Obsidian.Utilities.Collection;
using System.Diagnostics;

namespace Obsidian.Stripped.Utilities.Collections;

public record struct BufferSlabEntry(int Position, int Size) : IEquatable<BufferSlabEntry>, IDisposable
{
    public static SemaphorePool SemaphorePool = new ();
    public SemaphoreSlim Semaphore { get; } = SemaphorePool.GetSemaphore();

    public BufferSlabEntry(int position) : this(position, default)
    {

    }

    public string UUID { get; } = Guid.NewGuid().ToString();
    public void Dispose()
    {
        Debug.WriteLine("Disposing:" + UUID);
        SemaphorePool.ReturnSemaphore(Semaphore);
    }
    public bool Equals(BufferSlabEntry other) => Position == other.Position;
    public override int GetHashCode() => Position;
}

public class SemaphoreSlimPolicy : IPooledObjectPolicy<SemaphoreSlim>
{
    public SemaphoreSlim Create()
    {
        return new SemaphoreSlim(0); // Create a new SemaphoreSlim instance
    }

    public bool Return(SemaphoreSlim semaphore)
    {
        while (semaphore.CurrentCount > 0) semaphore.Wait();
        return true;
    }
}

public class SemaphorePool
{
    private readonly Microsoft.Extensions.ObjectPool.ObjectPool<SemaphoreSlim> semaphorePool;

    public SemaphorePool()
    {
        semaphorePool = new DefaultObjectPool<SemaphoreSlim>(
            new SemaphoreSlimPolicy()
        );
    }

    public SemaphoreSlim GetSemaphore()
    {
        SemaphoreSlim semaphore = semaphorePool.Get(); // Get a semaphore from the pool
        return semaphore;
    }

    public void ReturnSemaphore(SemaphoreSlim semaphore)
    {
        semaphorePool.Return(semaphore); // Return the semaphore to the pool
    }
}
