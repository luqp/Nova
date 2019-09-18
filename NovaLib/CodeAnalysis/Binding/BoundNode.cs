using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Nova.CodeAnalysis.Binding
{
    internal abstract class BoundNode
    {
        public abstract BoundNodeKind Kind { get; }

        public IEnumerable<BoundNode> GetChildren()
        {
            PropertyInfo[] properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo property in properties)
            {
                if (typeof(BoundNode).IsAssignableFrom(property.PropertyType))
                {
                    BoundNode child = (BoundNode) property.GetValue(this);
                    if (child != null)
                        yield return child;
                }
                else if (typeof(IEnumerable<BoundNode>).IsAssignableFrom(property.PropertyType))
                {
                    IEnumerable<BoundNode> children = (IEnumerable<BoundNode>) property.GetValue(this);
                    foreach (BoundNode child in children)
                    {
                        if (child != null)
                            yield return child;
                    }
                }
            }
        }

        public void WriteTo(TextWriter writer)
        {
            PrettyPrint(writer, this);
        }

        private static void PrettyPrint(TextWriter writer, BoundNode node, string indent = "", bool islast = true)
        {
            bool isToConsole = writer == Console.Out;
            string marker = islast ? "└──" : "├──";

            if (isToConsole)
                Console.ForegroundColor = ConsoleColor.DarkGray;

            writer.Write(indent);
            writer.Write(marker);

            WriteNode(writer, node);
            
            if (isToConsole)
                Console.ResetColor();

            writer.WriteLine();
            indent += islast ? "   " : "│  ";

            BoundNode lastChild = node.GetChildren().LastOrDefault();
            foreach (BoundNode child in node.GetChildren())
                PrettyPrint(writer, child, indent, child == lastChild);
        }

        private static void WriteNode(TextWriter writer, BoundNode node)
        {
            Console.ForegroundColor = GetColor(node);
            writer.Write(node.Kind);
            Console.ResetColor();
        }

        private static ConsoleColor GetColor(BoundNode node)
        {
            if (node is BoundExpression)
                return ConsoleColor.Blue;
            if (node is BoundStatement)
                return ConsoleColor.Cyan;
            
            return ConsoleColor.Yellow;
        }

        public override string ToString()
        {
            using (StringWriter writer = new StringWriter())
            {
                WriteTo(writer);
                return writer.ToString();
            }
        }
    }
}