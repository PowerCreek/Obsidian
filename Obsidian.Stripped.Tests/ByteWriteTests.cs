using Shouldly;
using System.Diagnostics;
using Xunit.Abstractions;

namespace Obsidian.Tests;
public class ByteWriteTests
{

    [Fact(DisplayName = "WriteSlab")]
    public async Task Test()
    {
        var Slab = new BufferSlab(1300);
        byte[] ITEMS = new byte[1300].Select((e) => (byte)new Random().Next()).ToArray();

        var byteArray = new byte[0];
        Array.Fill(byteArray, (byte)1);

        var check = new List<byte[]>();
        
        for (int i = 0; i < 405; i += 2)
        {
            check.Add(byteArray = new byte[Math.Min(i + 50, ITEMS.Length)]);
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

        for(int i = 0; i < check.Count; i++)
        {
            var ValueA = check[i];
            var ValueB = items[i];
            var valid = ValueB.SequenceEqual(ValueA);
        }

        items.ShouldBeEquivalentTo(check);
    }
}
