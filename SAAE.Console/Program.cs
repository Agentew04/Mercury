using Antlr4.Runtime;
using SAAE.Engine.Mips.Assembler;
using SAAE.Engine.Mips.Assembler.Antlr;
using static System.Console;

namespace SAAE.Console; 
internal class Program {
    static void Main(string[] args) {

        //SAAE.Engine.Mips.Assembler.
        WriteLine("Hello, World!");
        string file = """
                        .text
            label1: addi $t0, $zero, 5
            label2: addi $t1, $zero, 10
            sub $t2, $t1, $t0
            .data
            label3: .word 
            label4: .word 1 2
            label5: .word 'a'
            emptyVar: .byte
            emptyVar2: .byte
            array1: .space 12
            str1: .asciiz "Hello!"
            str2: .ascii 
            str3: .ascii 
            str4: .asciiz "" "" 
            str5: .asciiz
            str6: .ascii 
            .text
            .eqv blam 5
            .eqv bla show de bola
            .macro add3(%a,%b,%c)
            add %a, %a, %b
            add %a, %a, %c
            .end_macro 
            beq $t2, $t0, label1
            addi $s0, $zero, 1  
            add3($t0,$t1,$t2)
            j label2
            """;

        var inputStream = new AntlrInputStream(file);
        var lexer = new MipsLexer(inputStream);
        var cts = new CommonTokenStream(lexer);
        var parser = new MipsParser(cts);

        var visitor = new MipsVisitor();
        //visitor.g
        var result = visitor.Visit(parser.program());
    }
}
