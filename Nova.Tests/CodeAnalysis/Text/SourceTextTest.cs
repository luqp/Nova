using Nova.CodeAnalysis.Text;
using Xunit;

namespace Nova.Tests.CodeAnalysis.Text
{
    public class SourceTextTest
    {
        [Theory]
        [InlineData(".", 1)]
        [InlineData(".\r\n", 2)]
        [InlineData(".\r\n\r\n", 3)]
        public void SourceTextIncludesLastLine(string text, int expectedLineCount)
        {
            SourceText sourceText = SourceText.From(text);
            Assert.Equal(expectedLineCount, sourceText.Lines.Length);
        }
    }
}