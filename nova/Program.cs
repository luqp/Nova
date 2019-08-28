using System;
using System.Linq;

using Nova.CodeAnalysis;
using Nova.CodeAnalysis.Syntax;

namespace Nova
{
    internal static class Program
    {
        private static void Main()
        {
            bool showTree = false;
            Console.WriteLine("Commands: #trees, #cls");

            while (true)
            {
                Console.Write("> ");
                string line = Console.ReadLine();

                if  (string.IsNullOrWhiteSpace(line))
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

                if (showTree)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    PrettyPrint(syntaxTree.Root);
                    Console.ResetColor();
                }

                if (!syntaxTree.Diagnostics.Any())
                {
                    Evaluator e = new Evaluator(syntaxTree.Root);
                    int result = e.Evaluate();
                    Console.WriteLine(result);
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    foreach (string diagnostic in syntaxTree.Diagnostics)
                        Console.WriteLine(diagnostic);

                    Console.ResetColor();
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
