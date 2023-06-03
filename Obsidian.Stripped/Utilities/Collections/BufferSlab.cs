using Obsidian.Stripped.Utilities.Collections;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace Obsidian.Stripped.Tests;

public class BufferSlab
{
    public Channel<(Memory<byte> Data, ConcurrentDictionary<BufferSlabEntry, BufferSlabEntry> Bag)> DataQueue =
        Channel.CreateUnbounded<(Memory<byte> Data, ConcurrentDictionary<BufferSlabEntry, BufferSlabEntry> Bag)>();

    public async IAsyncEnumerable<Memory<byte>> GetData([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach ((var bytes, var map) in DataQueue.Reader.ReadAllAsync().WithCancellation(cancellationToken))
            for (var entry = map[new(0)]; ;)
            {
                //Debug.WriteLine("Reading: " + entry.UUID);
                await entry.Sem.WaitAsync();

                yield return bytes.Slice(entry.Position, entry.Size);
                var position = entry.Position + entry.Size;
                entry.Dispose();
                if (map.TryRemove(new(position), out entry))
                    continue;

                break;
            }
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

        var entry = new BufferSlabEntry(currentIndex, data.Length);
        bag.TryAdd(entry, entry);
        data.CopyTo(buffer.AsSpan(currentIndex));

        entry.Sem.Release();
    }
}
