namespace Nova.CodeAnalysis.Binding
{
    internal sealed class BoundExpressionStatement : BoundStatement
    {
        public BoundExpressionStatement(BoundExpression expresion)
        {
            Expresion = expresion;
        }

        public override BoundNodeKind Kind => BoundNodeKind.ExpressionStatement;
        public BoundExpression Expresion { get; }
    }
}