using System;

namespace nova
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("> ");
            string line = Console.ReadLine();
            if  (string.IsNullOrWhiteSpace(line))
                return;
            var lexer = new Lexer(line);
            while(true)
            {
                SyntaxToken token = lexer.NextToken();
                if (token.Kind == SyntaxKind.EndOfFileToken)
                    break;

                Console.Write($"{token.Kind}: '{token.Text}'");

                if (token.Value != null)
                    Console.Write($" {token.Value}");
                
                Console.WriteLine();
            }
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
        EndOfFileToken
    }

    class SyntaxToken
    {
        public SyntaxToken(SyntaxKind kind, int position, string text, object value)
        {
            Kind = kind;
            Position = position;
            Text = text;
            Value = value;
        }

        public SyntaxKind Kind { get; }
        public int Position { get; }
        public string Text { get; }
        public object Value { get; }
    }

    class Lexer
    {
        private readonly string text;
        private int position;

        public Lexer(string text)
        {
            this.text = text;
        }

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

            return new SyntaxToken(SyntaxKind.BadToken, position++, text.Substring(position - 1, 1), null);
        }
    }
}
