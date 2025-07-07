using System;
using System.Collections.Generic;

namespace SAAE.Editor.Extensions;

internal static class LinqExtensions {
    
    public static IEnumerable<T> ForEach<T>(this IEnumerable<T> source, System.Action<T> action) {
        foreach (T item in source) {
            action(item);
            yield return item;
        }
    }
    
    public static int IndexOf<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(predicate);

        int index = 0;
        foreach (T item in source)
        {
            if (predicate(item))
            {
                return index;
            }
            index++;
        }
        return -1; // Not found
    }
}