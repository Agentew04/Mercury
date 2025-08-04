using System.Text.Json.Serialization;

namespace SAAE.Engine;

/// <summary>
/// An enumerator that lists all available Instruction Set Architectures
/// for the engine.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<Architecture>))]
public enum Architecture {
    [JsonStringEnumMemberName("unknown")]
    Unknown,
    [JsonStringEnumMemberName("mips")]
    Mips,
    [JsonStringEnumMemberName("riscv")]
    RiscV,
    [JsonStringEnumMemberName("arm")]
    Arm
}