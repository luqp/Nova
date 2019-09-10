using System;
using System.Collections.Generic;
using Nova.CodeAnalysis;
using Nova.CodeAnalysis.Syntax;
using Xunit;

namespace Nova.Tests.CodeAnalysis
{
    public class EvaluationTests
    {
        [Theory]
        [InlineData("1", 1)]
        [InlineData("+5", 5)]
        [InlineData("-2", -2)]
        [InlineData("9 + 1", 10)]
        [InlineData("9 - 15", -6)]
        [InlineData("2 * 7", 14)]
        [InlineData("20 / 5", 4)]
        [InlineData("(83)", 83)]
        [InlineData("55 == 55", true)]
        [InlineData("12 == 1", false)]
        [InlineData("12 != 1", true)]
        [InlineData("55 != 55", false)]
        [InlineData("false == false", true)]
        [InlineData("true == false", false)]
        [InlineData("false != true", true)]
        [InlineData("true != true", false)]
        [InlineData("true", true)]
        [InlineData("false", false)]
        [InlineData("!true", false)]
        [InlineData("!false", true)]
        [InlineData("{ var x = 0 (x = 5) * x }", 25)]
        public void EvaluateResult(string text, object expectedValue)
        {
            SyntaxTree syntaxTree = SyntaxTree.Parse(text);
            Compilation compilation = new Compilation(syntaxTree);
            Dictionary<VariableSymbol, object> variables = new Dictionary<VariableSymbol, object>();
            EvaluationResult result = compilation.Evaluate(variables);

            Assert.Empty(result.Diagnostics);
            Assert.Equal(expectedValue, result.Value);
        }
    }
}
