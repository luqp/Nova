using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Nova.CodeAnalysis.Binding;
using Nova.CodeAnalysis.Symbols;
using Nova.CodeAnalysis.Syntax;

namespace Nova.CodeAnalysis.Lowering
{
    internal sealed class Lowerer : BoundTreeRewriter
    {
        private int labelCount;

        private Lowerer()
        {
        }

        private BoundLabel GenerateLabel()
        {
            string name = $"Label {++labelCount}";
            return new BoundLabel(name);
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
                BoundLabel endLabel = GenerateLabel();

                BoundConditionalGotoStatement gotoFalse = new BoundConditionalGotoStatement(endLabel, node.Condition, false);
                BoundLabelStatement endLabelStatement = new BoundLabelStatement(endLabel);
                BoundBlockStatement result = new BoundBlockStatement(ImmutableArray.Create<BoundStatement>(gotoFalse, node.ThenStatement, endLabelStatement));

                return RewriteStatement(result);
            }
            else
            {
                BoundLabel elseLabel = GenerateLabel();
                BoundLabel endLabel = GenerateLabel();

                BoundConditionalGotoStatement gotoFalse = new BoundConditionalGotoStatement(elseLabel, node.Condition, false);
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
            BoundLabel bodyLabel = GenerateLabel();
            BoundLabel endLabel = new BoundLabel("End");

            BoundGotoStatement gotoContinueLabel = new BoundGotoStatement(node.ContinueLabel);
            BoundLabelStatement bodyLabelStatement = new BoundLabelStatement(bodyLabel);
            BoundLabelStatement continueLabelStatement = new BoundLabelStatement(node.ContinueLabel);
            BoundConditionalGotoStatement gotoTrue = new BoundConditionalGotoStatement(bodyLabel, node.Condition, true);
            BoundLabelStatement breakLabelStatement = new BoundLabelStatement(node.BreakLabel);
            BoundLabelStatement endLabelStatement = new BoundLabelStatement(endLabel);
            BoundBlockStatement result = new BoundBlockStatement(ImmutableArray.Create<BoundStatement>(
                gotoContinueLabel,
                bodyLabelStatement,
                node.Body,
                continueLabelStatement,
                gotoTrue,
                breakLabelStatement,
                endLabelStatement
            ));
            
            return RewriteStatement(result);
        }

        protected override BoundStatement RewriteDoWhileStatement(BoundDoWhileStatement node)
        {
            BoundLabel bodyLabel = GenerateLabel();
            BoundLabel endLabel = new BoundLabel("End");

            BoundLabelStatement bodyLabelStatement = new BoundLabelStatement(bodyLabel);
            BoundLabelStatement continueLabelStatement = new BoundLabelStatement(node.ContinueLabel);
            BoundConditionalGotoStatement gotoTrue = new BoundConditionalGotoStatement(bodyLabel, node.Condition, true);
            BoundLabelStatement breakLabelStatement = new BoundLabelStatement(node.BreakLabel);
            BoundLabelStatement endLabelStatement = new BoundLabelStatement(endLabel);
            BoundBlockStatement result = new BoundBlockStatement(ImmutableArray.Create<BoundStatement>(
                bodyLabelStatement,
                node.Body,
                continueLabelStatement,
                gotoTrue,
                breakLabelStatement,
                endLabelStatement
            ));
            
            return RewriteStatement(result);
        }

        protected override BoundStatement RewriteForStatement(BoundForStatement node)
        {
            BoundVariableDeclaration variableDeclaration = new BoundVariableDeclaration(node.Variable, node.LowerBound);
            BoundVariableExpression variableExpression = new BoundVariableExpression(node.Variable);
            VariableSymbol upperVariableSymbol = new LocalVariableSymbol("upperBound", true, TypeSymbol.Int);
            BoundVariableDeclaration upperVariableDeclaration = new BoundVariableDeclaration(upperVariableSymbol, node.UpperBound);

            BoundBinaryExpression condition = new BoundBinaryExpression(
                variableExpression,
                BoundBinaryOperator.Bind(SyntaxKind.LessOrEqualsToken, TypeSymbol.Int, TypeSymbol.Int),
                new BoundVariableExpression(upperVariableSymbol)
            );
            BoundLabelStatement continueLabelStatement = new BoundLabelStatement(node.ContinueLabel);
            BoundExpressionStatement increment = new BoundExpressionStatement(
                new BoundAssignmentExpression(
                    node.Variable,
                    new BoundBinaryExpression(
                        variableExpression,
                        BoundBinaryOperator.Bind(SyntaxKind.PlusToken, TypeSymbol.Int, TypeSymbol.Int),
                        new BoundLiteralExpression(1)
                    )
                )
            );

            BoundBlockStatement whileBody = new BoundBlockStatement(ImmutableArray.Create<BoundStatement>(
                node.Body,
                continueLabelStatement,
                increment
            ));
            BoundWhileStatement whileStatement = new BoundWhileStatement(condition, whileBody, node.BreakLabel, GenerateLabel());

            BoundBlockStatement result = new BoundBlockStatement(ImmutableArray.Create<BoundStatement>(
                variableDeclaration,
                upperVariableDeclaration,
                whileStatement
            ));
            return RewriteStatement(result);
        }
    }
}