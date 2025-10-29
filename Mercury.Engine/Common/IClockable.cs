namespace Mercury.Engine.Common;

/// <summary>
/// Defines that a class can be clocked.
/// </summary>
internal interface IClockable {
    
    /// <summary>
    /// Executes the code referent to one clock cycle.
    /// </summary>
    void Clock();
    
    /// <summary>
    /// Defines if the clocking is finished or not.
    /// For example: nothing more will be done with clocking continues.
    /// </summary>
    bool IsClockingFinished();
}

public interface IAsyncClockable {
    /// <inheritdoc cref="IClockable.Clock"/>
    ValueTask ClockAsync();

    /// <inheritdoc cref="IClockable.IsClockingFinished"/>
    bool IsClockingFinished();
}