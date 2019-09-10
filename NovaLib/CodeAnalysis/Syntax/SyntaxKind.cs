namespace Nova.CodeAnalysis.Syntax
{
    public enum SyntaxKind
    {
        // Tokens
        BadToken,
        EndOfFileToken,
        WhiteSpaceToken,
        NumberToken,
        PlusToken,
        MinusToken,
        StarToken,
        SlashToken,
        BangToken,
        EqualsToken,
        BangEqualsToken,
        EqualsEqualsToken,
        AmpersanAmpersanToken,
        PipePipeToken,
        OpenParenthesisToken,
        CloseParenthesisToken,
        OpenBraceToken,
        CloseBraceToken,
        IdentifierToken,

        // Keywords
        FalseKeyword,
        TrueKeyword,
        LetKeyword,
        VarKeyword,

        // Nodes
        CompilationUnit,

        // Statements
        BlockStatement,
        ExpressionStatement,
        VariableDeclaration,

        // Expressions
        LiteralExpression,
        NameExpression,
        UnaryExpression,
        BinaryExpression,
        ParenthesizedExpression,
        AssignmentExpression
    }
}