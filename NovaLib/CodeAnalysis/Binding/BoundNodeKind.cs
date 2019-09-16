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

        // Expressions
        LiteralExpression,
        VariableExpression,
        AssignmentExpression,
        UnaryExpression,
        BinaryExpression,
    }
}