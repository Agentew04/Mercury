namespace Mercury.Engine.Common.Builders;

/// <summary>
/// Interface that defines a builder pattern for creating instances of type T.
/// </summary>
/// <typeparam name="T">The type that is being created</typeparam>
public interface IBuilder<out T>
{
    /// <summary>
    /// Creates an instance of type T based on the configuration set in the builder.
    /// </summary>
    /// <returns></returns>
    T Build();
}