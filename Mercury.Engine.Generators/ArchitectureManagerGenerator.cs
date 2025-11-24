using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Mercury.Generators;

internal readonly record struct FlagsInfo {
    public readonly string Architecture;
    public readonly string EnumTypeName;
    public readonly int FlagCount;

    public FlagsInfo(string architecture, string enumTypeName, int flagCount) {
        Architecture = architecture;
        EnumTypeName = enumTypeName;
        FlagCount = flagCount;
    }
}

internal readonly record struct BankInfo {
    public readonly string Architecture;
    public readonly int Coprocessor;
    public readonly EquatableArray<RegisterDef> Registers;
    public readonly string EnumTypeName;

    public BankInfo(string architecture, int coprocessor, string type, List<RegisterDef> registers) {
        Architecture = architecture;
        Coprocessor = coprocessor;
        Registers = new EquatableArray<RegisterDef>(registers.ToArray());
        EnumTypeName = type;
    }
}

internal readonly record struct ArchitecturesInfo {
    public readonly string EnumFullname;
    public readonly EquatableArray<string> Architectures;

    public ArchitecturesInfo(string fullname, List<string> architectures) {
        EnumFullname = fullname;
        Architectures = new EquatableArray<string>(architectures.ToArray());
    }
}

[Generator]
public class ArchitectureManagerGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext ctx) {
        ctx.RegisterPostInitializationOutput(pic => {
            pic.AddSource("ArchitectureMetadata.g.cs", MetadataText);
            pic.AddSource("Processor.g.cs", ProcessorText);
            pic.AddSource("RegisterDefinition.g.cs", RegisterDefinitionText);
            pic.AddSource("ArchitectureAttribute.g.cs", ArchitectureAttributeText);
            pic.AddSource("InvalidAttribute.g.cs", InvalidAttributeText);
            pic.AddSource("ProcessorFlagsAttribute.g.cs", ProcessorFlagsText);
        });

        IncrementalValueProvider<ImmutableArray<BankInfo>> banks = ctx.SyntaxProvider.ForAttributeWithMetadataName(
                fullyQualifiedMetadataName: "Mercury.Generators.RegisterBankDefinitionAttribute",
                predicate: static (_, _) => true,
                transform: static (ctx, _) => GetRegisterBank(ctx))
            .Collect();

        IncrementalValueProvider<ImmutableArray<ArchitecturesInfo>> arch = ctx.SyntaxProvider.ForAttributeWithMetadataName(
                fullyQualifiedMetadataName: "Mercury.Generators.ArchitectureAttribute",
                predicate: static (_, _) => true,
                transform: static (ctx, _) => GetArchitecture(ctx))
            .Collect();

        IncrementalValueProvider<ImmutableArray<FlagsInfo>> flags = ctx.SyntaxProvider.ForAttributeWithMetadataName(
                fullyQualifiedMetadataName: "Mercury.Generators.ProcessorFlagsAttribute",
                predicate: static (_, _) => true,
                transform: static (ctx, _) => GetFlags(ctx))
            .Collect();

        IncrementalValueProvider<(ImmutableArray<BankInfo> Left, ImmutableArray<ArchitecturesInfo> Right)> data1 = banks.Combine(arch);

        IncrementalValueProvider<((ImmutableArray<BankInfo> Left, ImmutableArray<ArchitecturesInfo> Right) Left, ImmutableArray<FlagsInfo> Right)> data2 = data1.Combine(flags);
        

