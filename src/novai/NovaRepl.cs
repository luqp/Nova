using System;
using System.Collections.Generic;
using System.Linq;
using Nova.CodeAnalysis;
using Nova.CodeAnalysis.Symbols;
using Nova.CodeAnalysis.Syntax;
using Nova.CodeAnalysis.Text;

namespace Nova
{
    internal sealed class NovaRepl : Repl
    {
        private Compilation previous;
        private bool showTree;
        private bool showProgram;
        private readonly Dictionary<VariableSymbol, object> variables = new Dictionary<VariableSymbol, object>();

        protected override void RenderLine(string line)
        {
            IEnumerable<SyntaxToken> tokens = SyntaxTree.ParseTokens(line);
            foreach (SyntaxToken token in tokens)
            {
                bool isKeyword = token.Kind.ToString().EndsWith("Keyword");
                bool isIdentifier = token.Kind == SyntaxKind.IdentifierToken;
                bool isNumber = token.Kind == SyntaxKind.NumberToken;
                bool isString = token.Kind == SyntaxKind.StringToken;
    
                if (isKeyword)
                    Console.ForegroundColor = ConsoleColor.Blue;
                else if (isIdentifier)
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                else if (isNumber)
                    Console.ForegroundColor = ConsoleColor.Cyan;
                else if (isString)
                    Console.ForegroundColor = ConsoleColor.Green;
                else
                    Console.ForegroundColor = ConsoleColor.DarkGray;

                Console.Write(token.Text);
                Console.ResetColor();
            }

        }

        protected override void EvaluateMetaCommand(string input)
        {
            switch (input)
            {
                case "#trees":
                    showTree = !showTree;
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine(showTree ? "Enabled parse trees." : "Disabled parse trees.");
                    break;
                case "#showProgram":
                    showProgram = !showProgram;
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine(showProgram ? "Enabled bound trees." : "Disabled bound trees.");
                    break;
                case "#cls":
                    Console.Clear();
                    break;
                case "#reset":
                    previous = null;
                    variables.Clear();
                    break;
                default:
                    base.EvaluateMetaCommand(input);
                    break;
            }
        }

        protected override bool IsCompleteSubmission(string text)
        {            
            if (string.IsNullOrEmpty(text))
                return true;
            
            bool lastTwoLineAreBlank = text.Split(Environment.NewLine)
                                          .Reverse()
                                          .TakeWhile(s => string.IsNullOrEmpty(s))
                                          .Take(2)
                                          .Count() == 2;

            if (lastTwoLineAreBlank)
                return true;

            SyntaxTree syntaxTree = SyntaxTree.Parse(text);

            if (syntaxTree.Root.Members.Last().GetLastToken().IsMissing)
                return false;
            
            return true;
        }

        protected override void EvaluateSubmission(string text)
        {
            SyntaxTree syntaxTree = SyntaxTree.Parse(text);

            Compilation compilation = previous == null
                                        ? new Compilation(syntaxTree)
                                        : previous.ContinueWith(syntaxTree);

            if (showTree)
                syntaxTree.Root.WriteTo(Console.Out);

            if (showProgram)
                compilation.EmitTree(Console.Out);

            EvaluationResult result = compilation.Evaluate(variables);
            IReadOnlyList<Diagnostic> diagnostics = result.Diagnostics;

            if (!diagnostics.Any())
            {
                if (result.Value != null)
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine(result.Value);
                    Console.ResetColor();
                }

                previous = compilation;
            }
            else
            {
                foreach (Diagnostic diagnostic in diagnostics.OrderBy(d => d.Span, new TextSpanComparer()))
                {
                    SourceText treeText = syntaxTree.Text;
                    int lineIndex = treeText.GetLineIndex(diagnostic.Span.Start);
                    var line = treeText.Lines[lineIndex];
                    int lineNumber = lineIndex + 1;
                    int character = diagnostic.Span.Start - line.Start + 1;

                    Console.WriteLine();

                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.Write($"({lineNumber}, {character}): ");
                    Console.WriteLine(diagnostic);
                    Console.ResetColor();

                    TextSpan prefixSpan = TextSpan.FromBounds(line.Start, diagnostic.Span.Start);
                    TextSpan suffixSpan = TextSpan.FromBounds(diagnostic.Span.End, line.End);

                    string prefix = treeText.ToString(prefixSpan);
                    string error = treeText.ToString(diagnostic.Span);
                    string suffix = treeText.ToString(suffixSpan);

                    Console.Write($"   {prefix}");
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.Write(error);
                    Console.ResetColor();
                    Console.Write(suffix);
                    Console.WriteLine();
                }
                Console.WriteLine();
            }
        }
    }
}
