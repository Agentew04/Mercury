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

    /// <summary>
    /// Serializes all data from this module in a way that when <see cref="Deserialize"/> is called,
    /// it continues execution from the same past point.
    /// </summary>
    /// <remarks>
    /// Any serialization method may be used, but binary (with <see cref="BinaryWriter"/>) is recommended
    /// to ensure maximum space efficiency.
    /// </remarks>
    /// <param name="s">An empty stream that implementer must fill with data</param>
    public void Serialize(Stream s);

    /// <summary>
    /// Deserializes custom state data for this module from a stream.
    /// </summary>
    /// <param name="s">A stream with data</param>
    public void Deserialize(Stream s);
}

public interface IConfigurableModule<in TConfig> : IModule {
    /// <summary>
    /// Receives a set of parameters and configures the module.
    /// </summary>
    /// <param name="config">The configuration parameters</param>
    public void Configure(TConfig config);
}