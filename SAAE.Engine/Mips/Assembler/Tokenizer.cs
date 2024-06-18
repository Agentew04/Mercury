using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAAE.Engine.Mips.Assembler;

/// <summary>
/// Receives a string of source code and returns a list of tokens.
/// </summary>
public class Tokenizer {

    public List<Token> Tokenize(string source) {
        var tokens = new List<Token>();
        var lines = source.Split('\n');
        int startOfLine = 0;
        for (int i = 0; i < lines.Length; i++) {
            var line = lines[i];
            var lineTokens = TokenizeLine(line);
            foreach (var token in lineTokens) {
                token.LineNumber = i + 1;
                // fix ranges based on the line
                token.TextRange = new Range(token.TextRange.Start.Value + startOfLine, token.TextRange.End.Value + startOfLine);
            }
            tokens.AddRange(lineTokens);
            if(i < lines.Length - 1) {
                tokens.Add(new Token {
                    Type = TokenType.NEWLINE,
                    Value = "\n",
                    TextRange = new Range(startOfLine + line.Length, startOfLine + line.Length + 1)
                });
            }
            startOfLine += line.Length+1;
        }
        tokens.Add(new Token {
            Type = TokenType.EOF,
            Value = "",
            TextRange = new Range(startOfLine, startOfLine)
        });
        return tokens;
    }

    private List<Token> TokenizeLine(string line) {
        var tokens = new List<Token>();

        StringBuilder reader = new StringBuilder();
        
        for(int i=0; i < line.Length; i++) {
            if (char.IsWhiteSpace(line[i])) {
                continue;
            }

            if (line[i] == '#') {
                // read until the end
                int j;
                for (j = i; j < line.Length; j++) {
                    reader.Append(line[j]);
                }
                tokens.Add(new Token() {
                    Type = TokenType.COMMENT,
                    Value = reader.ToString(),
                    TextRange = new Range(i, j)
                });
                break;
            }

            // match directives
            if (line[i] == '.') {
                int j = i;
                while(j < line.Length && !IsSeparator(line[j])) {
                    reader.Append(line[j]);
                    j++;
                }
                tokens.Add(new Token() {
                    Type = TokenType.DIRECTIVE,
                    Value = reader.ToString(),
                    TextRange = new Range(i,j)
                });
                reader.Clear();
                i = j - 1;
                continue;
            }

            // match special characters
            if (line[i] == ',') {
                tokens.Add(new Token() {
                    Type = TokenType.COMMA,
                    Value = ",",
                    TextRange = new Range(i,i+1)
                });
                continue;
            }else if (line[i] == ':') {
                tokens.Add(new Token() {
                    Type = TokenType.COLON,
                    Value = ":",
                    TextRange = new Range(i, i + 1)
                });
                continue;
            }else if (line[i] == '(') {
                tokens.Add(new Token() {
                    Type = TokenType.LEFT_PARENTHESIS,
                    Value = "(",
                    TextRange = new Range(i, i + 1)
                });
                continue;
            }else if (line[i] == ')') {
                tokens.Add(new Token() {
                    Type = TokenType.RIGHT_PARENTHESIS,
                    Value = ")",
                    TextRange = new Range(i, i + 1)
                });
                continue;
            }

            // check if it is a register
            if (line[i] == '$') {
                int j = i+1;
                while(j<line.Length && !IsSeparator(line[j])){
                    reader.Append(line[j]);
                    j++;
                }
                tokens.Add(new Token() {
                    Type = TokenType.REGISTER,
                    Value = reader.ToString(),
                    TextRange = new Range(i+1, j) // i+1 pra pular o $
                });
                reader.Clear();
                i = j - 1;
                continue;
            }

            // read potential number
            if (char.IsDigit(line[i])) {
                int j = i;
                while(j < line.Length && (char.IsDigit(line[j]) || char.ToLower(line[j]) == 'x' || char.ToLower(line[j]) is >= 'a' and <= 'f')) {
                    reader.Append(line[j]);
                    j++;
                }
                tokens.Add(new Token {
                    Type = TokenType.NUMBER,
                    Value = reader.ToString(),
                    TextRange = new Range(i,j)
                });
                reader.Clear();
                i = j - 1;
                continue;
            }


            int k = i;
            while (k < line.Length && !IsSeparator(line[k])) {
                reader.Append(line[k]);
                k++;
            }
            tokens.Add(new Token() {
                Type = TokenType.IDENTIFIER,
                Value = reader.ToString(),
                TextRange = new Range(i,k)
            });
            reader.Clear();
            i = k - 1;
        }
        
        return tokens;
    }

    private static bool IsSeparator(char c) {
        return c == ',' || c == '(' || c == ')' || c == '#' || c == ':' || c == ' ' || c == '\n' || c == '\r';
    }

    public class Token {

        /// <summary>
        /// The type of this token
        /// </summary>
        public TokenType Type { get; set; }

        /// <summary>
        /// The literal string value of the token.
        /// </summary>
        public string Value { get; set; } = "";

        /// <summary>
        /// The line this token belongs to.
        /// </summary>
        public int LineNumber { get; set; }

        /// <summary>
        /// Represents the range on the line that this token is located
        /// </summary>
        public Range TextRange { get; set; }
    }

    public enum TokenType {
        /// <summary>
        /// A name, can be a label, an eqv or a mnemonic
        /// </summary>
        IDENTIFIER,
        /// <summary>
        /// Represents the ':' character
        /// </summary>
        COLON,
        /// <summary>
        /// Represents the ',' character
        /// </summary>
        COMMA,
        /// <summary>
        /// The name of a register
        /// </summary>
        REGISTER,
        /// <summary>
        /// A decimal or hexadecimal number
        /// </summary>
        NUMBER,
        /// <summary>
        /// Represents the '(' character
        /// </summary>
        LEFT_PARENTHESIS,
        /// <summary>
        /// Represents the ')' character
        /// </summary>
        RIGHT_PARENTHESIS,
        /// <summary>
        /// Represents a preprocessing directive. Always starts with the '.' character.
        /// </summary>
        DIRECTIVE,
        /// <summary>
        /// Represents a comment in the line. Always starts with
        /// the '#' character.
        /// </summary>
        COMMENT,
        /// <summary>
        /// Represents the division between two lines
        /// </summary>
        NEWLINE,
        /// <summary>
        /// Represents the end of the file. Should be the last token emited by the tokenizer.
        /// </summary>
        EOF
    }

}
