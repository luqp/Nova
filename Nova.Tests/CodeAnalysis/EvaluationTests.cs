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
        [InlineData("55 < 55", false)]
        [InlineData("56 < 55", false)]
        [InlineData("50 < 100", true)]
        [InlineData("5 > 6", false)]
        [InlineData("5 > 5", false)]
        [InlineData("5 > 4", true)]
        [InlineData("2 <= 4", true)]
        [InlineData("2 <= 2", true)]
        [InlineData("5 <= 3", false)]
        [InlineData("7 >= 5", true)]
        [InlineData("5 >= 5", true)]
        [InlineData("4 >= 5", false)]
        [InlineData("false == false", true)]
        [InlineData("true == false", false)]
        [InlineData("false != true", true)]
        [InlineData("true != true", false)]
        [InlineData("true", true)]
        [InlineData("false", false)]
        [InlineData("!true", false)]
        [InlineData("!false", true)]
        [InlineData("{ var x = 0 (x = 5) * x }", 25)]
        [InlineData("{ var x = 0 if x == 0 x = 3 x }", 3)]
        [InlineData("{ var x = 0 if x == 4 x = 8 x }", 0)]
        [InlineData("{ var x = 0 if x == 0 x = 3 else x = 9 x }", 3)]
        [InlineData("{ var x = 0 if x == 4 x = 8 else x = 9 x }", 9)]
        [InlineData("{ var x = 0 if true x = 3 else x = 9 x }", 3)]
        [InlineData("{ var x = 0 if false x = 8 else x = 9 x }", 9)]
        [InlineData("{ var i = 0 var result = 10 while i < 10  { result = result + i i = i + 1 } result }", 55)]
        public void EvaluateResult(string text, object expectedValue)
        {
            AssertValue(text, expectedValue);
        }

        [Fact]
        public void EvaluatorVaribleDeclarationReportsRedeclaration()
        {
            var text = @"
                {
                    var x = 10
                    var y = 100
                    {
                        var x = 2
                    }
                    var [x] = 5
                }
            ";

            var diagnostics = @"
                Variable 'x' is already declared.
            ";

            AssertDiagnostics(text, diagnostics);
        }

        [Fact]
        public void EvaluatorNameReportsUndefined()
        {
            var text = @"[x] * 10";

            var diagnostics = @"
                Variable 'x' doesn't exist.
            ";

            AssertDiagnostics(text, diagnostics);
        }

        [Fact]
        public void EvaluatorAssignedReportsUndefined()
        {
            var text = @"
                {
                    let x = 40
                    x [=] 10
                }
            ";

            var diagnostics = @"
                Variable 'x' is read-only and cannot be assigned to.
            ";

            AssertDiagnostics(text, diagnostics);
        }

        [Fact]
        public void EvaluatorAssignedReportsCannotConvert()
        {
            var text = @"
                {
                    var x = 3
                    x = [true]
                }
            ";

            var diagnostics = @"
                Cannot convert type 'System.Boolean' to 'System.Int32'.
            ";

            AssertDiagnostics(text, diagnostics);
        }

        [Fact]
        public void EvaluatorUnaryReportsUndefined()
        {
            var text = @"[+] true";

            var diagnostics = @"
                Unary operator '+' is not defined for type <System.Boolean>.
            ";

            AssertDiagnostics(text, diagnostics);
        }
        
        [Fact]
        public void EvaluatorBinaryReportsUndefined()
        {
            var text = @"1 [*] true";

            var diagnostics = @"
                Binary operator '*' is not defined for types <System.Int32> and <System.Boolean>.
            ";

            AssertDiagnostics(text, diagnostics);
        }

        private static void AssertValue(string text, object expectedValue)
        {
            SyntaxTree syntaxTree = SyntaxTree.Parse(text);
            Compilation compilation = new Compilation(syntaxTree);
            Dictionary<VariableSymbol, object> variables = new Dictionary<VariableSymbol, object>();
            EvaluationResult result = compilation.Evaluate(variables);

            Assert.Empty(result.Diagnostics);
            Assert.Equal(expectedValue, result.Value);
        }

        private void AssertDiagnostics(string text, string diagnosticText)
        {
            AnnotatedText annotatedText = AnnotatedText.Parse(text);
            SyntaxTree syntaxTree = SyntaxTree.Parse(annotatedText.Text);
            Compilation compilation = new Compilation(syntaxTree);
            EvaluationResult result = compilation.Evaluate(new Dictionary<VariableSymbol, object>());

            string[] expectedDiagnostics = AnnotatedText.UnindentLines(diagnosticText);

            if (annotatedText.Spans.Length != expectedDiagnostics.Length)
                throw new Exception("Error: Must mark as many spans as there are expected diagnostics.");

            Assert.Equal(expectedDiagnostics.Length, result.Diagnostics.Length);

            for (int i = 0; i < expectedDiagnostics.Length; i++)
            {
                string expectedMessage = expectedDiagnostics[i];
                string actualMessage = result.Diagnostics[i].Message;
                Assert.Equal(expectedMessage, actualMessage);

                var expectedSpan = annotatedText.Spans[i];
                var actualSpan = result.Diagnostics[i].Span;
                Assert.Equal(expectedSpan, actualSpan);
            }
        }
    }
}
