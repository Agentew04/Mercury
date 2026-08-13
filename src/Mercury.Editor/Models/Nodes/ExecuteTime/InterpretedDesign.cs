using System;
using System.Collections.Generic;
using System.Linq;
using Mercury.Editor.Models.Nodes.DesignTime;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Mercury.Editor.Models.Nodes.ExecuteTime;

/// <summary>
/// Untested
/// </summary>
public class InterpretedDesign : ICompiledDesign {

    private readonly Design design;
    private readonly Dictionary<DesignBlock, StatementSyntax[]> blockBody;
    private readonly List<DesignBlock> topoOrder;
    
    private readonly Dictionary<(DesignBlock,IoItem), int> itemValues = [];
    
    public InterpretedDesign(Design design, Dictionary<DesignBlock, StatementSyntax[]> blockBody, List<DesignBlock> topoOrder) {
        this.design = design;
        this.blockBody = blockBody;
        this.topoOrder = topoOrder;
    }
    
    public void Clock() {
        
        // compute barriers
        foreach (DesignBlock block in design.Blocks.Where(x => x.IsBarrier)) {
            Compute(block);
        }
        
        Dictionary<Connection, bool> computed = design.Connections.ToDictionary(x => x, _ => false);
        // set inputs
        foreach(DesignBlock block in topoOrder) {
            if (block.IsBarrier) {
                continue;
            }
            
            foreach (Connection incoming in design.Connections.Where(x => x.End == block)) {
                if (computed[incoming]) {
                    continue;
                }
                computed[incoming] = true;
                itemValues[(block, incoming.EndInput)] = itemValues[(incoming.Start, incoming.StartOutput)];
            }
            Compute(block);
            
            foreach (Connection outgoing in design.Connections.Where(x => x.Start == block)) {
                if (computed[outgoing]) {
                    continue;
                }
                computed[outgoing] = true;
                itemValues[(outgoing.End, outgoing.EndInput)] = itemValues[(block, outgoing.StartOutput)];
            }
        }
        
        // commit barriers
        
    }

    private void Compute(DesignBlock block) {
        StatementSyntax[] statements = blockBody[block];
        VariableStore variableStore = new();
        // update inputs
        
        foreach (StatementSyntax statement in statements) {
            Evaluate(statement, variableStore);
        }
        // check outputs
    }

    #region Evaluate

    private static void Evaluate(StatementSyntax statement, VariableStore variableStore) {
        switch (statement) {
            case LocalDeclarationStatementSyntax decl:
                foreach (VariableDeclaratorSyntax variable in decl.Declaration.Variables) {
                    variableStore.Allocate(variable.Identifier.ValueText);
                    if (variable.Initializer != null) {
                        int result = Evaluate(variable.Initializer.Value, variableStore);
                        variableStore.SetValue(variable.Identifier.ValueText, result);
                    }
                }
                break;
            case ExpressionStatementSyntax expr:
                Evaluate(expr.Expression, variableStore);
                break;
        }
    }

