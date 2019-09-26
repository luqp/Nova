namespace Nova.CodeAnalysis.Binding
{
    internal sealed class BoundDoWhileStatement : BoundStatement
    {
        public BoundDoWhileStatement(BoundExpression condition, BoundStatement body)
        {
            Condition = condition;
            Body = body;
        }

        public override BoundNodeKind Kind => BoundNodeKind.DoWhileStatement;
        public BoundExpression Condition { get; }
        public BoundStatement Body { get; }
    }
}