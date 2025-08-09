using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAAE.Engine.Mips.Runtime;

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

internal interface IAsyncClockable {
    /// <inheritdoc cref="IClockable.Clock"/>
    ValueTask ClockAsync();

    /// <inheritdoc cref="IClockable.IsClockingFinished"/>
    bool IsClockingFinished();
}