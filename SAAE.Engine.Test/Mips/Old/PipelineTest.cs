using SAAE.Engine.Mips.Assembler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAAE.Engine.Test.Mips.Old;

[TestClass]
public class PipelineTest
{

    [TestMethod]
    public void TestSourceCodeStage()
    {
        string code = """
            add $t1, $zero, $s0
            sub $t1, $0, $s1
            .data
            label3: .word 5
            .text
            label1: or $s7, $k0, $1
            xori $27, $k1, 23
            srav $t1, $t2, $a3
            label2: sll $fp, $v0, 5
            beq $t2, $t0, label2
            sw $t1, 0($t2)
            """;

        MipsAssembler assembler = new();
        SourceTextStage sourceTextStage = assembler.InputText(code);
        Assert.AreEqual(11, sourceTextStage.SourceText.Count);
    }

    [TestMethod]
    public void TestMacroSolverStage1()
    {
        string code = """
            .macro done
            addi $v0, $0, 10
            syscall
            .end_macro
            add $t1, $zero, $s0
            sub $t1, $0, $s1
            .macro add3(%a,%b,%c)
            add %a, %a, %b
            add %a, %a, %c
            .end_macro 
            label1: or $s7, $k0, $1
            .macro add2 (%a,%b)
            add %a, %a, %b
            .end_macro 
            xori $27, $k1, 23
            done
            srav $t1, $t2, $a3
            label2: sll $fp, $v0, 5
            beq $t2, $t0, label2
            add3($t0,$t1,$t2)
            add2($t0,$t1)
            sw $t1, 0($t2)
            """;

        MipsAssembler assembler = new();
        SourceTextStage sourceTextStage = assembler.InputText(code);
        MacroSolvingStage macroSolvingStage = assembler.ResolveMacros(sourceTextStage);

        Assert.AreEqual(13, macroSolvingStage.SourceText.Count);
    }

    [TestMethod]
    public void TestMacroSolverStage2()
    {
        string code = """
            .macro add3(%a,%b,%c)
            add %a, %a, %b
            add %a, %a, %c
            .end_macro 
            add3($t0,$t1,$t2)
            """;

        MipsAssembler assembler = new();
        SourceTextStage sourceTextStage = assembler.InputText(code);
        MacroSolvingStage macroSolvingStage = assembler.ResolveMacros(sourceTextStage);

        Assert.AreEqual(2, macroSolvingStage.SourceText.Count);
        Assert.AreEqual("add $t0, $t0, $t1", macroSolvingStage.SourceText[0]);
        Assert.AreEqual("add $t0, $t0, $t2", macroSolvingStage.SourceText[1]);
    }

    [TestMethod]
    public void TestMacroSolverStage3()
    {
        string code = """
            .eqv test 5
            addi $t1, $zero, test
            """;

        MipsAssembler assembler = new();
        SourceTextStage sourceTextStage = assembler.InputText(code);
        MacroSolvingStage macroSolvingStage = assembler.ResolveMacros(sourceTextStage);

        Assert.AreEqual(1, macroSolvingStage.SourceText.Count);
        Assert.AreEqual("addi $t1, $zero, 5", macroSolvingStage.SourceText[0]);
    }

    [TestMethod]
    public void TestSymbolReadingStage1()
    {
        string code = """
            add $t1, $zero, $s0
            sub $t1, $0, $s1
            .data
            label3: .word 5
            .text
            label1: or $s7, $k0, $1
            xori $27, $k1, 23
            srav $t1, $t2, $a3
            label2: sll $fp, $v0, 5
            beq $t2, $t0, label2
            sw $t1, 0($t2)
            # label: test comments and labels
            """;

        MipsAssembler assembler = new();
        SourceTextStage sourceTextStage = assembler.InputText(code);
        MacroSolvingStage macroStage = assembler.ResolveMacros(sourceTextStage);
        PseudoInstructionSolvingStage pseudoStage = assembler.ResolvePseudoInstructions(macroStage);
        SymbolReadingStage symbolReadingStage = assembler.ReadSymbols(pseudoStage);

        Assert.AreEqual(3, symbolReadingStage.SymbolTable.Count);
        Assert.IsTrue(symbolReadingStage.SymbolTable.ContainsKey("label1"));
        Assert.IsTrue(symbolReadingStage.SymbolTable.ContainsKey("label2"));
        Assert.IsTrue(symbolReadingStage.SymbolTable.ContainsKey("label3"));
    }

    [TestMethod]
    public void TestSymbolReadingStage2()
    {
        string code = """
            label1: or $s7, $k0, $1
            """;

        MipsAssembler assembler = new();
        SourceTextStage sourceTextStage = assembler.InputText(code);
        MacroSolvingStage macroStage = assembler.ResolveMacros(sourceTextStage);
        PseudoInstructionSolvingStage pseudoStage = assembler.ResolvePseudoInstructions(macroStage);
        SymbolReadingStage symbolReadingStage = assembler.ReadSymbols(pseudoStage);

        Assert.AreEqual(1, symbolReadingStage.SymbolTable.Count);
        Assert.IsTrue(symbolReadingStage.SymbolTable.ContainsKey("label1"));
        Assert.AreEqual("or $s7, $k0, $1", symbolReadingStage.SourceText[0]);
    }

    [TestMethod]
    public void TestInstructionAssembly1()
    {
        string code = """
            addi $t0, $zero, 5
            addi $t1, $zero, 10
            sub $t2, $t1, $t0
            beq $t2, $t0, 0x2
            addi $s0, $zero, 1
            j 0x0040001C
            addi $s0, $zero, 2
            """;

        byte[] expectedTextSegment = [
            0x05, 0x00, 0x08, 0x20, // addi
            0x0A, 0x00, 0x09, 0x20, // addi
            0x22, 0x50, 0x28, 0x01, // sub
            0x02, 0x00, 0x48, 0x11, // beq
            0x01, 0x00, 0x10, 0x20, // addi
            0x07, 0x00, 0x10, 0x08, // j
            0x02, 0x00, 0x10, 0x20  // addi
        ];

        MipsAssembler assembler = new();
        SourceTextStage sourceTextStage = assembler.InputText(code);
        MacroSolvingStage macroStage = assembler.ResolveMacros(sourceTextStage);
        PseudoInstructionSolvingStage pseudoStage = assembler.ResolvePseudoInstructions(macroStage);
        SymbolReadingStage symbolReadingStage = assembler.ReadSymbols(pseudoStage);
        AssembleStage assembleStage = assembler.AssembleInstructions(symbolReadingStage);

        CollectionAssert.AreEqual(expectedTextSegment, assembleStage.TextSegment);
    }

}
