namespace Mercury.Engine.Modules.Gpu.Configs;

public record FramebufferGpuConfig {
    public required ulong FramebufferBaseAddress { get; init; }
    public required uint Width { get; init; }
    public required uint Height { get; init; }
}