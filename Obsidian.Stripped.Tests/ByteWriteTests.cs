using Obsidian.Stripped.Utilities.Collections;
using Shouldly;
using System.Collections.Concurrent;

namespace Obsidian.Stripped.Tests;
public class ByteWriteTests
{

    [Fact(DisplayName = "WriteSlab")]
    public async Task Test()
    {
        var Slab = new BufferSlab(3000);
        var ITEMS = new byte[2300].Select((e) => (byte)new Random().Next()).ToArray();

        var itemMap = new ConcurrentDictionary<int, (byte[] Data, BufferSlabEntry Entry)>();
        var mre = new ManualResetEventSlim(false);
        var completionSource = new TaskCompletionSource();
        var count = 0;
        for (var i = 0; i < 2500; i += 2)
        {
            var insertArray = new byte[Math.Min(i + 50, ITEMS.Length)];
            ITEMS.AsSpan(0, insertArray.Length).ToArray().CopyTo(insertArray, 0);
            //await  Task.Run(()=> Slab.InsertDataAsync(byteArray));
            _ = Task.Run(() => {

                itemMap.TryAdd(count++, Slab.InsertDataAsync(insertArray));

            });
            if (insertArray.Length >= 2300)
                continue;
        }

        var items = new List<byte[]>();
        try
        {
            await foreach (var v in Slab.GetData().WithCancellation(new CancellationTokenSource(1000).Token))
                items.Add(v.ToArray());
            ;
        }
        catch (Exception ex)
        {

        }

        var check = itemMap.OrderBy(a=>a.Key).Select(a=>a.Value.Data).ToList();
        items.SequenceEqual(check);        
        //for (var i = 0; i < check.Count; i++)
        //{
        //    var ValueA = check[i];
        //    var ValueB = items[i];
        //    var valid = ValueB.SequenceEqual(ValueA);
        //}

        //items.ShouldBeEquivalentTo(check);
    }
}
