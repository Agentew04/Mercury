namespace Mercury.Engine.Common;

/// <summary>
/// Interface that defines methods all instruction extensions should have.
/// </summary>
/// <example>
/// SIMD instructions should be implemented with a
/// <code>MipsSimdExtension.Decode(binary);</code>
/// while the main CPU module, when encountering an unknown
/// instruction, should first check in all installed instruction extensions.
/// </example>
public interface IInstructionExtension {
    public IInstruction? Decode(uint binary);
}