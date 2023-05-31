using Obsidian.Stripped.Extensions;

namespace Obsidian.Stripped.Utilities;

public class Indexer
{
    public HashSet<int> Used = new();
    public HashSet<int?> Unused = new();

    public int GetAvailableTag()
    {
        var result = -1;
        int GetMaxTag()
        {
            lock (Used)
            {
                var count = Used.Count;
                Used.Add(count);
                return count;
            }
        };

        Unused.TryTake(out var unusedItem);

        if (unusedItem is int value)
        {
            result = value;
            Used.Add(result);
        }
        else
            result = GetMaxTag();

        return result;
    }

    public void ResetTag(int tag)
    {
        var numberRemovedItems = 0;
        lock (Used)
        {
            numberRemovedItems = Used.RemoveWhere(t => t == tag);
            if (numberRemovedItems is 1)
                Unused.Add(tag);
        }
    }

    public string GetContents()
    {
        var str = string.Join("\n",
                              string.Join(" ", Used.ToArray()),
                             string.Join(" ", Unused.ToArray()));
        return str;
    }
}
