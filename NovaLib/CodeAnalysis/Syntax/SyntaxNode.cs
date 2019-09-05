using System.Collections.Generic;
using System.Reflection;

namespace Nova.CodeAnalysis.Syntax
{
    public abstract class SyntaxNode
    {
        public abstract SyntaxKind Kind { get; }

        public IEnumerable<SyntaxNode> GetChildren()
        {
            PropertyInfo[] properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo property in properties)
            {
                if (typeof(SyntaxNode).IsAssignableFrom(property.PropertyType))
                {
                    SyntaxNode child = (SyntaxNode) property.GetValue(this);
                    yield return child;
                }
                else if (typeof(IEnumerable<SyntaxNode>).IsAssignableFrom(property.PropertyType))
                {
                    IEnumerable<SyntaxNode> children = (IEnumerable<SyntaxNode>) property.GetValue(this);
                    foreach (SyntaxNode child in children)
                        yield return child;
                }
            }
        }
    }
}