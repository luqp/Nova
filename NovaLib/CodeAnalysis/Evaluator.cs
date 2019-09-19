using System;
using System.Collections.Generic;
using Nova.CodeAnalysis.Binding;

namespace Nova.CodeAnalysis
{
    internal sealed class Evaluator
    {
        private readonly BoundBlockStatement root;
        private readonly Dictionary<VariableSymbol, object> variables;

        private object lastValue;

        public Evaluator(BoundBlockStatement root, Dictionary<VariableSymbol, object> variables)
        {
            this.root = root;
            this.variables = variables;
        }

        public object Evaluate()
        {
            var labelToIndex = new Dictionary<LabelSymbol, int>();

            for (int i = 0; i < root.Statements.Length; i++)
            {
                if (root.Statements[i] is BoundLabelStatement l)
                    labelToIndex.Add(l.Label, i + 1);
            }

            int index = 0;
            while (index < root.Statements.Length)
            {
                BoundStatement s = root.Statements[index];

                switch (s.Kind)
                {
                    case BoundNodeKind.VariableDeclaration:
                        EvaluateVariableDeclaration((BoundVariableDeclaration)s);
                        index++;
                        break;
                    case BoundNodeKind.ExpressionStatement:
                        EvaluateExpressionStatement((BoundExpressionStatement)s);
                        index++;
                        break;
                    case BoundNodeKind.GotoStatement:
                        BoundGotoStatement gs = (BoundGotoStatement)s;
                        index = labelToIndex[gs.Label];
                        break;
                    case BoundNodeKind.ConditionalGotoStatement:
                        BoundConditionalGotoStatement cgs = (BoundConditionalGotoStatement)s;
                        bool condition = (bool)EvaluateExpression(cgs.Condition);
                        if (condition && !cgs.JumpIfFalse ||
                            !condition && cgs.JumpIfFalse)
                            index = labelToIndex[cgs.Label];
                        else
                            index++;
                        break;
                    case BoundNodeKind.LabelStatement:
                        index++;
                        break;
                    default:
                        throw new Exception($"Unexpected node <{s.Kind}>");
                }
            }

            return lastValue;
        }

        private void EvaluateVariableDeclaration(BoundVariableDeclaration node)
        {
            object value = EvaluateExpression(node.Initializer);
            variables[node.Variable] = value;
            lastValue = value;
        }

        private void EvaluateExpressionStatement(BoundExpressionStatement node)
        {
            lastValue = EvaluateExpression(node.Expression);
        }

        private object EvaluateExpression(BoundExpression node)
        {
            switch (node.Kind)
            {
                case BoundNodeKind.LiteralExpression:
                    return EvaluateLiteralExpression((BoundLiteralExpression)node);
                case BoundNodeKind.VariableExpression:
                    return EvaluateVariableExpression((BoundVariableExpression)node);
                case BoundNodeKind.AssignmentExpression:
                    return EvaluateAssignmentExpression((BoundAssignmentExpression)node);
                case BoundNodeKind.UnaryExpression:
                    return EvaluateUnaryExpression((BoundUnaryExpression)node);
                case BoundNodeKind.BinaryExpression:
                    return EvaluateBinaryExpression((BoundBinaryExpression)node);
                default:
                    throw new Exception($"Unexpected node <{node.Kind}>");
            }
        }

        private static object EvaluateLiteralExpression(BoundLiteralExpression n)
        {
            return n.Value;
        }

        private object EvaluateVariableExpression(BoundVariableExpression v)
        {
            return variables[v.Variable];
        }

        private object EvaluateAssignmentExpression(BoundAssignmentExpression a)
        {
            object value = EvaluateExpression(a.Expression);
            variables[a.Variable] = value;
            return value;
        }

        private object EvaluateUnaryExpression(BoundUnaryExpression u)
        {
            var operand = EvaluateExpression(u.Operand);

            switch (u.Op.Kind)
            {
                case BoundUnaryOperatorKind.Identity:
                    return (int)operand;
                case BoundUnaryOperatorKind.Negation:
                    return -(int)operand;
                case BoundUnaryOperatorKind.LogicalNegation:
                    return !(bool)operand;
                case BoundUnaryOperatorKind.OnesComplement:
                    return ~(int)operand;
                default:
                    throw new Exception($"Unexpected unary operator {u.Op}");
            }
        }

        private object EvaluateBinaryExpression(BoundBinaryExpression b)
        {
            var left = EvaluateExpression(b.Left);
            var right = EvaluateExpression(b.Right);

            switch (b.Op.Kind)
            {
                case BoundBinaryOperatorKind.Addition:
                    return (int)left + (int)right;
                case BoundBinaryOperatorKind.Subtraction:
                    return (int)left - (int)right;
                case BoundBinaryOperatorKind.Multiplication:
                    return (int)left * (int)right;
                case BoundBinaryOperatorKind.Division:
                    return (int)left / (int)right;
                case BoundBinaryOperatorKind.BitwiseAnd:
                    if (b.Type == (typeof(int)))
                        return (int)left & (int)right;
                    else
                        return (bool)left & (bool)right;
                case BoundBinaryOperatorKind.BitwiseOr:
                    if (b.Type == (typeof(int)))
                        return (int)left | (int)right;
                    else
                        return (bool)left | (bool)right;
                case BoundBinaryOperatorKind.BitwiseXor:
                    if (b.Type == (typeof(int)))
                        return (int)left ^ (int)right;
                    else
                        return (bool)left ^ (bool)right;
                case BoundBinaryOperatorKind.LogicalAnd:
                    return (bool)left && (bool)right;
                case BoundBinaryOperatorKind.LogicalOr:
                    return (bool)left || (bool)right;
                case BoundBinaryOperatorKind.Equals:
                    return Equals(left, right);
                case BoundBinaryOperatorKind.NotEquals:
                    return !Equals(left, right);
                case BoundBinaryOperatorKind.Less:
                    return (int)left < (int)right;
                case BoundBinaryOperatorKind.Greater:
                    return (int)left > (int)right;
                case BoundBinaryOperatorKind.LessOrEquals:
                    return (int)left <= (int)right;
                case BoundBinaryOperatorKind.GreaterOrEquals:
                    return (int)left >= (int)right;
                default:
                    throw new Exception($"Unexpected binary operator <{b.Op}>");
            }
        }
    }
}