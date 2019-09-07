using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Nova.CodeAnalysis.Text;

namespace Nova.CodeAnalysis.Syntax
{
    public sealed class SyntaxTree
    {
        public SyntaxTree(SourceText text)
        {
            Parser parser = new Parser(text);
            CompilationUnitSyntax root = parser.ParseCompilationUnit();
            ImmutableArray<Diagnostic> diagnostics = parser.Diagnostics.ToImmutableArray();

            Text = text;
            Diagnostics = diagnostics;
            Root = root;
        }

        public SourceText Text { get; }
        public ImmutableArray<Diagnostic> Diagnostics { get; }
        public CompilationUnitSyntax Root { get; }

        public static SyntaxTree Parse(string text)
        {
            SourceText sourceText = SourceText.From(text);
            return Parse(sourceText);
        }

        public static SyntaxTree Parse(SourceText text)
        {
            return new SyntaxTree(text);
        }

        public static IEnumerable<SyntaxToken> ParseTokens(string text)
        {
            SourceText sourceText = SourceText.From(text);
            return ParseTokens(sourceText);
        }
        
        public static IEnumerable<SyntaxToken> ParseTokens(SourceText text)
        {
            Lexer lexer = new Lexer(text);
            while (true)
            {
                SyntaxToken token = lexer.Lex();
                if (token.Kind == SyntaxKind.EndOfFileToken)
                    break;
                
                yield return token;
            }
            
        }
    }
}