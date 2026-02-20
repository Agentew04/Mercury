using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Mercury.Generators.Registers;

internal static class RegisterHelperEmitter {
    public static void Emit(SourceProductionContext spc, EnumToGenerate enumToGenerate){
        StringBuilder numSb = new();
        StringBuilder nameSb = new();
        StringBuilder regSb = new();
        StringBuilder nameInvSb = new();

        if (enumToGenerate.Registers.Count == 0) {
            return;
        }

        foreach (RegisterInfo reg in enumToGenerate.Registers) {
            if (reg.HasNumber) {
                numSb.AppendLine(string.Format(RegisterInfoTemplates.SwitchCaseFormat,
                    /*0 field*/reg.EnumMemberName,
                    /*1 number*/reg.Number));
                regSb.AppendLine(string.Format(RegisterInfoTemplates.SwitchCaseFormat,
                    /*0 number*/reg.Number,
                    /*1 field*/reg.EnumMemberName));
            }
            nameSb.AppendLine(string.Format(RegisterInfoTemplates.SwitchCaseFormat,
                /*0 case*/$"{reg.EnumMemberName}",
                /*1 return*/$"\"{reg.Name}\""));
            nameInvSb.AppendLine(string.Format(RegisterInfoTemplates.SwitchCaseFormat,
                /*0 case*/$"\"{reg.Name}\"",
                /*1 ret*/reg.EnumMemberName));

        }

        string file = string.Format(RegisterInfoTemplates.ImplementationRegisterSetFormat,
            /*0 archname*/enumToGenerate.ArchitectureFieldName,
            /*1 enum name*/enumToGenerate.FullEnumName,
            /*2 get num cases*/numSb,
            /*3* enum name*/enumToGenerate.FullEnumName,
            /*4 get name cases*/nameSb,
            /*5 enum name*/enumToGenerate.ShortEnumName,
            /*6 reg count*/enumToGenerate.Registers.Count,
            /*7 enum name*/enumToGenerate.FullEnumName,
            /*8 enum name*/enumToGenerate.ShortEnumName,
            /*9 get reg cases*/regSb,
            /*10 enum name*/enumToGenerate.FullEnumName,
            /*11 short enum name*/enumToGenerate.ShortEnumName,
            /*12 name inverse*/nameInvSb);
        
        spc.AddSource($"RegisterHelper.{enumToGenerate.ShortEnumName}.g.cs", SourceText.From(file, Encoding.UTF8));
    }
}