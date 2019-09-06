using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nova.CodeAnalysis;
using Nova.CodeAnalysis.Binding;
using Nova.CodeAnalysis.Syntax;
using Nova.CodeAnalysis.Text;

namespace Nova
{
    internal static class Program
    {
        private static void Main()
        {
            bool showTree = false;
            StringBuilder textBuilder = new StringBuilder();
            Dictionary<VariableSymbol, object> variables = new Dictionary<VariableSymbol, object>();
            Console.WriteLine("Commands: #trees, #cls");

            while (true)
            {
                if (textBuilder.Length == 0)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write("» ");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write("· ");
                }
                Console.ResetColor();

                string input = Console.ReadLine();
                bool isBlank = string.IsNullOrWhiteSpace(input);

                if (textBuilder.Length == 0)
                {
                    if (isBlank)
                    {
                        break;
                    }
                    else if (input.ToLower() == "#trees")
                    {
                        showTree = !showTree;
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                        Console.WriteLine(showTree ? "Enabled trees to show" : "Disabled trees");
                        Console.ResetColor();
                        continue;
                    }
                    else if (input.ToLower() == "#cls")
                    {
                        Console.Clear();
                        continue;
                    }
                }

                textBuilder.AppendLine(input);
                string text = textBuilder.ToString();

                SyntaxTree syntaxTree = SyntaxTree.Parse(text);

                if (!isBlank && syntaxTree.Diagnostics.Any())
                    continue;

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
                    Console.ForegroundColor = ConsoleColor.DarkMagenta;
                    Console.WriteLine(result.Value);
                    Console.ResetColor();
                }
                else
                {
                    foreach (Diagnostic diagnostic in diagnostics)
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

                textBuilder.Clear();
            }
        }
    }
}
