using Antlr4.Runtime.Misc;
using SAAE.Engine.Mips.Assembler.Antlr;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAAE.Engine.Mips.Assembler; 
public class MipsVisitor : Antlr.MipsBaseVisitor<Program> {
    private ulong currentAddress = 0;
    private Dictionary<string, ulong> labels = [];

    public override Program VisitProgram([NotNull] MipsParser.ProgramContext context) {
        
        return base.VisitProgram(context);
    }


    public override  Program VisitLabel([NotNull] MipsParser.LabelContext context) {
        labels[context.ID().GetText()] = currentAddress;
        return base.VisitLabel(context);
    }

    public override Program VisitInstruction([NotNull] MipsParser.InstructionContext context) {
        InstructionValue inst = new();
        inst.Address = currentAddress;

        foreach(var child in context.children) {
            //if(child is MipsParser.ID mnemonic) {
            //    inst.Mnemonic = mnemonic.GetText();
            //}
            //else if(child is MipsParser.RegisterContext register) {
            //    inst.Registers.Add(register.GetText());
            //}
            //else if(child is MipsParser.ImmediateContext immediate) {
            //    inst.Immediates.Add(immediate.GetText());
            //}
            //else if(child is MipsParser.LabelContext label) {
            //    inst.Labels.Add(label.GetText());
            //}
        }
        currentAddress+=4;
        return base.VisitInstruction(context);
    }
}
