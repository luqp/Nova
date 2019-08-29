using System;

namespace Nova.CodeAnalysis.Binding
{
    internal abstract class BoundExpression : BoundNode
    {
        public abstract Type Type { get; }
    }
}