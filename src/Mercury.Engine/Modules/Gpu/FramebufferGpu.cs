using Mercury.Engine.Common;
using Mercury.Engine.Common.Events;
using Mercury.Engine.Memory;
using Mercury.Engine.Modules.Gpu.Configs;
using Mercury.Engine.Modules.Gpu.Events;

namespace Mercury.Engine.Modules.Gpu;

/// <summary>
/// A module that functions as simple GPU with framebuffer. Just holds memory for a single frame and
/// exposes the buffer for a UI to show it.  
/// </summary>
/// <remarks>
/// It's actually not a real framebuffer, just a color buffer
/// </remarks>
public class FramebufferGpu : IConfigurableModule<FramebufferGpuConfig>, IDisposable {
    private EventBus eventBus = null!;
    private readonly List<IDisposable> subscriptions = [];

    private const int BytesPerPixel = 4; // RGBA8888
    private byte[] framebuffer = [];

    public ulong FramebufferAddress { get; private set; }
    public ulong FramebufferSize { get; private set; }
    public uint Width { get; private set; }
    public uint Height { get; private set; }

    public void Configure(FramebufferGpuConfig config) {
        FramebufferAddress = config.FramebufferBaseAddress;
        FramebufferSize = config.Width * config.Height * BytesPerPixel;
        Width = config.Width;
        Height = config.Height;
        framebuffer = new byte[FramebufferSize];
    }

    public void SubscribeToEvents(EventBus bus) {
        eventBus = bus;
        subscriptions.Add(bus.Subscribe<GpuWriteEvent>(HandleWrite));
    }

    public void Map(AddressDecoderModule decoder) {
        decoder.MapWriteOnlyDevice<GpuWriteEvent,FramebufferGpu>(new MemoryRange(FramebufferAddress, FramebufferSize));
    }

    public void UnsubscribeFromEvents() {
        foreach (IDisposable subscription in subscriptions) {
            subscription.Dispose();
        }

        subscriptions.Clear();
    }

    /// <summary>
    /// Unsubscribes from all <see cref="EventBus"/> callbacks.
    /// </summary>
    public void Dispose() {
        UnsubscribeFromEvents();
    }

    private void HandleWrite(GpuWriteEvent e) {
        ulong max = Math.Min(e.Address + e.Size, FramebufferSize);

        // just forward the data to gpu buffer
        for (ulong i = e.Address, j = 0; i < max; i++, j++) {
            framebuffer[i] = e.Buffer.Span[(int)j];
        }

        Console.Write("First row: ");
        foreach (byte b in framebuffer.AsMemory(0, (int)Width * 4).Span) {
            Console.Write($"{b} ");
        }
        Console.WriteLine();
    }

    /// <summary>
    /// Returns a <see cref="ReadOnlyMemory{byte}"/> that points to the GPU's internal framebuffer.
    /// </summary>
    public ReadOnlyMemory<byte> GetFramebufferReference() {
        return framebuffer.AsMemory(0, (int)FramebufferSize);
    }
}