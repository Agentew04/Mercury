using System.Collections.Generic;
using System.Linq;
using SAAE.Editor.Models;

namespace SAAE.Editor;

internal static class LinqExtensions {
    
    public static IEnumerable<T> ForEach<T>(this IEnumerable<T> source, System.Action<T> action) {
        foreach (T item in source) {
            action(item);
            yield return item;
        }
    }
}