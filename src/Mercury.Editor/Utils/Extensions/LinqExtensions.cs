using System;
using System.Collections.Generic;
using System.Linq;

namespace Mercury.Editor.Extensions;

internal static class LinqExtensions {
    
    public static IEnumerable<T> ForEachExt<T>(this IEnumerable<T> source, Action<T> action) {
        IEnumerable<T> forEachExt = source as T[] ?? source.ToArray();
        foreach (T item in forEachExt) {
            action(item);
        }
        return forEachExt;
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