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
        {
            for (var entry = map[new(0)]; ;)
            {
                //Debug.WriteLine("Reading: " + entry.UUID);
                await entry.Semaphore.WaitAsync();

                yield return bytes.Slice(entry.Position, entry.Size);

                var position = entry.Position + entry.Size;

                entry.Dispose();

                if (map.TryRemove(new(position), out entry))
                    continue;

                break;
            }
        }
    }

    int Index = 0;
    public int Threshold { get; set; }
    public ManualResetEventSlim Reset = new(false);

    public bool BagFull = false;
    public byte[][] CurrentBuffer = null!;
    public ConcurrentDictionary<BufferSlabEntry, BufferSlabEntry> BagCatch = new();

    public BufferSlab(int threshold)
    {
        Threshold = threshold;
        WriteMemory(GetBuffer(), BagCatch);
    }

    private byte[][] GetBuffer()
    {
        return CurrentBuffer ??= CreateBuffer();
    }

    private byte[][] CreateBuffer(int? threshold = null)
    {
        var bytes = new byte[threshold ?? Threshold];
        return CurrentBuffer = new byte[][] { bytes };
    }

    private void WriteMemory(byte[][] buffer, ConcurrentDictionary<BufferSlabEntry, BufferSlabEntry> bagCatch)
    {
        DataQueue.Writer.TryWrite((new Memory<byte>(buffer[0]), bagCatch));
    }

    private int GetStart(int size, out ConcurrentDictionary<BufferSlabEntry, BufferSlabEntry> bag, out Span<byte> buffer)
    {
        var sIndex = 0;
        byte[][] bufferHold;

        lock (Reset)
        {
            bufferHold = CurrentBuffer;
            bag = BagCatch;

            sIndex = Index += size;

            void ResetProperties(int? bufferSize = null)
            {
                bufferHold = CreateBuffer(bufferSize);
                WriteMemory(bufferHold, BagCatch);
                sIndex = size;
                Index = sIndex;
            }

            switch (sIndex)
            {
                case { } when sIndex > bufferHold[0].Length && size > Threshold:
                    bag = BagCatch = new();
                    ResetProperties(size);
                    break;

                case { } when size > bufferHold[0].Length && size > Threshold:
                    bag = BagCatch = new();
                    ResetProperties(size);
                    break;

                case { } when size > Threshold:
                    bag = BagCatch = new();
                    ResetProperties(size * 2);
                    break;

                case { } when sIndex > Threshold:
                    bag = BagCatch = new();
                    ResetProperties();
                    break;

                default:
                    break;
            }

            buffer = bufferHold[0];
        }

        buffer = bufferHold[0].AsSpan(sIndex -= size, size);

        return sIndex;
    }

    public (byte[] Data, BufferSlabEntry Entry) InsertDataAsync(byte[] data)
    {
        var currentIndex = GetStart(data.Length, out var bag, out var buffer);

        var entry = new BufferSlabEntry(currentIndex, data.Length);
        bag.TryAdd(entry, entry);
        data.ToArray().CopyTo(buffer);

        entry.Semaphore.Release();

        return (data, entry);
    }
}
