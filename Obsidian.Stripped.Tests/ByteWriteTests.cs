using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace Obsidian.Tests;
public class ByteWriteTests
{

    [Fact(DisplayName = "WriteSlab")]
    public async Task Test()
    {
        var Slab = new BufferSlab(300);
        byte[] ITEMS = new byte[500];
        var byteArray = new byte[0];
        Array.Fill(byteArray, (byte)1);
        for (int i = 0; i < 10; i += 2)
        {
            byteArray = new byte[(i + 110)];
            ITEMS.AsSpan(0, byteArray.Length).ToArray().CopyTo(byteArray, 0);
            Slab.InsertDataAsync(byteArray);
        }

        var items = new List<byte[]>();
        try
        {
            await foreach (var v in Slab.GetData().WithCancellation(new CancellationTokenSource(1000).Token))
            {
                items.Add(v.ToArray());
            };
        }
        catch(Exception ex)
        {

        }


        Assert.True(true);
    }
}

public record struct BufferSlabEntry(int Position, int Size, SemaphoreSlim Sem) : IEquatable<BufferSlabEntry>
{
    public bool Equals(BufferSlabEntry other) => Position == other.Position;
    public override int GetHashCode() => Position;
}

public class BufferSlab
{
    public Channel<(byte[] Data, ConcurrentDictionary<BufferSlabEntry, BufferSlabEntry> Bag)> DataQueue =
        Channel.CreateUnbounded<(byte[] Data, ConcurrentDictionary<BufferSlabEntry, BufferSlabEntry> Bag)>();

    public async IAsyncEnumerable<Memory<byte>> GetData([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach ((var a, var b) in DataQueue.Reader.ReadAllAsync().WithCancellation(cancellationToken))
        {
            for (var sem = b[new BufferSlabEntry(0, 0, null)]; ;)
            {
                await sem.Sem.WaitAsync();

                yield return a.AsMemory().Slice(sem.Position, sem.Size);

                if (b.TryGetValue(sem with
                {
                    Position = sem.Position + sem.Size,
                }, out sem)) { continue; }

                break;
            }
        };
    }

    int Index = 0;
    public int Threshold { get; set; }
    public ManualResetEventSlim Reset = new(false);

    public bool BagFull = false;
    public byte[] CurrentBuffer;
    public ConcurrentDictionary<BufferSlabEntry, BufferSlabEntry> BagCatch = new();

    public BufferSlab(int threshold)
    {
        Threshold = threshold;
        CurrentBuffer = new byte[Threshold * 2];
        DataQueue.Writer.TryWrite((CurrentBuffer, BagCatch));
    }


    public int GetStart(int size, out byte[] buffer, out BufferSlabEntry entry)
    {
        var sIndex = 0;

        lock (Reset)
        {
            buffer = CurrentBuffer;
            var full = (sIndex = Index += size) > Threshold;

            if (full)
            {
                CurrentBuffer = new byte[Threshold * 2];
                BagCatch = new();
                DataQueue.Writer.TryWrite((CurrentBuffer, BagCatch));
                sIndex = size;
                Index = sIndex;
            }
        }

        var currentIndex = sIndex - size;
        var sem = new SemaphoreSlim(0);
        BagCatch.TryAdd(entry = new(currentIndex, size, sem), entry);
        return currentIndex;
    }

    public async void InsertDataAsync(byte[] data)
    {
        var currentIndex = GetStart(data.Length, out var buffer, out var entry);

        data.AsSpan().CopyTo(buffer.AsSpan(currentIndex));

        entry.Sem.Release();
    }
}
