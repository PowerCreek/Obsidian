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
        await foreach ((var bytes, var map) in DataQueue.Reader.ReadAllAsync().WithCancellation(cancellationToken)) {
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
    }

    int Index = 0;
    public int Threshold { get; set; }
    public ManualResetEventSlim Reset = new(false);

    public bool BagFull = false;
    public byte[][] CurrentBuffer;
    public ConcurrentDictionary<BufferSlabEntry, BufferSlabEntry> BagCatch = new();

    public BufferSlab(int threshold)
    {
        Threshold = threshold;
        CreateBuffer();
        WriteMemory(CurrentBuffer, BagCatch);
    }

    public void CreateBuffer(int? threshold = null)
    {
        var bytes = new byte[threshold??Threshold];
        CurrentBuffer = new byte[][] { bytes };
    }

    public void WriteMemory(byte[][] buffer, ConcurrentDictionary<BufferSlabEntry, BufferSlabEntry> bagCatch)
    {
        DataQueue.Writer.TryWrite((new Memory<byte>(buffer[0]), bagCatch));
    }

    public int GetStart(int size, out ConcurrentDictionary<BufferSlabEntry, BufferSlabEntry> bag, out byte[] buffer)
    {
        var sIndex = 0;

        lock (Reset)
        {
            switch ((sIndex = Index += size))
            {
                case { } when sIndex > CurrentBuffer[0].Length && size > Threshold:
                    CreateBuffer(size);
                    BagCatch = new();
                    WriteMemory(CurrentBuffer, BagCatch);
                    sIndex = size;
                    Index = sIndex;
                    break;

                case { } when size > CurrentBuffer[0].Length:
                    CreateBuffer(size);
                    BagCatch = new();
                    WriteMemory(CurrentBuffer, BagCatch);
                    sIndex = size;
                    Index = sIndex;
                    break;

                case { } when size > Threshold:
                    CreateBuffer(size * 2);
                    BagCatch = new();
                    WriteMemory(CurrentBuffer, BagCatch);
                    sIndex = size;
                    Index = sIndex;
                    break;

                case { } when sIndex > Threshold:
                    CreateBuffer();
                    BagCatch = new();
                    WriteMemory(CurrentBuffer, BagCatch);
                    sIndex = size;
                    Index = sIndex;
                    break;

                default:
                    break;
            }

            buffer = CurrentBuffer[0];
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
