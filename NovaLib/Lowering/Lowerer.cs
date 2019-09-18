using Nova.CodeAnalysis.Binding;

namespace Nova.CodeAnalysis.Lowering
{
    internal sealed class Lowerer : BoundTreeRewriter
    {
        private Lowerer()
        {
        }

        public static BoundStatement Lower(BoundStatement statement)
        {
            Lowerer lowerer = new Lowerer();
            return lowerer.RewriteStatement(statement);
        }
    }
}