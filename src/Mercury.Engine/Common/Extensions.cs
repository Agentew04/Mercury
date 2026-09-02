using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Channels;

namespace Mercury.Engine.Common; 
public static class Extensions {

    public static IEnumerable<IEnumerable<T>> Split<T>(this IEnumerable<T> collection, T splitter) {
        List<T> currentList = [];
        foreach (T item in collection) {
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
        foreach (T item in collection) {
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
        using IEnumerator<T> enumerator = enumerable.GetEnumerator();
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
    public static T? Before<T>(this IEnumerable<T?> enumerable, T item) {
        ArgumentNullException.ThrowIfNull(enumerable);

        using IEnumerator<T?> enumerator = enumerable.GetEnumerator();
        T? previous = default;
        bool isFirstItem = true;

        while (enumerator.MoveNext()) {
            T? curr = enumerator.Current;
            if (curr is null) {
                continue;
            }
            if (EqualityComparer<T>.Default.Equals(curr,item)) {
                if (isFirstItem)
                    throw new InvalidOperationException("The item is the first in the collection.");

                return previous;
            }

            previous = enumerator.Current;
            isFirstItem = false;
        }

        throw new InvalidOperationException("The item is not in the collection.");
    }
    
    /// <summary>
    /// Extension method to easily write a string to a <see cref="Channel"/> with T being <see cref="char"/>.
    /// </summary>
    /// <param name="writer">The channel writer</param>
    /// <param name="value">The string to write to the channel</param>
    /// <param name="cancellationToken">An optional cancellation token to cancel de method</param>
    public static async ValueTask WriteAsync(this ChannelWriter<char> writer, string value, CancellationToken cancellationToken = default) {
        for (int i = 0; i < value.Length && !cancellationToken.IsCancellationRequested; i++) {
            await writer.WriteAsync(value[i], cancellationToken);
        }
    }

    /// <summary>
    /// Reads a line from a channel of chars. The line is read until a newline character ('\n') or an
    /// EOF character (0x1A) is encountered. The method returns the line as a string, including the newline character
    /// if it was present. If the first character read is an EOF, it returns an empty string. The method will wait for
    /// data to be available in the channel before reading, and it can be canceled using the provided cancellation token.
    /// </summary>
    /// <param name="reader">A reader from the channel that will be used</param>
    /// <param name="cancellationToken">Token to cancel the async operation</param>
    /// <returns>A task with the resulting line read</returns>
    public static async ValueTask<string> ReadLine(this ChannelReader<char> reader,
        CancellationToken cancellationToken = default) {
        await reader.WaitToReadAsync(cancellationToken);
        // ok, agora tem valores para ler
        StringBuilder sb = new();
        char read = await reader.ReadAsync(cancellationToken);
        sb.Append(read != (char)0x1A ? read : '\0'); // if first char is eof, append end of string
        while (read != '\n' && read != (char)0x1A/*EOF*/) {
            read = await reader.ReadAsync(cancellationToken);
            sb.Append(read);
        }
        return sb.ToString();
    }

    /// <summary>
    /// Sign-extends a number using a number of input bits.
    /// </summary>
    /// <param name="value">The input value</param>
    /// <param name="bitCount">How many bits are valid</param>
    /// <typeparam name="T">The type of the number to sign extend</typeparam>
    /// <returns>The value sign extended.</returns>
    public static T SignExtend<T>(this T value, int bitCount) where T: IBinaryInteger<T> {
        int size = Unsafe.SizeOf<T>();
        int shift = size - bitCount;
        return (value << shift) >> shift;
    }
}
