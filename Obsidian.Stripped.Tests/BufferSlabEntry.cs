using System.Diagnostics;

namespace Obsidian.Tests;

public record struct BufferSlabEntry(int Position, int Size, SemaphoreSlim Sem) : IEquatable<BufferSlabEntry>, IDisposable
{

    public BufferSlabEntry(int position) : this(position, default, null){

    }

    public string UUID { get; } = Guid.NewGuid().ToString();
    public void Dispose() 
    {
        Debug.WriteLine("Disposing:"+UUID);
        ((IDisposable)Sem).Dispose();
    }
    public bool Equals(BufferSlabEntry other) => Position == other.Position;
    public override int GetHashCode() => Position;
}
