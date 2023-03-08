using System.Collections.Generic;
using System.Linq;

public static class ListExtensions
{
    public static void RemoveDuplicates<T>(this List<T> list)
    {
        list = list.Distinct().ToList();
    }

    public static void ReplaceDuplicatesWithNull<T>(this List<T> list)
    {
        HashSet<T> seen = new HashSet<T>();
        for (int i = 0; i < list.Count; i++)
        {
            T current = list[i];
            if (seen.Contains(current))
            {
                list[i] = default;
            }
            else
            {
                seen.Add(current);
            }
        }
    }

    public static void RemoveNulls<T>(this List<T> list) where T : class
    {
        list.RemoveAll(item => item == null);
    }

    public static void RemoveIfExists<T>(this List<T> list, T item)
    {
        if(list.Contains(item))
        {
            list.Remove(item);
        }
    }

    public static void RemoveRangeIfExists<T>(this List<T> list, List<T> collection)
    {
        collection.ForEach(item => list.RemoveIfExists(item));
    }
}
