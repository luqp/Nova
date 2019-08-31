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
                    PrettyPrint(syntaxTree.Root);
                    Console.ResetColor();
                }

                if (!diagnostics.Any())
                {
                    Console.WriteLine(result.Value);
                }
                else
                {
                    foreach (Diagnostic diagnostic in diagnostics)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.WriteLine();
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
        static void PrettyPrint(SyntaxNode node, string indent = "", bool islast = true)
        {
            string marker = islast ? "└──" : "├──";
            Console.Write(indent);
            Console.Write(marker);
            Console.Write(node.Kind);

            if (node is SyntaxToken t && t.Value != null)
            {
                Console.Write($" {t.Value}");
            }

            Console.WriteLine();
            indent += islast ? "   " : "│  ";

            SyntaxNode lastChild = node.GetChildren().LastOrDefault();
            foreach (SyntaxNode child in node.GetChildren())
                PrettyPrint(child, indent, child == lastChild);
        }
    }
}
