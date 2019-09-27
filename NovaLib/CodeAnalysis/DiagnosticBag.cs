using System;
using System.Collections;
using System.Collections.Generic;
using Nova.CodeAnalysis.Symbols;
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

        public void ReportInvalidNumber(TextSpan span, string tokenText, TypeSymbol type)
        {
            string message = $"The number '{tokenText}' isn't valid <{type}>.";
            Report(span, message);
        }

        public void ReportBadCharacter(int position, char character)
        {
            TextSpan span = new TextSpan(position, 1);
            string message = $"Bad character input '{character}'.";
            Report(span, message);
        }

        internal void ReportUnterminatedString(TextSpan span)
        {
            string message = "Unterminated string literal.";
            Report(span, message);
        }

        public void ReportUnexpectedToken(TextSpan span, SyntaxKind currendKind, SyntaxKind expectedKind)
        {
            string message = $"Unexpected token <{currendKind}>, expected <{expectedKind}>.";
            Report(span, message);
        }

        public void ReportUndefinedUnaryOperator(TextSpan span, string operatorText, TypeSymbol operandType)
        {
            string message = $"Unary operator '{operatorText}' is not defined for type <{operandType}>.";
            Report(span, message);
        }

        public void ReportUndefinedBinaryOperator(TextSpan span, string operatorText, TypeSymbol leftType, TypeSymbol rightType)
        {
            string message = $"Binary operator '{operatorText}' is not defined for types <{leftType}> and <{rightType}>.";
            Report(span, message);
        }

        public void ReportUndefinedName(TextSpan span, string name)
        {
            string message = $"Variable '{name}' doesn't exist.";
            Report(span, message);
        }

        public void ReportUndefinedType(TextSpan span, string name)
        {
            string message = $"Type {name} doesn't exist";
            Report(span, message);
        }

        public void ReportCannotConvert(TextSpan span, TypeSymbol fromType, TypeSymbol toType)
        {
            string message = $"Cannot convert type '{fromType}' to '{toType}'.";
            Report(span, message);
        }

        public void ReportCannotConvertImplicitly(TextSpan span, TypeSymbol fromType, TypeSymbol toType)
        {
            string message = $"Cannot convert type '{fromType}' to '{toType}'. An explicit conversion exists (are you missing a cast?)";
            Report(span, message);
        }

        public void ReportSymbolAlreadyDeclared(TextSpan span, string name)
        {
            string message = $"'{name}' is already declared.";
            Report(span, message);
        }

        public void ReportCannotAssign(TextSpan span, string name)
        {
            string message = $"Variable '{name}' is read-only and cannot be assigned to.";
            Report(span, message);
        }

        public void ReportUndefinedFunction(TextSpan span, string name)
        {
            string message = $"'{name}' is not a function.";
            Report(span, message);
        }

        public void ReportWrongArgumentCount(TextSpan span, string name, int expectedCount, int actualCount)
        {
            string message = $"Function '{name}' requires {expectedCount} arguments but was given {actualCount}.";
            Report(span, message);
        }

        public void ReportWrongArgumentType(TextSpan span, string name, TypeSymbol expectedType, TypeSymbol actualType)
        {
            string message = $"Parameter '{name}' requires a value of type <{expectedType}> but was given a value of type <{actualType}>.";
            Report(span, message);
        }

        public void ReportExpressionMustHaveValue(TextSpan span)
        {
            string message = "Expression must be have a value";
            Report(span, message);
        }
    }
}