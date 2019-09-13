namespace Nova.CodeAnalysis.Binding
{
    internal enum BoundNodeKind
    {
        // Statemens
        
        BlockStatement,
        ExpressionStatement,
        VariableDeclaration,
        IfStatement,

        // Expressions
        LiteralExpression,
        VariableExpression,
        AssignmentExpression,
        UnaryExpression,
        BinaryExpression
    }
}