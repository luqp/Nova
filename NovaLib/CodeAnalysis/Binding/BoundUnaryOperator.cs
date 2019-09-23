using System;
using Nova.CodeAnalysis.Symbols;
using Nova.CodeAnalysis.Syntax;

namespace Nova.CodeAnalysis.Binding
{
    internal sealed class BoundUnaryOperator
    {
        public BoundUnaryOperator(SyntaxKind syntaxKind, BoundUnaryOperatorKind kind, TypeSymbol operatorType)
        : this(syntaxKind, kind, operatorType, operatorType)
        {
        }

        public BoundUnaryOperator(SyntaxKind syntaxKind, BoundUnaryOperatorKind kind, TypeSymbol operandType, TypeSymbol resultType)
        {
            SyntaxKind = syntaxKind;
            Kind = kind;
            OperandType = operandType;
            Type = resultType;
        }

        public SyntaxKind SyntaxKind { get; }
        public BoundUnaryOperatorKind Kind { get; }
        public TypeSymbol OperandType { get; }
        public TypeSymbol Type { get; }

        private static BoundUnaryOperator[] operators =
        {
            new BoundUnaryOperator(SyntaxKind.BangToken, BoundUnaryOperatorKind.LogicalNegation, TypeSymbol.Bool),

            new BoundUnaryOperator(SyntaxKind.PlusToken, BoundUnaryOperatorKind.Identity, TypeSymbol.Int),
            new BoundUnaryOperator(SyntaxKind.MinusToken, BoundUnaryOperatorKind.Negation, TypeSymbol.Int),
            new BoundUnaryOperator(SyntaxKind.TildeToken, BoundUnaryOperatorKind.OnesComplement, TypeSymbol.Int)
        };

        public static BoundUnaryOperator Bind(SyntaxKind syntaxKind, TypeSymbol operandType)
        {
            foreach (BoundUnaryOperator op in operators)
            {
                if (op.SyntaxKind == syntaxKind && op.OperandType == operandType)
                    return op;
            }

            return null;
        }

    }
}