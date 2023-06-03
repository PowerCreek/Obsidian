using Obsidian.Entities;
using System.Collections.Concurrent;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace Obsidian.Tests;

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
        CurrentBuffer = new byte[Threshold];
        DataQueue.Writer.TryWrite((CurrentBuffer, BagCatch));
    }


    public int GetStart(int size, out ConcurrentDictionary<BufferSlabEntry, BufferSlabEntry> bag, out byte[] buffer)
    {
        var sIndex = 0;

        lock (Reset)
        {
            if ((sIndex = Index += size) > Threshold)
            {
                CurrentBuffer = new byte[Threshold];
                BagCatch = new();
                DataQueue.Writer.TryWrite((CurrentBuffer, BagCatch));
                sIndex = size;
                Index = sIndex;
            }

            buffer = CurrentBuffer;
            bag = BagCatch;
        }

        var currentIndex = sIndex - size;
        return currentIndex;
    }

    public async void InsertDataAsync(byte[] data)
    {
        var currentIndex = GetStart(data.Length, out var bag, out var buffer);

        var entry = new BufferSlabEntry(currentIndex, data.Length, new SemaphoreSlim(0));
        bag.TryAdd(entry, entry);
        data.AsSpan().CopyTo(buffer.AsSpan(currentIndex));

        entry.Sem.Release();
    }
}
