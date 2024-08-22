using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace SAAE.Engine.Mips.Assembler; 

/// <summary>
/// Classe que transforma uma serie de tokens
/// em uma representacao de programa
/// </summary>
public class _Parser {

    private Dictionary<Section, ulong> currentPositions = [];
    Section currentSection = Section.Text;
    Program program = new();
    List<Diagnostic> diagnostics = [];
    uint currentLineSize = 0;

    public Program Parse(List<Tokenizer.Token> tokens, out List<Diagnostic> diagnostics) {
        this.diagnostics = [];
        diagnostics = this.diagnostics;
        program = new();
        currentPositions = new() {
            { Section.Text, 0x0040_0000 },
            { Section.Data, 0x1001_0000 }
        };
        currentSection = Section.Text;


        var lines = tokens
            .Where(x => x.Type != Tokenizer.TokenType.EOF)
            .Split(x => x.Type == Tokenizer.TokenType.NEWLINE);

        // make sense of each line
        foreach (var ln in lines) {
            if (!ln.Any()) {
                continue;
            }
            currentLineSize = 0;
            var line = ln;
            if (HasLabel(line)) {
                line = ProcessLabel(line);
            }

            var firstToken = line.FirstOrDefault();

            if (firstToken is null) {
                continue;
            }

            if (firstToken.Type is Tokenizer.TokenType.DIRECTIVE) {
                ProcessDirective(line);
            }
            else if(firstToken.Type is Tokenizer.TokenType.MNEMONIC) { 
                ProcessInstruction(line);
            } else {
                // emit erro
            }


            // se for uma variavel na data
            // ou uma instrucao, aumenta o contador
            currentPositions[currentSection] += currentLineSize;
        }

        return program;
    }

    private IEnumerable<Tokenizer.Token> ProcessLabel(IEnumerable<Tokenizer.Token> line) {
        var firstToken = line.First();
        if (program.Labels.ContainsKey(firstToken.Value)) {
            diagnostics.Add(new Diagnostic(true, $"Label {firstToken.Value} already defined", firstToken));
        } else {
            program.Labels[firstToken.Value] = currentPositions[currentSection];
        }
        return line.Skip(2);
    }

    private void ProcessDirective(IEnumerable<Tokenizer.Token> line) {
        var directive = line.First().Value;

        // se for uma diretiva de secao, muda a secao
        if (directive == ".data" || directive == ".text") {
            currentSection = directive == ".data" ? Section.Data : Section.Text;
            return;
        }

        // se for uma diretiva de .eqv
        if (directive == ".eqv") {
            ProcessEqv(line);
            return;
        }

        // se for uma diretiva de dados
        var dataType = DataValue.DirectiveToDataType(line.First().Value);
        if (dataType != DataValue.DataType.Invalid) {
            if(currentSection != Section.Data) {
                diagnostics.Add(new Diagnostic(true, $"Directive {directive} is only allowed on the .data section", line.First()));
                return;
            }
            ProcessData(line, dataType);
            return;
        }

        // macro
        // TODO ProcessMacro(line)

    }

    private void ProcessEqv(IEnumerable<Tokenizer.Token> line) {
        var firstToken = line.First();
        if (line.Count() < 3) {
            diagnostics.Add(new Diagnostic(true, "Invalid .eqv directive. It must have at least two parameters.", firstToken));
            return;
        }
        program.Eqvs[line.After(firstToken).Value] = string.Join("",
            line.Skip(2).Where(x => x.Type != Tokenizer.TokenType.COMMENT));
    }

    private void ProcessData(IEnumerable<Tokenizer.Token> line, DataValue.DataType dataType) {
        switch(dataType) {
            case DataValue.DataType.Byte:
            case DataValue.DataType.Half:
            case DataValue.DataType.Word:
            case DataValue.DataType.Float:
            case DataValue.DataType.Double:
                ProcessDataNumber(line, dataType);
                break;
            case DataValue.DataType.Ascii:
            case DataValue.DataType.Asciiz:
                ProcessString(line, dataType);
                break;
            case DataValue.DataType.Space:
                ProcessSpace(line);
                break;
        }
    }

