using SAAE.Engine.Mips.Assembler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SAAE.Engine.Mips.Assembler.Tokenizer;

namespace SAAE.Engine.Test.Mips; 

[TestClass]
public class TokenizerTest {

    [TestMethod]
    public void TokenizeLine_ShouldReturnEmptyList_WhenLineIsEmpty() {
        var tokenizer = new Tokenizer();
        var result = tokenizer.Tokenize("");
        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public void TokenizeLine_ShouldReturnEmptyList_WhenLineIsWhiteSpace() {
        var tokenizer = new Tokenizer();
        var result = tokenizer.Tokenize("    ");
        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public void TokenizeLine_ShouldReturnSingleToken_WhenLineIsComment() {
        var tokenizer = new Tokenizer();
        string source = "# This is a comment";
        var result = tokenizer.Tokenize(source);
        Assert.AreEqual(2, result.Count);
        Assert.AreEqual(TokenType.COMMENT, result[0].Type);
        Assert.AreEqual("# This is a comment", result[0].Value);
        Assert.AreEqual("# This is a comment", source[result[0].TextRange]);
    }

    [TestMethod]
    public void TokenizeLine_ShouldReadLabels() {
        var tokenizer = new Tokenizer();
        string source = "label:";
        var result = tokenizer.Tokenize(source);
        Assert.AreEqual(3, result.Count);
        Assert.AreEqual(TokenType.IDENTIFIER, result[0].Type);
        Assert.AreEqual("label", result[0].Value);
        Assert.AreEqual("label", source[result[0].TextRange]);
        Assert.AreEqual(TokenType.COLON, result[1].Type);
        Assert.AreEqual(":", result[1].Value);
        Assert.AreEqual(":", source[result[1].TextRange]);
        Assert.AreEqual(TokenType.EOF, result[2].Type);
    }

    [TestMethod]
    public void TokenizeLine_ShouldReadRegisters() {
        var tokenizer = new Tokenizer();
        string source = "$t0, $ra";
        var result = tokenizer.Tokenize(source);
        Assert.AreEqual(4, result.Count);
        Assert.AreEqual(TokenType.REGISTER, result[0].Type);
        Assert.AreEqual("t0", result[0].Value);
        Assert.AreEqual("t0", source[result[0].TextRange]);
        Assert.AreEqual(TokenType.COMMA, result[1].Type);
        Assert.AreEqual(TokenType.REGISTER, result[2].Type);
        Assert.AreEqual("ra", result[2].Value);
        Assert.AreEqual("ra", source[result[2].TextRange]);
        Assert.AreEqual(TokenType.EOF, result[3].Type);
    }

    [TestMethod]
    public void TokenizeLine_ShouldReadParethesis() {
        var tokenizer = new Tokenizer();
        var source = "add()";
        var result = tokenizer.Tokenize(source);
        Assert.AreEqual(4, result.Count);
        Assert.AreEqual(TokenType.IDENTIFIER, result[0].Type);
        Assert.AreEqual("add", result[0].Value);
        Assert.AreEqual("add", source[result[0].TextRange]);
        Assert.AreEqual(TokenType.LEFT_PARENTHESIS, result[1].Type);
        Assert.AreEqual("(", source[result[1].TextRange]);
        Assert.AreEqual(TokenType.RIGHT_PARENTHESIS, result[2].Type);
        Assert.AreEqual(")", source[result[2].TextRange]);
        Assert.AreEqual(TokenType.EOF, result[3].Type);
    }

    [TestMethod]
    public void TokenizeLine_ShouldReadNumber() {
        var tokenizer = new Tokenizer();
        var source = "15 0x20 0X2 0xF0 0xCaFEF0Fa";
        var result = tokenizer.Tokenize(source);
        Assert.AreEqual(6, result.Count);
        Assert.AreEqual(TokenType.NUMBER, result[0].Type);
        Assert.AreEqual("15", result[0].Value);
        Assert.AreEqual("15", source[result[0].TextRange]);
        Assert.AreEqual(TokenType.NUMBER, result[1].Type);
        Assert.AreEqual("0x20", result[1].Value);
        Assert.AreEqual("0x20", source[result[1].TextRange]);
        Assert.AreEqual(TokenType.NUMBER, result[2].Type);
        Assert.AreEqual("0X2", result[2].Value);
        Assert.AreEqual("0X2", source[result[2].TextRange]);
        Assert.AreEqual(TokenType.NUMBER, result[3].Type);
        Assert.AreEqual("0xF0", result[3].Value);
        Assert.AreEqual("0xF0", source[result[3].TextRange]);
        Assert.AreEqual(TokenType.NUMBER, result[4].Type);
        Assert.AreEqual("0xCaFEF0Fa", result[4].Value);
        Assert.AreEqual("0xCaFEF0Fa", source[result[4].TextRange]);
        Assert.AreEqual(TokenType.EOF, result[5].Type);
    }

    [TestMethod]
    public void TokenizeLine_ShouldReadDirective() {
        var tokenizer = new Tokenizer();
        var source = """
            .macro done
            li $v0, 1
            syscall
            .end_macro
            """;
        var result = tokenizer.Tokenize(source);
        Assert.AreEqual(12, result.Count);
        CollectionAssert.AreEqual(new TokenType[] {
            TokenType.DIRECTIVE, TokenType.IDENTIFIER, TokenType.NEWLINE,
            TokenType.IDENTIFIER, TokenType.REGISTER, TokenType.COMMA, TokenType.NUMBER, TokenType.NEWLINE,
            TokenType.IDENTIFIER, TokenType.NEWLINE,
            TokenType.DIRECTIVE, TokenType.EOF
        }, result.Select(x => x.Type).ToList());
    }

    [TestMethod]
    public void TokenizeLine_ShouldReadAll() {
        var tokenizer = new Tokenizer();
        var source = """
            label: add $t0, $t1, $zero
            sw $f2 0x20($ra) 
            branch: beq $s0, $s1, label# this is a comment 
            j branch 
            """;
        var result = tokenizer.Tokenize(source);
        Assert.AreEqual(29, result.Count);
        CollectionAssert.AreEqual(new TokenType[] {
            TokenType.IDENTIFIER, TokenType.COLON, TokenType.IDENTIFIER, TokenType.REGISTER, TokenType.COMMA, TokenType.REGISTER, TokenType.COMMA, TokenType.REGISTER, TokenType.NEWLINE,
            TokenType.IDENTIFIER, TokenType.REGISTER, TokenType.NUMBER, TokenType.LEFT_PARENTHESIS, TokenType.REGISTER, TokenType.RIGHT_PARENTHESIS, TokenType.NEWLINE,
            TokenType.IDENTIFIER, TokenType.COLON, TokenType.IDENTIFIER, TokenType.REGISTER, TokenType.COMMA, TokenType.REGISTER, TokenType.COMMA, TokenType.IDENTIFIER, TokenType.COMMENT, TokenType.NEWLINE,
            TokenType.IDENTIFIER, TokenType.IDENTIFIER, TokenType.EOF},
            result.Select(x => x.Type).ToList()
        );
    }
}
