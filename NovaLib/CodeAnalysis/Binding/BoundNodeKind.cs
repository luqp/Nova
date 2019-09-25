namespace Nova.CodeAnalysis.Binding
{
    internal enum BoundNodeKind
    {
        // Statemens
        VariableDeclaration,
        ExpressionStatement,
        BlockStatement,
        IfStatement,
        WhileStatement,
        ForStatement,
        LabelStatement,
        GotoStatement,
        ConditionalGotoStatement,

        // Expressions
        ErrorExpression,
        LiteralExpression,
        VariableExpression,
        AssignmentExpression,
        UnaryExpression,
        BinaryExpression,
        CallExpression,
        ConversionExpression,
    }
}