﻿using Obsidian.Entities;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace Obsidian.Stripped.Utilities;

public class BufferSlab
{
    public Channel<(byte[] Data, ConcurrentDictionary<BufferSlabEntry, BufferSlabEntry> Bag)> DataQueue =
        Channel.CreateUnbounded<(byte[] Data, ConcurrentDictionary<BufferSlabEntry, BufferSlabEntry> Bag)>();

    public async IAsyncEnumerable<Memory<byte>> GetData([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach ((var data, var map) in DataQueue.Reader.ReadAllAsync().WithCancellation(cancellationToken))
        {
            for (var entry = map[new (0)]; ;)
            {
                Debug.WriteLine(entry.UUID);
                await entry.Sem.WaitAsync();
                yield return new Memory<byte>(data, entry.Position, entry.Size);

                if (map.TryGetValue(entry with
                {
                    Position = entry.Position + entry.Size,
                }, out entry)) continue;
                break;
            }
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
