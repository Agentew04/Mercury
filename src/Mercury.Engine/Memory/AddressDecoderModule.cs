using Mercury.Engine.Common;
using Mercury.Engine.Common.Events;
using Mercury.Engine.Memory.Events;

namespace Mercury.Engine.Memory;

/// <summary>
/// Module responsible from routing read and write events to
/// specific modules.
/// </summary>
public class AddressDecoderModule : IModule, IDisposable {
    private EventBus eventBus = null!;
    private readonly List<IDisposable> subscriptions = [];

    private readonly List<MemoryRange> ranges = [];

    // used to resolve multi-segment mapped address
    private readonly Dictionary<Type, List<MemoryRange>> groups = [];
    private readonly Dictionary<MemoryRange, ulong> lowestAddress = new();

    private readonly
        Dictionary<MemoryRange, (Action<EventBus, MemoryReadEvent,ulong>? read, Action<EventBus, MemoryWriteEvent,ulong>? write)>
        events = new();

    public void SubscribeToEvents(EventBus bus) {
        eventBus = bus;
        subscriptions.Add(bus.Subscribe<MemoryWriteEvent>(HandleWrite));
        subscriptions.Add(bus.Subscribe<MemoryReadEvent>(HandleRead));
    }

    public void UnsubscribeFromEvents() {
        foreach (IDisposable sub in subscriptions) {
            sub.Dispose();
        }

        subscriptions.Clear();
    }

    public void Dispose() {
        UnsubscribeFromEvents();
    }

    public void MapReadWriteDevice<TRead, TWrite, TId>(MemoryRange range)
        where TRead : IMemoryReadEvent, new() where TWrite : IMemoryWriteEvent, new() {
        ranges.Add(range);
        if(!groups.TryGetValue(typeof(TId), out List<MemoryRange>? groupRanges)){
            groupRanges = [];
            groups.Add(typeof(TId), groupRanges);
        }
        groupRanges.Add(range);
        events[range] = (TransformRead<TRead>, TransformWrite<TWrite>);
    }

    public void MapReadOnlyDevice<TRead, TId>(MemoryRange range) where TRead : IMemoryReadEvent, new() {
        ranges.Add(range);
        if(!groups.TryGetValue(typeof(TId), out List<MemoryRange>? groupRanges)){
            groupRanges = [];
            groups.Add(typeof(TId), groupRanges);
        }
        groupRanges.Add(range);
        events[range] = (TransformRead<TRead>, null);
    }

    public void MapWriteOnlyDevice<TWrite, TId>(MemoryRange range) where TWrite : IMemoryWriteEvent, new() {
        ranges.Add(range);
        if(!groups.TryGetValue(typeof(TId), out List<MemoryRange>? groupRanges)){
            groupRanges = [];
            groups.Add(typeof(TId), groupRanges);
        }
        groupRanges.Add(range);
        events[range] = (null, TransformWrite<TWrite>);
    }

    private static void TransformWrite<TWrite>(EventBus bus, MemoryWriteEvent e, ulong baseAddress) where TWrite : IMemoryWriteEvent, new() {
        TWrite newEvent = new() { Size = e.Size, Address = e.Address - baseAddress, Buffer = e.Buffer, };
        bus.Publish(newEvent);
    }
    
    private static void TransformRead<TRead>(EventBus bus, MemoryReadEvent e, ulong baseAddress) where TRead : IMemoryReadEvent, new(){
        TRead newEvent = new() { Size = e.Size, Address = e.Address - baseAddress, Buffer = e.Buffer, };
        bus.Publish(newEvent);
    }

    public void BuildMappings() {
        foreach (List<MemoryRange> group in groups.Values) {
            ulong lowest = ulong.MaxValue;
            foreach (MemoryRange range in group) {
                if (range.StartAddress < lowest) {
                    lowest = range.StartAddress;
                }
            }
            foreach (MemoryRange range in group) {
                lowestAddress[range] = lowest;
            }
        }
    }

    private void HandleWrite(MemoryWriteEvent e) {
        MemoryRange? correctRange = null;
        foreach (MemoryRange range in ranges) {
            if (range.Contains(e.Address)) {
                correctRange = range;
                break;
            }
        }

        if (correctRange == null) {
            eventBus.Publish(new RamWriteEvent() {
                Address = e.Address,
                Size = e.Size,
                Buffer = e.Buffer,
            });
            return;
        }

        (_, Action<EventBus, MemoryWriteEvent,ulong>? write) = events[correctRange.Value];
        write?.Invoke(eventBus, e, lowestAddress[correctRange.Value]);
    }

    private void HandleRead(MemoryReadEvent e) {
        MemoryRange? correctRange = null;
        foreach (MemoryRange range in ranges) {
            if (range.Contains(e.Address)) {
                correctRange = range;
                break;
            }
        }

        if (correctRange == null) {
            eventBus.Publish(new RamReadEvent() {
                Address = e.Address,
                Size = e.Size,
                Buffer = e.Buffer,
            });
            return;
        }

        (Action<EventBus, MemoryReadEvent,ulong>? read, _) = events[correctRange.Value];
        read?.Invoke(eventBus, e,lowestAddress[correctRange.Value]);
    }
}

public readonly record struct MemoryRange(ulong StartAddress, ulong Size) {
    public ulong EndAddress => StartAddress + Size;

    public bool Contains(ulong address) {
        return StartAddress <= address && address <= EndAddress;
    }
}