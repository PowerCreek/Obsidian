namespace Obsidian.Tests;

public record struct BufferSlabEntry(int Position, int Size, SemaphoreSlim Sem) : IEquatable<BufferSlabEntry>
{
    public bool Equals(BufferSlabEntry other) => Position == other.Position;
    public override int GetHashCode() => Position;
}
