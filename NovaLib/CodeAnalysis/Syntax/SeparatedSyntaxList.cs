using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Nova.CodeAnalysis.Syntax
{
    public sealed class SeparatedSyntaxList<T> : IEnumerable<T>
        where T: SyntaxNode
    {
        private readonly ImmutableArray<SyntaxNode> nodesAndSeparators;

        public SeparatedSyntaxList(ImmutableArray<SyntaxNode> nodesAndSeparators)
        {
            this.nodesAndSeparators = nodesAndSeparators;
        }

        public int Count => (nodesAndSeparators.Length + 1) / 2;

        public T this[int index] => (T) nodesAndSeparators[index * 2];

        public SyntaxToken GetSeparator(int index) => (SyntaxToken) nodesAndSeparators[index * 2 + 1];

        public ImmutableArray<SyntaxNode> GetWithSeparators() => nodesAndSeparators;

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
                yield return this[i];
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}