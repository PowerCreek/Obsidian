namespace Obsidian.Stripped.Extensions;

public static class HashSetExt
{
    public static bool TryTake<T>(this HashSet<T> collection, out T item)
    {
        lock (collection)
        {
            using var enumerator = collection.GetEnumerator();

            enumerator.MoveNext();
            var element = enumerator.Current;

            var removed = collection.Remove(element);

            item = element;

            return removed;
        }
    }
}
