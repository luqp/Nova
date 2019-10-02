namespace Nova.CodeAnalysis.Binding
{
    internal sealed class BoundDoWhileStatement : BoundLoopStatement
    {
        public BoundDoWhileStatement(BoundExpression condition, BoundStatement body, BoundLabel breakLabel, BoundLabel continueLabel)
            : base(breakLabel, continueLabel)
        {
            Condition = condition;
            Body = body;
        }

        public override BoundNodeKind Kind => BoundNodeKind.DoWhileStatement;
        public BoundExpression Condition { get; }
        public BoundStatement Body { get; }
    }
}