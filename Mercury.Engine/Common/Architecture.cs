using System.Text.Json.Serialization;
using Mercury.Engine.Mips.Runtime;

namespace Mercury.Engine.Common;

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
