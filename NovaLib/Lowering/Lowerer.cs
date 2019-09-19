using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Nova.CodeAnalysis.Binding;
using Nova.CodeAnalysis.Syntax;

namespace Nova.CodeAnalysis.Lowering
{
    internal sealed class Lowerer : BoundTreeRewriter
    {
        private int labelCount;

        private Lowerer()
        {
        }

        private LabelSymbol GenerateLabel()
        {
            string name = $"Label {++labelCount}";
            return new LabelSymbol(name);
        }

        public static BoundBlockStatement Lower(BoundStatement statement)
        {
            Lowerer lowerer = new Lowerer();
            BoundStatement result = lowerer.RewriteStatement(statement);
            return Flatten(result);
        }

        private static BoundBlockStatement Flatten(BoundStatement statement)
        {
            var builder = ImmutableArray.CreateBuilder<BoundStatement>();
            Stack<BoundStatement> stack = new Stack<BoundStatement>();
            stack.Push(statement);

            while (stack.Count > 0)
            {
                BoundStatement current = stack.Pop();

                if (current is BoundBlockStatement block)
                {
                    foreach (BoundStatement s in block.Statements.Reverse())
                        stack.Push(s);
                }
                else
                {
                    builder.Add(current);
                }
            }

            return new BoundBlockStatement(builder.ToImmutable());
        }

        protected override BoundStatement RewriteIfStatement(BoundIfStatement node)
        {
            if (node.ElseStatement == null)
            {
                LabelSymbol endLabel = GenerateLabel();

                BoundConditionalGotoStatement gotoFalse = new BoundConditionalGotoStatement(endLabel, node.Condition, true);
                BoundLabelStatement endLabelStatement = new BoundLabelStatement(endLabel);
                BoundBlockStatement result = new BoundBlockStatement(ImmutableArray.Create<BoundStatement>(gotoFalse, node.ThenStatement, endLabelStatement));

                return RewriteStatement(result);
            }
            else
            {
                LabelSymbol elseLabel = GenerateLabel();
                LabelSymbol endLabel = GenerateLabel();

                BoundConditionalGotoStatement gotoFalse = new BoundConditionalGotoStatement(elseLabel, node.Condition, true);
                BoundGotoStatement gotoEndStatement = new BoundGotoStatement(endLabel);
                BoundLabelStatement elseLabelStatement = new BoundLabelStatement(elseLabel);
                BoundLabelStatement endLabelStatement = new BoundLabelStatement(endLabel);
                BoundBlockStatement result = new BoundBlockStatement(ImmutableArray.Create<BoundStatement>(
                    gotoFalse,
                    node.ThenStatement,
                    gotoEndStatement,
                    elseLabelStatement,
                    node.ElseStatement,
                    endLabelStatement
                ));
                
                return RewriteStatement(result);
            }
        }

        protected override BoundStatement RewriteWhileStatement(BoundWhileStatement node)
        {
            LabelSymbol continueLabel = GenerateLabel();
            LabelSymbol checkLabel = GenerateLabel();
            LabelSymbol endLabel = GenerateLabel();

            BoundGotoStatement gotoCheck = new BoundGotoStatement(checkLabel);
            BoundLabelStatement continueLabelStatement = new BoundLabelStatement(continueLabel);
            BoundLabelStatement checkLabelStatement = new BoundLabelStatement(checkLabel);
            BoundConditionalGotoStatement gotoTrue = new BoundConditionalGotoStatement(continueLabel, node.Condition, false);
            BoundLabelStatement endLabelStatement = new BoundLabelStatement(endLabel);
            BoundBlockStatement result = new BoundBlockStatement(ImmutableArray.Create<BoundStatement>(
                gotoCheck,
                continueLabelStatement,
                node.Body,
                checkLabelStatement,
                gotoTrue,
                endLabelStatement
            ));
            
            return RewriteStatement(result);
        }

        protected override BoundStatement RewriteForStatement(BoundForStatement node)
        {
            BoundVariableDeclaration variableDeclaration = new BoundVariableDeclaration(node.Variable, node.LowerBound);
            BoundVariableExpression variableExpression = new BoundVariableExpression(node.Variable);

            BoundBinaryExpression condition = new BoundBinaryExpression(
                variableExpression,
                BoundBinaryOperator.Bind(SyntaxKind.LessOrEqualsToken, typeof(int), typeof(int)),
                node.UpperBound
            );

            BoundExpressionStatement increment = new BoundExpressionStatement(
                new BoundAssignmentExpression(
                    node.Variable,
                    new BoundBinaryExpression(
                        variableExpression,
                        BoundBinaryOperator.Bind(SyntaxKind.PlusToken, typeof(int), typeof(int)),
                        new BoundLiteralExpression(1)
                    )
                )
            );

            BoundBlockStatement whileBody = new BoundBlockStatement(ImmutableArray.Create<BoundStatement>(node.Body, increment));
            BoundWhileStatement whileStatement = new BoundWhileStatement(condition, whileBody);

            BoundBlockStatement result = new BoundBlockStatement(ImmutableArray.Create<BoundStatement>(variableDeclaration, whileStatement));
            return RewriteStatement(result);
        }
    }
}