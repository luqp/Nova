using System;
using System.Collections;
using System.Collections.Generic;
using Nova.CodeAnalysis.Syntax;
using Nova.CodeAnalysis.Text;

namespace Nova.CodeAnalysis
{
    internal sealed class DiagnosticBag : IEnumerable<Diagnostic>
    {
        private readonly List<Diagnostic> diagnostics = new List<Diagnostic>();

        public IEnumerator<Diagnostic> GetEnumerator() => diagnostics.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void AddRange(DiagnosticBag diagnostics)
        {
            this.diagnostics.AddRange(diagnostics.diagnostics);
        }

        private void Report(TextSpan span, string message)
        {
            Diagnostic diagnostic = new Diagnostic(span, message);
            diagnostics.Add(diagnostic);
        }

        public void ReportInvalidNumber(TextSpan span, string tokenText, Type type)
        {
            string message = $"The number {tokenText} isn't valid {type}.";
            Report(span, message);
        }

        public void ReportBadCharacter(int position, char character)
        {
            TextSpan span = new TextSpan(position, 1);
            string message = $"Bad character input '{character}'.";
            Report(span, message);
        }

        public void ReportUnexpectedToken(TextSpan span, SyntaxKind currendKind, SyntaxKind expectedKind)
        {
            string message = $"Unexpected token <{currendKind}>, expected <{expectedKind}>.";
            Report(span, message);
        }

        public void ReportUndefinedUnaryOperator(TextSpan span, string operatorText, Type operandType)
        {
            string message = $"Unary operator '{operatorText}' is not defined for type <{operandType}>.";
            Report(span, message);
        }

        public void ReportUndefinedBinaryOperator(TextSpan span, string operatorText, Type leftType, Type rightType)
        {
            string message = $"Binary operator '{operatorText}' is not defined for types <{leftType}> and <{rightType}>.";
            Report(span, message);
        }

        public void ReportUndefinedName(TextSpan span, string name)
        {
            string message = $"Variable {name} doesn't exist.";
            Report(span, message);
        }

        public void ReportVariableAlreadyDeclared(TextSpan span, string name)
        {
            string message = $"Variable {name} is already declared.";
            Report(span, message);
        }
    }
}