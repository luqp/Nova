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
        AmpersandAmpersandToken,
        PipePipeToken,
        LessToken,
        LessOrEqualsToken,
        GreaterToken,
        GreaterOrEqualsToken,
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
        IfKeyword,
        ElseKeyword,
        WhileKeyword,
        ForKeyword,

        // Nodes
        CompilationUnit,
        ElseClause,

        // Statements
        VariableDeclaration,
        ExpressionStatement,
        BlockStatement,
        IfStatement,
        WhileStatement,
        ForStatement,
        ToKeyword,

        // Expressions
        LiteralExpression,
        NameExpression,
        UnaryExpression,
        BinaryExpression,
        ParenthesizedExpression,
        AssignmentExpression,
    }
}