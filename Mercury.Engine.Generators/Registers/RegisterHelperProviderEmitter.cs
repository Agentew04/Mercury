using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Mercury.Generators.Registers;

internal static class RegisterHelperProviderEmitter {
    public static void Emit(SourceProductionContext spc, ImmutableArray<EnumToGenerate> enums) {
        // separar enums por arquitetura
        ImmutableArray<IGrouping<string, EnumToGenerate>> groups = enums.GroupBy(x => x.ArchitectureFieldName).ToImmutableArray();

        foreach (IGrouping<string, EnumToGenerate> t in groups)
        {
            ImmutableArray<EnumToGenerate> group = t.ToImmutableArray();
            ExecuteManager(group, spc);
        }

        // criar agora o provider
        StringBuilder providerSb = new();
        foreach (IGrouping<string, EnumToGenerate> t in groups)
        {
            ImmutableArray<EnumToGenerate> group = t.ToImmutableArray();
            providerSb.AppendLine(string.Format(RegisterInfoTemplates.ProviderCaseFormat,
                /*0 arch class*/group[0].FullArchitectureClassName,
                /*1 arch field*/group[0].ArchitectureFieldName,
                /*2 arch field*/group[0].ArchitectureFieldName));
        }
        string file = string.Format(RegisterInfoTemplates.ProviderFormat,
            /*0 arch class*/enums[0].FullArchitectureClassName,
            /*1 cases*/providerSb);
        spc.AddSource($"RegisterHelperProvider.g.cs", SourceText.From(file, Encoding.UTF8));
    }
    
    private static void ExecuteManager(ImmutableArray<EnumToGenerate> enums, SourceProductionContext ctx)
    {
        if (enums.Length == 0) {
            return;
        }
        
        StringBuilder registerSb = new();
        StringBuilder numberSb = new();
        StringBuilder nameSb = new();
        StringBuilder nameInvSb = new();
        StringBuilder getRegisterFromNumberSb = new();
        StringBuilder getCountSb = new();
        foreach (EnumToGenerate enumtogen in enums) {
            registerSb.AppendLine(string.Format(RegisterInfoTemplates.SharedIfRegisterFormat,
                /*0 type*/enumtogen.FullEnumName,
                /*1 type*/enumtogen.ShortEnumName
            ));
            numberSb.AppendLine(string.Format(RegisterInfoTemplates.SharedIfNumberFormat,
                enumtogen.FullEnumName,enumtogen.FullEnumName));
            nameSb.AppendLine(string.Format(RegisterInfoTemplates.SharedIfNameFormat,
                enumtogen.FullEnumName,enumtogen.FullEnumName));
            nameInvSb.AppendLine(string.Format(RegisterInfoTemplates.SharedIfInvNameFormat,
                enumtogen.FullEnumName, enumtogen.ShortEnumName));
            getRegisterFromNumberSb.AppendLine(string.Format(RegisterInfoTemplates.SharedIfGetFromNumberFormat,
                enumtogen.FullEnumName, enumtogen.ShortEnumName));
            getCountSb.AppendLine(string.Format(RegisterInfoTemplates.SharedIfCountFormat,
                enumtogen.FullEnumName,
                enumtogen.ShortEnumName
            ));
        }

        string file = string.Format(RegisterInfoTemplates.SharedImplementationFormat,
            /*0 arch */ enums[0].ArchitectureFieldName,
            /*1 getRegister */ registerSb,
            /*2 getCount*/ getCountSb,
            numberSb,
            nameSb,
            nameInvSb,
            /*6 */getRegisterFromNumberSb
        );
        ctx.AddSource($"{enums[0].ArchitectureFieldName}RegisterHelper.g.cs", SourceText.From(file, Encoding.UTF8));
    }
}