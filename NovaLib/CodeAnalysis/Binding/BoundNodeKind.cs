namespace Nova.CodeAnalysis.Binding
{
    internal enum BoundNodeKind
    {
        // Statemens
        
        BlockStatement,
        ExpressionStatement,
        VariableDeclaration,

        // Expressions
        LiteralExpression,
        VariableExpression,
        AssignmentExpression,
        UnaryExpression,
        BinaryExpression
    }
}