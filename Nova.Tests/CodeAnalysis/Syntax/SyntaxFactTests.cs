using System;
using System.Collections.Generic;
using Nova.CodeAnalysis.Syntax;
using Xunit;

namespace Nova.Tests.CodeAnalysis.Syntax
{
    public class SyntaxFactTests
    {
        [Theory]
        [MemberData(nameof(GetSyntaxKindData))]
        public void SyntaxGetTextRoundTrips(SyntaxKind kind)
        {
            string text = SyntaxFacts.GetText(kind);

            if (text == null)
                return;
            
            IEnumerable<SyntaxToken> tokens = SyntaxTree.ParseTokens(text);
            SyntaxToken token = Assert.Single(tokens);
            Assert.Equal(kind, token.Kind);
            Assert.Equal(text, token.Text);
        }

        public static IEnumerable<object[]> GetSyntaxKindData()
        {
            SyntaxKind[] kinds = (SyntaxKind[]) Enum.GetValues(typeof(SyntaxKind));
            foreach (SyntaxKind kind in kinds)
                yield return new object[] { kind };
        }
    }
}
