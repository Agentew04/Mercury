using System.Text.Json.Serialization;
using SAAE.Engine.Mips.Runtime;
using SAAE.Generators;

namespace SAAE.Engine.Common;

/// <summary>
/// An enumerator that lists all available Instruction Set Architectures
/// for the engine.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<Architecture>))]
[Architecture]
public enum Architecture {
    [Invalid]
    [JsonStringEnumMemberName("unknown")]
    Unknown,
    [JsonStringEnumMemberName("mips")]
    Mips,
    [JsonStringEnumMemberName("riscv")]
    RiscV,
    [JsonStringEnumMemberName("arm")]
    Arm
}
