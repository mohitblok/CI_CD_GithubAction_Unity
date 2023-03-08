using System.Collections;
using System.Collections.Generic;

public static class HashSetExtentions
{
    public static void AddRange<T, C>(this HashSet<T> myHashSet, C otherHashSet) where C : IEnumerable
    {
        foreach (T elementToAdd in otherHashSet)
        {
            myHashSet.Add(elementToAdd);
        }
    }
}