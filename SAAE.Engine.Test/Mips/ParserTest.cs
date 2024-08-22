using SAAE.Engine.Mips.Assembler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAAE.Engine.Test.Mips;

[TestClass]
public class ParserTest {

    [TestMethod]
    public void InstructionLabelTest() {
        string code = """
            start: add $t0, $t1, $t2
            add $t0, $t1, $t2
            add $t0, $t1, $t2
            loop:
            addi $s0,$s0, 1
            bne $s0, $t2, loop
            end:
            """;
        var tokens = Tokenizer.Tokenize(code);
        _Parser parser = new();
        Program prog = parser.Parse(tokens, out var diagnostics);
        Assert.AreEqual(0, diagnostics.Count);
        Assert.AreEqual(3, prog.Labels.Count);
        Assert.AreEqual(0x0040_0000ul, prog.Labels["start"], $"\nExpected:\t{0x0040_0000ul:X2}\nReal:\t\t{prog.Labels["start"]:X2}");
        Assert.AreEqual(0x0040_000cul, prog.Labels["loop"], $"\nExpected:\t{0x0040_000cul:X2}\nReal:\t\t{prog.Labels["loop"]:X2}");
        Assert.AreEqual(0x0040_0014ul, prog.Labels["end"], $"\nExpected:\t{0x0040_0014ul:X2}\nReal:\t\t{prog.Labels["end"]:X2}");
    }

    [TestMethod]
    public void VariableLabelTest() {
        string code = """
            add $t2,$t1,$t0
            #empty line

            .data
            int1: .word 1337 123 5
            int2: .word
            int3: .word 2
            byte1: .byte 21 15
            byte2: .byte
            byte3: .byte 5 1
            float1: .float 3.14
            float2: .double 3.1415
            str1: .ascii
            str2: .ascii "opt1" "hello"
            str3: .asciiz "" "Hello, World!"
            str4: .asciiz ""
            arr1: .space 1
            arr2: .space 10

            .text
            addi $s0, $s1, 5

            """;
        var tokens = Tokenizer.Tokenize(code);
        _Parser parser = new();
        Program prog = parser.Parse(tokens, out var diagnostics);
        Assert.AreEqual(14, prog.Labels.Count);
        Assert.AreEqual(0x1001_0000ul, prog.Labels["int1"], $"\nExpected:\t{0x1001_0000ul:X2}\nReal:\t\t{prog.Labels["int1"]:X2}");
        Assert.AreEqual(0x1001_000cul, prog.Labels["int2"], $"\nExpected:\t{0x1001_000cul:X2}\nReal:\t\t{prog.Labels["int2"]:X2}");
        Assert.AreEqual(0x1001_000cul, prog.Labels["int3"], $"\nExpected:\t{0x1001_0000ul:X2}\nReal:\t\t{prog.Labels["int3"]:X2}");
        Assert.AreEqual(0x1001_0010ul, prog.Labels["byte1"], $"\nExpected:\t{0x1001_0010ul:X2}\nReal:\t\t{prog.Labels["byte1"]:X2}");
        Assert.AreEqual(0x1001_0012ul, prog.Labels["byte2"], $"\nExpected:\t{0x1001_0012ul:X2}\nReal:\t\t{prog.Labels["byte2"]:X2}");
        Assert.AreEqual(0x1001_0012ul, prog.Labels["byte3"], $"\nExpected:\t{0x1001_0012ul:X2}\nReal:\t\t{prog.Labels["byte3"]:X2}");
        Assert.AreEqual(0x1001_0014ul, prog.Labels["float1"], $"\nExpected:\t{0x1001_0014ul:X2}\nReal:\t\t{prog.Labels["float1"]:X2}");
        Assert.AreEqual(0x1001_0018ul, prog.Labels["float2"], $"\nExpected:\t{0x1001_0018ul:X2}\nReal:\t\t{prog.Labels["float2"]:X2}");
        Assert.AreEqual(0x1001_0020ul, prog.Labels["str1"], $"\nExpected:\t{0x1001_0020ul:X2}\nReal:\t\t{prog.Labels["str1"]:X2}");
        Assert.AreEqual(0x1001_0020ul, prog.Labels["str2"], $"\nExpected:\t{0x1001_0020ul:X2}\nReal:\t\t{prog.Labels["str2"]:X2}");
        Assert.AreEqual(0x1001_0029ul, prog.Labels["str3"], $"\nExpected:\t{0x1001_0029ul:X2}\nReal:\t\t{prog.Labels["str3"]:X2}");
        Assert.AreEqual(0x1001_0038ul, prog.Labels["str4"], $"\nExpected:\t{0x1001_0038ul:X2}\nReal:\t\t{prog.Labels["str4"]:X2}");
        Assert.AreEqual(0x1001_0039ul, prog.Labels["arr1"], $"\nExpected:\t{0x1001_0039ul:X2}\nReal:\t\t{prog.Labels["arr1"]:X2}");
        Assert.AreEqual(0x1001_003aul, prog.Labels["arr2"], $"\nExpected:\t{0x1001_003aul:X2}\nReal:\t\t{prog.Labels["arr2"]:X2}");
    }
}
