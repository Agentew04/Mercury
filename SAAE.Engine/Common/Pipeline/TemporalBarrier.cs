namespace SAAE.Engine.Common.Pipeline;

/// <summary>
/// Represents a logical temporar barrier in a pipeline context.
/// </summary>
/// <typeparam name="T">A type that encompasses all information shared between two stages</typeparam>
public class TemporalBarrier<T> {

    private T next;
    private T current;
    private bool hasNext;
    private bool hasValue;
    
    /// <summary>
    /// Indicates if there is a value written to this barrier
    /// that has not yet been commited.
    /// </summary>
    public bool HasNext => hasNext;

    /// <summary>
    /// Indicates if the barrier currently has a value that can be read
    /// with the <see cref="Read"/> method.
    /// </summary>
    public bool HasValue => hasValue;

    /// <summary>
    /// Writes a value to this barrier. The value will be available
    /// when <see cref="Commit"/> is called.
    /// </summary>
    /// <param name="value">The new value</param>
    public void Write(T value) {
        next = value;
        hasNext = true;
    }

    /// <summary>
    /// Reads the current value that this barrier is
    /// outputting.
    /// </summary>
    public T Read() => current;

    /// <summary>
    /// Advances the barrier, commiting the value written
    /// with <see cref="Write"/>, making it available to be read
    /// with <see cref="Read"/>. If no value
    /// was written, this method does nothing.
    /// </summary>
    public void Commit() {
        if (!hasNext) {
            hasValue = false;
            return;
        }
        current = next;
        hasNext = false;
        hasValue = true;
    }
}