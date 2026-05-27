using Mercury.Engine.Memory;

namespace Mercury.Engine.Common;

/// <summary>
/// Shared interface for all modules that are used in a machine.
/// </summary>
public interface IModule {

    /// <summary>
    /// Subscribe this module to its relevant events from the event bus.
    /// </summary>
    /// <param name="bus">The event bus</param>
    public void SubscribeToEvents(EventBus bus);
    
    /// <summary>
    /// Unsubscribe this module from all events.
    /// </summary>
    public void UnsubscribeFromEvents();

    public void Map(AddressDecoderModule decoder) {}
}

public interface IConfigurableModule<in TConfig> : IModule {
    /// <summary>
    /// Receives a set of parameters and configures the module.
    /// </summary>
    /// <param name="config">The configuration parameters</param>
    public void Configure(TConfig config);
}