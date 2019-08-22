﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace nova
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                Console.Write("> ");
                string line = Console.ReadLine();
                if  (string.IsNullOrWhiteSpace(line))
                    return;

                Parser parser = new Parser(line);
                SyntaxTree syntaxTree = parser.Parse();

                var color = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.DarkGray;
                PrettyPrint(syntaxTree.Root);
                Console.ForegroundColor = color;

                if (syntaxTree.Diagnostics.Any())
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    foreach (string diagnostic in parser.Diagnostics)
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

    enum SyntaxKind
    {
        NumberToken,
        WhiteSpaceToken,
        PlusToken,
        MinusToken,
        StarToken,
        SlashToken,
        OpenParenthesisToken,
        CloseParenthesisToken,
        BadToken,
        EndOfFileToken,
        NumberExpression,
        BinaryExpression
    }

    class SyntaxToken : SyntaxNode
    {
        public SyntaxToken(SyntaxKind kind, int position, string text, object value)
        {
            Kind = kind;
            Position = position;
            Text = text;
            Value = value;
        }

        public override SyntaxKind Kind { get; }
        public int Position { get; }
        public string Text { get; }
        public object Value { get; }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            return Enumerable.Empty<SyntaxNode>();
        }
            
    }

    class Lexer
    {
        private readonly string text;
        private int position;
        private List<string> diagnostics = new List<string>();

        public Lexer(string text)
        {
            this.text = text;
        }

        public IEnumerable<string> Diagnostics => diagnostics;
        private char Current
        {
            get
            {
                if (position >= text.Length)
                    return '\0';
                return text[position];
            }
        }

        private void Next()
        {
            position++;
        }

        public SyntaxToken NextToken()
        {
            // <numbers>
            // + - * ( ) <symbols>
            // <whitespace>

            if (position == text.Length)
                return new SyntaxToken(SyntaxKind.EndOfFileToken, position, "\0", null);

            if (char.IsDigit(Current))
            {
                int start = position;
                while (char.IsDigit(Current))
                    Next();
                int length = position - start;
                string text_token = text.Substring(start, length);
                int.TryParse(text_token, out var value);
                return new SyntaxToken(SyntaxKind.NumberToken, start, text_token, value);
            }
            
            if (char.IsWhiteSpace(Current))
            {
                int start = position;
                while (char.IsWhiteSpace(Current))
                    Next();
                int length = position - start;
                string text_token = text.Substring(start, length);
                return new SyntaxToken(SyntaxKind.WhiteSpaceToken, start, text_token, null);
            }

            if (Current == '+')
                return new SyntaxToken(SyntaxKind.PlusToken, position++, "+", null);
            else if (Current == '-')
                return new SyntaxToken(SyntaxKind.MinusToken, position++, "-", null);
            else if (Current == '*')
                return new SyntaxToken(SyntaxKind.StarToken, position++, "*", null);
            else if (Current == '/')
                return new SyntaxToken(SyntaxKind.SlashToken, position++, "/", null);
            else if (Current == '(')
                return new SyntaxToken(SyntaxKind.OpenParenthesisToken, position++, "(", null);
            else if (Current == ')')
                return new SyntaxToken(SyntaxKind.CloseParenthesisToken, position++, ")", null);

            diagnostics.Add($"ERROR: bad character input '{Current}'");
            return new SyntaxToken(SyntaxKind.BadToken, position++, text.Substring(position - 1, 1), null);
        }
    }

    abstract class SyntaxNode
    {
        public abstract SyntaxKind Kind { get; }

        public abstract IEnumerable<SyntaxNode> GetChildren();
    }

    abstract class ExpressionSynax : SyntaxNode
    {
    }

    sealed class NumberExpressionSyntax : ExpressionSynax
    {
        public NumberExpressionSyntax(SyntaxToken numbreToken)
        {
            NumberToken = numbreToken;
        }

        public override SyntaxKind Kind => SyntaxKind.NumberExpression;
        public SyntaxToken NumberToken { get; }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return NumberToken;
        }
    }

    sealed class BinaryExpressionSyntax : ExpressionSynax
    {
        public BinaryExpressionSyntax(ExpressionSynax left, SyntaxToken operatorToken, ExpressionSynax right)
        {
            Left = left;
            OperatorToken = operatorToken;
            Right = right;
        }
        public override SyntaxKind Kind => SyntaxKind.BinaryExpression;
        public ExpressionSynax Left { get; }
        public SyntaxToken OperatorToken { get; }
        public ExpressionSynax Right { get; }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Left;
            yield return OperatorToken;
            yield return Right;
        }
    }

    sealed class SyntaxTree
    {
        public SyntaxTree(IEnumerable<string> diagnostics, ExpressionSynax root, SyntaxToken endOfFileToken)
        {
            Diagnostics = diagnostics.ToArray();
            Root = root;
            EndOfFileToken = endOfFileToken;
        }

        public IReadOnlyList<string> Diagnostics { get; }
        public ExpressionSynax Root { get; }
        public SyntaxToken EndOfFileToken { get; }
    }

    class Parser
    {
        private readonly SyntaxToken[] tokens;
        private int position;
        private List<string> diagnostics = new List<string>();
        public Parser(string text)
        {
            var tokens = new List<SyntaxToken>();

            Lexer lexer = new Lexer(text);
            SyntaxToken token;
            do
            {
                token = lexer.NextToken();
                if (token.Kind != SyntaxKind.WhiteSpaceToken &&
                    token.Kind != SyntaxKind.BadToken)
                {
                    tokens.Add(token);
                }
            } while (token.Kind != SyntaxKind.EndOfFileToken);

            this.tokens = tokens.ToArray();
            this.diagnostics.AddRange(lexer.Diagnostics);
        }
        public IEnumerable<string> Diagnostics => diagnostics;
        private SyntaxToken Peek(int offset)
        {
            int index = position + offset;
            if (index >= tokens.Length)
                return tokens[tokens.Length - 1];
            
            return tokens[index];
        }

        private SyntaxToken Current => Peek(0);

        private SyntaxToken NextToken()
        {
            var current = Current;
            position++;
            return current;
        }

        private SyntaxToken Match(SyntaxKind kind)
        {
            if (Current.Kind == kind)
                return NextToken();

            diagnostics.Add($"ERROR: Unexpected token <{Current.Kind}>, expected <{kind}>");
            return new SyntaxToken(kind, Current.Position, null, null);
        }

        public SyntaxTree Parse()
        {
            ExpressionSynax expression = ParseExpression();
            SyntaxToken endOfFileToken = Match(SyntaxKind.EndOfFileToken);
            return new SyntaxTree(diagnostics, expression, endOfFileToken);
        }

        private ExpressionSynax ParseExpression()
        {
            var left = ParsePrimaryExpression();
            while (Current.Kind == SyntaxKind.PlusToken ||
                   Current.Kind == SyntaxKind.MinusToken)
            {
                var operatorToken = NextToken();
                var right = ParsePrimaryExpression();
                left = new BinaryExpressionSyntax(left, operatorToken, right);
            }
            return left;
        }

        private ExpressionSynax ParsePrimaryExpression()
        {
            var numberToken = Match(SyntaxKind.NumberToken);
            return new NumberExpressionSyntax(numberToken);
        }
    }
}
