using System;
using System.Collections.Generic;
using System.Linq;

using Nova.CodeAnalysis;
using Nova.CodeAnalysis.Binding;
using Nova.CodeAnalysis.Syntax;

namespace Nova
{
    internal static class Program
    {
        private static void Main()
        {
            bool showTree = false;
            Dictionary<VariableSymbol, object> variables = new Dictionary<VariableSymbol, object>();
            Console.WriteLine("Commands: #trees, #cls");

            while (true)
            {
                Console.Write("> ");
                string line = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(line))
                    return;

                if (line.ToLower() == "#trees")
                {
                    showTree = !showTree;
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine(showTree ? "Enabled trees to show" : "Disabled trees");
                    Console.ResetColor();
                    continue;
                }
                else if (line.ToLower() == "#cls")
                {
                    Console.Clear();
                    continue;
                }

                SyntaxTree syntaxTree = SyntaxTree.Parse(line);
                Compilation compilation = new Compilation(syntaxTree);
                EvaluationResult result = compilation.Evaluate(variables);

                IReadOnlyList<Diagnostic> diagnostics = result.Diagnostics;

                if (showTree)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    syntaxTree.Root.WriteTo(Console.Out);
                    Console.ResetColor();
                }

                if (!diagnostics.Any())
                {
                    Console.WriteLine(result.Value);
                }
                else
                {
                    var text = syntaxTree.Text;

                    foreach (Diagnostic diagnostic in diagnostics)
                    {
                        int lineIndex = text.GetLineIndex(diagnostic.Span.Start);
                        int lineNumber = lineIndex + 1;
                        int character = diagnostic.Span.Start - text.Lines[lineIndex].Start + 1;

                        Console.WriteLine();

                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.Write($"({lineNumber}, {character}): ");
                        Console.WriteLine(diagnostic);
                        Console.ResetColor();

                        string prefix = line.Substring(0, diagnostic.Span.Start);
                        string error = line.Substring(diagnostic.Span.Start, diagnostic.Span.Length);
                        string suffix = line.Substring(diagnostic.Span.End);

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
}