    private void ProcessDataNumber(IEnumerable<Tokenizer.Token> line, DataValue.DataType dataType) {
        IEnumerable<Tokenizer.Token> numbers = line.Skip(1).Where(x => x.Type is Tokenizer.TokenType.NUMBER or Tokenizer.TokenType.CHAR);
        if (!numbers.Any()) {
            diagnostics.Add(new Diagnostic(false, "This directive has no use", line.First()));
            return;
        }

        foreach (string content in numbers.Select(x => x.Value)) {
            byte[] bytes = DataValue.Serialize(dataType, content);
            DataValue dataValue = new(dataType, currentPositions[Section.Data], (uint)bytes.Length, bytes);
            currentLineSize += (uint)bytes.Length;
            program.DataValues.Add(dataValue);
        }
    }

    private void ProcessString(IEnumerable<Tokenizer.Token> line, DataValue.DataType dataType) {
        IEnumerable<Tokenizer.Token> strings = line.Skip(1).Where(x => x.Type is Tokenizer.TokenType.STRING);
        if (!strings.Any()) {
            diagnostics.Add(new Diagnostic(false, "This directive has no use", line.First()));
            return;
        }


        foreach (string content in strings.Select(x => x.Value).Select(x => x[1..^1])) {
            byte[] bytes = DataValue.Serialize(dataType, content);
            DataValue dataValue = new(dataType, currentPositions[Section.Data], (uint)bytes.Length, bytes);
            currentLineSize += (uint)bytes.Length;
            program.DataValues.Add(dataValue);
        }
    }

    private void ProcessSpace(IEnumerable<Tokenizer.Token> line) {
        if(line.Count() < 2) {
            diagnostics.Add(new Diagnostic(true, "Invalid .space directive. It must have at least one parameter.", line.First()));
            return;
        }

        currentLineSize = uint.Parse(line.Skip(1).First().Value);
        DataValue dataValue = new(DataValue.DataType.Space, currentPositions[Section.Data], currentLineSize, new byte[currentLineSize]);
        program.DataValues.Add(dataValue);
    }

    private void ProcessInstruction(IEnumerable<Tokenizer.Token> line) {
        if(currentSection != Section.Text) {
            return;
        }
        if (!line.Any()) {
            return;
        }

        if(line.First().Type != Tokenizer.TokenType.IDENTIFIER) {
            diagnostics.Add(new Diagnostic(true, "Format not recognized", line.First()));
            return;
        }
        program.Instructions.Add(new InstructionValue() {
            Address = currentPositions[currentSection],
            //Tokens = line.ToList()
        });
        currentLineSize = 4;
    }

    private static bool HasLabel(IEnumerable<Tokenizer.Token> line) {
        // se primeiro eh identificador e o segundo eh dois pontos
        return line.First().Type == Tokenizer.TokenType.IDENTIFIER && line.After(line.First()).Type == Tokenizer.TokenType.COLON;
    }

    private enum Section {
        Data,
        Text
    }

    /// <summary>
    /// Represents a problem in the source code
    /// </summary>
    public class Diagnostic {

        public bool IsError { get; init; }

        public string ErrorMessage { get; init; }

        public Tokenizer.Token? Token { get; init; }

        public Range Range { get; init; }

        public int LineNumber { get; init; }

        public Diagnostic(bool isError, string errorMessage, Tokenizer.Token token) {
            IsError = isError;
            ErrorMessage = errorMessage;
            Token = token;
            Range = Token.TextRange;
            LineNumber = Token.LineNumber;
        }

        public Diagnostic(bool isError, string errorMessage, Range range, int lineNumber) {
            IsError = isError;
            ErrorMessage = errorMessage;
            Token = null;
            Range = range;
            LineNumber = lineNumber;
        }
    }
}
