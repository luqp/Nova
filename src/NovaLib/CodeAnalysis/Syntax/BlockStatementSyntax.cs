using System.Collections.Immutable;

namespace Nova.CodeAnalysis.Syntax
{
    public sealed class BlockStatementSyntax : StatementSyntax
    {
        public BlockStatementSyntax(SyntaxToken openBranceToken, ImmutableArray<StatementSyntax> statements, SyntaxToken closeBranceToken)
        {
            OpenBranceToken = openBranceToken;
            Statements = statements;
            CloseBranceToken = closeBranceToken;
        }

        public override SyntaxKind Kind => SyntaxKind.BlockStatement;
        public SyntaxToken OpenBranceToken { get; }
        public ImmutableArray<StatementSyntax> Statements { get; }
        public SyntaxToken CloseBranceToken { get; }

    }
}