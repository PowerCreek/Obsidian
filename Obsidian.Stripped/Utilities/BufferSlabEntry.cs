using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Obsidian.Stripped.Utilities
{
    public record struct BufferSlabEntry(int Position, int Size, SemaphoreSlim Sem) : IEquatable<BufferSlabEntry>, IDisposable
    {
        public static ILogger Logger = new LoggerFactory().CreateLogger(typeof(BufferSlabEntry));
        public BufferSlabEntry(int position) : this(position, default, default) { }

        public void Dispose() 
        {
            Debug.WriteLine("Test");
            ((IDisposable)Sem)?.Dispose();
        }
        public bool Equals(BufferSlabEntry other) => Position == other.Position;
        public override int GetHashCode() => Position;

        public readonly string UUID = Guid.NewGuid().ToString(); 
    }
}