    private static int Evaluate(ExpressionSyntax expression, VariableStore variableStore) {
        switch (expression) {
            case PostfixUnaryExpressionSyntax postfix: {
                if (postfix.Operand is not IdentifierNameSyntax identifier) {
                    throw new Exception("Unexpected expression");
                }

                int value = variableStore.GetValue(identifier.Identifier.ValueText);
                if (postfix.IsKind(SyntaxKind.PostIncrementExpression)) {
                    variableStore.SetValue(identifier.Identifier.ValueText, value + 1);
                }
                else if (postfix.IsKind(SyntaxKind.PostDecrementExpression)) {
                    variableStore.SetValue(identifier.Identifier.ValueText, value - 1);
                }
                return value;
            }
            case PrefixUnaryExpressionSyntax prefix: {
                if (prefix.Operand is not IdentifierNameSyntax identifier) {
                    throw new Exception("Unexpected expression");
                }

                int value = variableStore.GetValue(identifier.Identifier.ValueText);
                if (prefix.IsKind(SyntaxKind.PostIncrementExpression)) {
                    value += 1;
                    variableStore.SetValue(identifier.Identifier.ValueText, value);
                }
                else if (prefix.IsKind(SyntaxKind.PostDecrementExpression)) {
                    value -= 1;
                    variableStore.SetValue(identifier.Identifier.ValueText, value);
                }else if (prefix.IsKind(SyntaxKind.BitwiseNotExpression)) {
                    value = ~value;
                }else if (prefix.IsKind(SyntaxKind.LogicalNotExpression)) {
                    value = value > 0 ? 0 : 1;
                }

                return value;
            }
            case AssignmentExpressionSyntax assign: {
                int result = Evaluate(assign.Right, variableStore);
                if (assign.Left is IdentifierNameSyntax left) {
                    // direct store
                    variableStore.SetValue(left.Identifier.ValueText, result);
                }
                else if (assign.Left is MemberAccessExpressionSyntax memberaccess) {
                    if (memberaccess.Expression is not IdentifierNameSyntax identifier) {
                        throw new Exception("Unexpected expression");
                    }

                    variableStore.SetValue(
                        identifier.Identifier.ValueText + "." + memberaccess.Name.Identifier.ValueText, result);
                    // sets on variable named like "input.out"
                }

                return result;
            }
            case BinaryExpressionSyntax binary: {
                switch (binary.OperatorToken.Kind()) {
                    case SyntaxKind.EqualsEqualsToken: {
                        int left = Evaluate(binary.Left, variableStore);
                        int right = Evaluate(binary.Right, variableStore);
                        return left == right ? 1 : 0;
                    }
                    case SyntaxKind.ExclamationEqualsToken: {
                        int left = Evaluate(binary.Left, variableStore);
                        int right = Evaluate(binary.Right, variableStore);
                        return left != right ? 1 : 0;
                    }
                    case SyntaxKind.LessThanEqualsToken: {
                        int left = Evaluate(binary.Left, variableStore);
                        int right = Evaluate(binary.Right, variableStore);
                        return left < right ? 1 : 0;
                    }
                    case SyntaxKind.GreaterThanEqualsToken: {
                        int left = Evaluate(binary.Left, variableStore);
                        int right = Evaluate(binary.Right, variableStore);
                        return left > right ? 1 : 0;
                    }
                    case SyntaxKind.LessThanToken: {
                        int left = Evaluate(binary.Left, variableStore);
                        int right = Evaluate(binary.Right, variableStore);
                        return left < right ? 1 : 0;
                    }
                    case SyntaxKind.GreaterThanToken: {
                        int left = Evaluate(binary.Left, variableStore);
                        int right = Evaluate(binary.Right, variableStore);
                        return left > right ? 1 : 0;
                    }
                    case SyntaxKind.PlusToken: {
                        int left = Evaluate(binary.Left, variableStore);
                        int right = Evaluate(binary.Right, variableStore);
                        return left + right;
                    }
                    case SyntaxKind.MinusToken: {
                        int left = Evaluate(binary.Left, variableStore);
                        int right = Evaluate(binary.Right, variableStore);
                        return left - right;
                    }
                    case SyntaxKind.AsteriskToken: {
                        int left = Evaluate(binary.Left, variableStore);
                        int right = Evaluate(binary.Right, variableStore);
                        return left * right;
                    }
                    case SyntaxKind.SlashToken: {
                        int left = Evaluate(binary.Left, variableStore);
                        int right = Evaluate(binary.Right, variableStore);
                        return left / right;
                    }
                    case SyntaxKind.AmpersandToken: {
                        int left = Evaluate(binary.Left, variableStore);
                        int right = Evaluate(binary.Right, variableStore);
                        return left & right;
                    }
                    case SyntaxKind.AmpersandAmpersandToken: {
                        int left = Evaluate(binary.Left, variableStore);
                        int right = Evaluate(binary.Right, variableStore);
                        return left > 0 && right > 0 ? 1 : 0;
                    }
                    case SyntaxKind.BarToken: {
                        int left = Evaluate(binary.Left, variableStore);
                        int right = Evaluate(binary.Right, variableStore);
                        return left | right;
                    }
                    case SyntaxKind.BarBarToken: {
                        int left = Evaluate(binary.Left, variableStore);
                        int right = Evaluate(binary.Right, variableStore);
                        return left > 0 || right > 0 ? 1 : 0;
                    }
                    case SyntaxKind.CaretToken: {
                        int left = Evaluate(binary.Left, variableStore);
                        int right = Evaluate(binary.Right, variableStore);
                        return left ^ right;
                    }
                }
                break;
            }
            case ParenthesizedExpressionSyntax parenthesized: {
                return Evaluate(parenthesized.Expression, variableStore);
            }
            case LiteralExpressionSyntax literal: {
                return (int)(literal.Token.Value ?? throw new Exception("Unexpected value"));
            }
            case IdentifierNameSyntax identifier: {
                return variableStore.GetValue(identifier.Identifier.ValueText);
            }
        }
        throw new Exception("Unexpected expression: " + expression.GetType().Name + "(Kind: " + expression.Kind() + ")");
    }

    #endregion
    

    public T GetInputValue<T>(DesignBlock block, IoItem item) {
        return default;
    }

    public T GetOutputValue<T>(DesignBlock block, IoItem item) {
        return default;
    }

    public void Dispose() {
        
    }

    private class VariableStore {

        private Dictionary<string, int> values = [];
        
        public void Allocate(string name) {
            values[name] = 0;
        }
        
        public int GetValue(string name) {
            if (values.TryGetValue(name, out int value)) {
                return value;
            }
            return 0;
        }

        public void SetValue(string name, int value) {
            values[name] = value;
        }
    }
}
