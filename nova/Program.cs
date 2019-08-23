using System;
using System.Linq;
using Nova.CodeAnalysis;

namespace Nova
{
    class Program
    {
        static void Main(string[] args)
        {
            bool showTree = false;
            Console.WriteLine("Commands: #trees, #cls");

            while (true)
            {
                Console.Write("> ");
                string line = Console.ReadLine();
                var color = Console.ForegroundColor;

                if  (string.IsNullOrWhiteSpace(line))
                    return;

                if (line.ToLower() == "#trees")
                {
                    showTree = !showTree;
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine(showTree ? "Enabled trees to show" : "Disabled trees");
                    Console.ForegroundColor = color;
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
                    Console.ForegroundColor = color;
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

                    Console.ForegroundColor = color;
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
            indent += islast ? "    " : "│   ";

            var lastChild = node.GetChildren().LastOrDefault();
            foreach (var child in node.GetChildren())
                PrettyPrint(child, indent, child == lastChild);
        }
    }
}