        ctx.RegisterSourceOutput(data2,
            (spc, source) => Execute(source, spc));
    }

    private static BankInfo GetRegisterBank(GeneratorAttributeSyntaxContext ctx) {
        // get coprocessor number
        int coprocessor = (int)ctx.Attributes[0].NamedArguments.First(x => x.Key == "Processor").Value.Value!;
        
        // get architecture name
        int archIndex = (int)ctx.Attributes[0].ConstructorArguments[0].Value!;
        string architecture = ctx.Attributes[0].ConstructorArguments[0].Type!.GetMembers()
            .OfType<IFieldSymbol>()
            .FirstOrDefault(x => x.HasConstantValue && (int)x.ConstantValue! == archIndex)?.Name ?? "null";
        
        string enumTypeName = ctx.TargetSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        List<RegisterDef> registers = [];
        
        EnumDeclarationSyntax eds = (EnumDeclarationSyntax)ctx.TargetNode;
        foreach (EnumMemberDeclarationSyntax member in eds.Members) {
            IFieldSymbol? field = (IFieldSymbol?)ctx.SemanticModel.GetDeclaredSymbol(member);

            AttributeData? attribute = field?.GetAttributes()
                .FirstOrDefault(x => x.AttributeClass!.ToDisplayString() == "Mercury.Generators.RegisterAttribute");
            if (attribute is null || field is null) {
                continue;
            }

            int number = -1;
            int index = 0;
            if (attribute.ConstructorArguments.Length == 4) {
                number = (int)attribute.ConstructorArguments[0].Value!;
                index++;
            }
            
            registers.Add(new  RegisterDef(
                field.ToDisplayString(),
                (string)attribute.ConstructorArguments[index].Value!,
                number != -1,
                number,
                (int)attribute.ConstructorArguments[index+1].Value!,
                (bool)attribute.ConstructorArguments[index+2].Value!
                ));
        }
        
        return new BankInfo(architecture, coprocessor, enumTypeName, registers);
    }

    private static ArchitecturesInfo GetArchitecture(GeneratorAttributeSyntaxContext ctx) {
        string fullname = ctx.TargetSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        
        // pegar campos do enum
        EnumDeclarationSyntax eds = (EnumDeclarationSyntax)ctx.TargetNode;
        List<string> archs = [];
        foreach (EnumMemberDeclarationSyntax member in eds.Members) {
            IFieldSymbol? field = (IFieldSymbol?)ctx.SemanticModel.GetDeclaredSymbol(member);
            if (field is null) {
                continue;
            }
            
            // check if is marked as invalid
            bool invalid = field.GetAttributes()
                .Any(x => x.AttributeClass!.ToDisplayString() == "Mercury.Generators.InvalidAttribute");
            if (!invalid) {
                archs.Add(field.Name);
            }
        }
        return new ArchitecturesInfo(fullname, archs);
    }

    private static FlagsInfo GetFlags(GeneratorAttributeSyntaxContext ctx) {
        
        // get architecture name
        int archIndex = (int)ctx.Attributes[0].ConstructorArguments[0].Value!;
        string architecture = ctx.Attributes[0].ConstructorArguments[0].Type!.GetMembers()
            .OfType<IFieldSymbol>()
            .FirstOrDefault(x => x.HasConstantValue && (int)x.ConstantValue! == archIndex)?.Name ?? "null";
        
        string enumTypeName = ctx.TargetSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        
        EnumDeclarationSyntax eds = (EnumDeclarationSyntax)ctx.TargetNode;
        int flagCount = eds.Members.Count;
        
        return new FlagsInfo(architecture, enumTypeName, flagCount);
    }
    
    private static void Execute(((ImmutableArray<BankInfo> Left, ImmutableArray<ArchitecturesInfo> Right) Left, ImmutableArray<FlagsInfo> Right) data, SourceProductionContext ctx) {
        if (data.Right.Length != 1) {
            return;
        }

        StringBuilder sbArchArray = new();
        foreach (string arch in data.Left.Right[0].Architectures) {
            sbArchArray.AppendLine($"            {data.Left.Right[0].EnumFullname}.{arch},");
        }
        
        StringBuilder sbInitCalls = new();
        foreach (string arch in data.Left.Right[0].Architectures) {
            sbInitCalls.AppendLine($"        ArchitectureMetadata[{data.Left.Right[0].EnumFullname}.{arch}] = Init{arch}();");
        }
        
        StringBuilder sbInitFunctions = new();
        foreach (string arch in data.Left.Right[0].Architectures) {
            sbInitFunctions.AppendLine($"    private static ArchitectureMetadata Init{arch}() {{");
            List<BankInfo> banks = data.Left.Left.Where(x => x.Architecture == arch).ToList();
            IEnumerable<IGrouping<int, BankInfo>> grouped = banks.GroupBy(x => x.Coprocessor);
            if (banks.Count == 0) {
                sbInitFunctions.AppendLine("        return new ArchitectureMetadata([]);\n    }\n");
                continue;
            }
            
            sbInitFunctions.AppendLine("        return new ArchitectureMetadata([");
            foreach (IGrouping<int, BankInfo>? group in grouped) {
                sbInitFunctions.Append("            new Processor(");
                sbInitFunctions.Append(group.Key);
                sbInitFunctions.Append(", \"");
                sbInitFunctions.Append(group.Key == 0 ? "Registers" : $"Coproc {group.Key}");
                sbInitFunctions.AppendLine("\", [");

                foreach (BankInfo bank in group) {
                    foreach (RegisterDef reg in bank.Registers) {
                        sbInitFunctions.AppendLine(string.Format(RegisterInitializationText,
                            reg.HasNumber ? reg.Number.ToString() : "-1",
                            reg.Name,
                            reg.Size.ToString(),
                            reg.IsGpr ? "true" : "false",
                            bank.EnumTypeName,
                            reg.EnumMemberName
                        ));
                    }
                }
                sbInitFunctions.AppendLine("            ], [");
                FlagsInfo flags = data.Right.FirstOrDefault(x => x.Architecture == arch);
                if (flags != default) {
                    for(int i=0;i<flags.FlagCount;i++) {
                        sbInitFunctions.AppendLine($"                \"{i}\",");
                    }
                }
                // no flags for now
                sbInitFunctions.AppendLine("            ]),");
            }
            sbInitFunctions.AppendLine("        ]);");
            sbInitFunctions.AppendLine("    }");
            sbInitFunctions.AppendLine();
        }

        /*
         * 0: arch array
         * 1: call inits
         * 2: init functions
         */
        string text = string.Format(ManagerText,
            sbArchArray,
            sbInitCalls,
            sbInitFunctions
        );
        
        ctx.AddSource("ArchitectureManager.g.cs", SourceText.From(text, Encoding.UTF8));
        
    }
    
    #region Templates

    private const string ManagerText =
        """
        // <auto-generated />
        
        using System;
        
        namespace Mercury.Engine.Common;
        
        /// <summary>
        /// Static class that holds all metadata about the different available architectures.
        /// </summary>
        public static class ArchitectureManager {{
            private static readonly List<Architecture> AvailableArchitectures;
            private static readonly Dictionary<Architecture, ArchitectureMetadata> ArchitectureMetadata = [];
            /// <summary>
            /// Returns a readonly collection of all the currently supported architectures.
            /// </summary>
            public static IReadOnlyList<Architecture> GetAvailableArchitectures() => AvailableArchitectures;
            
            /// <summary>
            /// Returns the metadata of an architecture.
            /// </summary>
            public static ArchitectureMetadata GetArchitectureMetadata(Architecture architecture) {{
                return ArchitectureMetadata[architecture];
            }}
            
            /// <summary>
            /// Static constructor to initialize the available architectures and their metadata.
            /// </summary>
            static ArchitectureManager() {{
                // list with recognized architectures
                AvailableArchitectures = [
        {0}
                ];
                // initialization of metadata for each architecture
        {1}
            }}
           
            #region Initializers 
        {2}
            #endregion
        }}
        """;
    
    private const string RegisterInitializationText =
        """
                        new RegisterDefinition({0}, "{1}", {2}, {3}, typeof({4}), {5}),
        """;
    
    private const string MetadataText =
        """
        // <auto-generated />
        
        using System;
        
        namespace Mercury.Engine.Common;
        
        /// <summary>
        /// A struct to represent metadata about a register.
        /// </summary>
        public record ArchitectureMetadata(Processor[] processors) {
            public Processor[] Processors { get; } = processors;
        }
        """;

    private const string ProcessorText =
        """
        // <auto-generated />
        using System;
        
        namespace Mercury.Engine.Common;
        
        /// <summary>
        /// Represents a processor inside an architecture.
        /// </summary>
        public record Processor(int number, string name, RegisterDefinition[] registers, string[] flags) {
            public int Number { get; } = number;
            public string Name { get; } = name;
            public RegisterDefinition[] Registers { get; } = registers;
            public string[] Flags { get; } = flags;
        }
        """;
    
    private const string RegisterDefinitionText =
        """
        // <auto-generated />
        
        using System;
        
        namespace Mercury.Engine.Common;
        
        /// <summary>
        /// A struct to represent a register inside a processor.
        /// </summary>
        public record RegisterDefinition(int number, string name, int bitSize, bool isGeneralPurpose, Type type, Enum reference) {
            public int Number { get; } = number;
            public string Name { get; } = name;
            public int BitSize { get; } = bitSize;
            public bool IsGeneralPurpose { get; } = isGeneralPurpose;
            public Type Type { get; } = type;
            public Enum Reference { get; } = reference;
        }
        """;

    private const string ArchitectureAttributeText =
        """
        // <auto-generated />
        using System;
        
        namespace Mercury.Generators;
        
        [AttributeUsage(AttributeTargets.Enum, Inherited = false, AllowMultiple = false)]
        public class ArchitectureAttribute : Attribute {
            public ArchitectureAttribute() {
            }
        }
        
        """;
    
    private const string InvalidAttributeText =
        """
        // <auto-generated />
        using System;

        namespace Mercury.Generators;

        /// <summary>
        /// Attribute that marks a field of an enum as not
        /// being a valid architecture value.
        /// </summary>
        [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
        public class InvalidAttribute : Attribute {
            public InvalidAttribute() {
            }
        }
        """;

    private const string ProcessorFlagsText =
        """
        // <auto-generated />
        using System;
        
        namespace Mercury.Generators;
        
        /// <summary>
        /// Attribute that marks an enum as being the definition of flags that a 
        /// coprocessor has.
        /// </summary>
        [AttributeUsage(AttributeTargets.Enum, Inherited = false, AllowMultiple = false)]
        public class ProcessorFlagsAttribute : Attribute {
            public ProcessorFlagsAttribute(Mercury.Engine.Common.Architecture architecture) {
            }
            public required int Processor { get; init; }
        }
        """;

    #endregion Templates
}