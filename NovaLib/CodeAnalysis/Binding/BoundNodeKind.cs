namespace Nova.CodeAnalysis.Binding
{
    internal enum BoundNodeKind
    {
        // Statemens
        
        BlockStatement,
        ExpressionStatement,

        // Expressions
        LiteralExpression,
        VariableExpression,
        AssignmentExpression,
        UnaryExpression,
        BinaryExpression
    }
}