using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAAE.Engine; 
public static class Extensions {

    public static IEnumerable<IEnumerable<T>> Split<T>(this IEnumerable<T> collection, T splitter) {
        List<T> currentList = [];
        foreach (var item in collection) {
            if(EqualityComparer<T>.Default.Equals(item, splitter)) {
                yield return new List<T>(currentList);
                currentList.Clear();
            } else {
                currentList.Add(item);
            }
        }
        if (currentList.Count > 0) {
            yield return new List<T>(currentList);
        }
    }

    public static IEnumerable<IEnumerable<T>> Split<T>(this IEnumerable<T> collection, Predicate<T> splitPredicate) {
        List<T> currentList = [];
        foreach (var item in collection) {
            if (splitPredicate.Invoke(item)) {
                yield return new List<T>(currentList);
                currentList.Clear();
            } else {
                currentList.Add(item);
            }
        }
        if (currentList.Count > 0) {
            yield return new List<T>(currentList);
        }
    }

    /// <summary>
    /// Returns the next item after the current one in the enumerator.
    /// </summary>
    /// <typeparam name="T">The type of the items</typeparam>
    /// <param name="enumerable">The collection</param>
    /// <param name="item">The current analyzed item.</param>
    /// <returns>The next one or</returns>
    /// <exception cref="InvalidOperationException">If the item is not in the collection or is the last one.</exception>"
    public static T After<T>(this IEnumerable<T> enumerable, T item) {
        var enumerator = enumerable.GetEnumerator();
        while (enumerator.MoveNext()) {
            if (EqualityComparer<T>.Default.Equals(enumerator.Current, item) && enumerator.MoveNext()) {
                return enumerator.Current;
            }
        }
        throw new InvalidOperationException("The item is not in the collection or is the last one.");
    }

    /// <summary>
    /// Finds the item that comes before the current one in the collection.
    /// </summary>
    /// <typeparam name="T">The type of the items</typeparam>
    /// <param name="enumerable">The array to iterate</param>
    /// <param name="item">The current item</param>
    /// <returns>The item before</returns>
    /// <exception cref="InvalidOperationException">If the item is not in the collection or is the first one.</exception>"
    public static T Before<T>(this IEnumerable<T> enumerable, T item) {
        if (enumerable == null)
            throw new ArgumentNullException(nameof(enumerable));

        if (!enumerable.Contains(item))
            throw new InvalidOperationException("The item is not in the collection.");

        using (var enumerator = enumerable.GetEnumerator()) {
            T previous = default;
            bool isFirstItem = true;

            while (enumerator.MoveNext()) {
                if (enumerator.Current.Equals(item)) {
                    if (isFirstItem)
                        throw new InvalidOperationException("The item is the first in the collection.");

                    return previous;
                }

                previous = enumerator.Current;
                isFirstItem = false;
            }
        }
        throw new InvalidOperationException("The item is not in the collection.");
    }
}
