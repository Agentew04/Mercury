namespace SAAE.Engine.Common;

public readonly struct RegisterDefinition(int number, string name, int bitSize, bool isGeneralPurpose) {
    public int Number { get; } = number;
    public string Name { get; } = name;
    public int BitSize { get; } = bitSize;
    public bool IsGeneralPurpose { get; } = isGeneralPurpose;
}

public readonly struct Processor(int number, string name, RegisterDefinition[] registers, string[] flags) {
    public int Number { get; } = number;
    public string Name { get; } = name;
    public RegisterDefinition[] Registers { get; } = registers;
    public string[] Flags { get; } = flags;
}

public readonly struct ArchitectureMetadata {

    public ArchitectureMetadata(Processor[] processors) {
        Processors = processors;
    }
    
    // multiple coprocessors
    public readonly Processor[] Processors;
}

/// <summary>
/// A class to manage and gather metadata about the
/// different architectures available.
/// </summary>
public static class ArchitectureManager {
    private static readonly List<Architecture> AvailableArchitectures;
    private static readonly Dictionary<Architecture, ArchitectureMetadata> ArchitectureMetadata = [];

    static ArchitectureManager() {
        AvailableArchitectures = [
            Architecture.Mips, Architecture.Arm, Architecture.RiscV
        ];
        ArchitectureMetadata[Architecture.Mips] = InitMips();
    }

    /// <summary>
    /// Returns a readonly collection of all the currently supported architectures.
    /// </summary>
    public static IReadOnlyList<Architecture> GetAvailableArchitectures() => AvailableArchitectures;

    /// <summary>
    /// Returns the metadata of an architecture
    /// </summary>
    public static ArchitectureMetadata GetArchitectureMetadata(Architecture architecture) {
        return ArchitectureMetadata[architecture];
    }

    private static ArchitectureMetadata InitMips() {
        return new ArchitectureMetadata([
            new Processor(0, "Registers", [
                new RegisterDefinition(0, "zero", 32, false),
                new RegisterDefinition(1, "at", 32, false),
                // RETURN
                new RegisterDefinition(2, "v0", 32, true),
                new RegisterDefinition(3, "v1", 32, true),
                // ARGS
                new RegisterDefinition(4, "a0", 32, true),
                new RegisterDefinition(5, "a1", 32, true),
                new RegisterDefinition(6, "a2", 32, true),
                new RegisterDefinition(7, "a3", 32, true),
                // TX1
                new RegisterDefinition(8, "t0", 32, true),
                new RegisterDefinition(9, "t1", 32, true),
                new RegisterDefinition(10, "t2", 32, true),
                new RegisterDefinition(11, "t3", 32, true),
                new RegisterDefinition(12, "t4", 32, true),
                new RegisterDefinition(13, "t5", 32, true),
                new RegisterDefinition(14, "t6", 32, true),
                new RegisterDefinition(15, "t7", 32, true),
                // SX
                new RegisterDefinition(16, "s0", 32, true),
                new RegisterDefinition(17, "s1", 32, true),
                new RegisterDefinition(18, "s2", 32, true),
                new RegisterDefinition(19, "s3", 32, true),
                new RegisterDefinition(20, "s4", 32, true),
                new RegisterDefinition(21, "s5", 32, true),
                new RegisterDefinition(22, "s6", 32, true),
                new RegisterDefinition(23, "s7", 32, true),
                // TX2
                new RegisterDefinition(24, "t8", 32, true),
                new RegisterDefinition(25, "t9", 32, true),
                // KERNEL
                new RegisterDefinition(26, "k0", 32, false),
                new RegisterDefinition(27, "k1", 32, false),
                // STACK AND MEMORY
                new RegisterDefinition(28, "gp", 32, false),
                new RegisterDefinition(29, "sp", 32, false),
                new RegisterDefinition(30, "fp", 32, false),
                new RegisterDefinition(31, "ra", 32, false),
                // NO NUMBER
                new RegisterDefinition(-1, "pc", 32, false),
                new RegisterDefinition(-1, "hi", 32, false),
                new RegisterDefinition(-1, "lo", 32, false),
            ], []),
            new Processor(1, "Coproc 1", [
                new RegisterDefinition(0, "f0", 32, true),
                new RegisterDefinition(0, "f1", 32, true),
                new RegisterDefinition(0, "f2", 32, true),
                new RegisterDefinition(0, "f3", 32, true),
                new RegisterDefinition(0, "f4", 32, true),
                new RegisterDefinition(0, "f5", 32, true),
                new RegisterDefinition(0, "f6", 32, true),
                new RegisterDefinition(0, "f7", 32, true),
                new RegisterDefinition(0, "f8", 32, true),
                new RegisterDefinition(0, "f9", 32, true),
                new RegisterDefinition(0, "f10", 32, true),
                new RegisterDefinition(0, "f11", 32, true),
                new RegisterDefinition(0, "f12", 32, true),
                new RegisterDefinition(0, "f13", 32, true),
                new RegisterDefinition(0, "f14", 32, true),
                new RegisterDefinition(0, "f15", 32, true),
                new RegisterDefinition(0, "f16", 32, true),
                new RegisterDefinition(0, "f17", 32, true),
                new RegisterDefinition(0, "f18", 32, true),
                new RegisterDefinition(0, "f19", 32, true),
                new RegisterDefinition(0, "f20", 32, true),
                new RegisterDefinition(0, "f21", 32, true),
                new RegisterDefinition(0, "f22", 32, true),
                new RegisterDefinition(0, "f23", 32, true),
                new RegisterDefinition(0, "f24", 32, true),
                new RegisterDefinition(0, "f25", 32, true),
                new RegisterDefinition(0, "f26", 32, true),
                new RegisterDefinition(0, "f27", 32, true),
                new RegisterDefinition(0, "f28", 32, true),
                new RegisterDefinition(0, "f29", 32, true),
                new RegisterDefinition(0, "f30", 32, true),
                new RegisterDefinition(0, "f31", 32, true),
            ], [
                "0",
                "1",
                "2",
                "3",
                "4",
                "5",
                "6",
                "7"
            ]),
            new Processor(2, "Coproc 0", [
                new RegisterDefinition(8, "vaddr", 32, false),
                new RegisterDefinition(12, "status", 32, false),
                new RegisterDefinition(13, "cause", 32, false),
                new RegisterDefinition(14, "epc", 32, false),
            ], []),
        ]);
    }
}