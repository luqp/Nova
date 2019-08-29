using System.Collections.Generic;

namespace Nova.CodeAnalysis.Syntax
{
    internal sealed class Lexer
    {
        private readonly string text;
        private int position;
        private List<string> diagnostics = new List<string>();

        public Lexer(string text)
        {
            this.text = text;
        }

        public IEnumerable<string> Diagnostics => diagnostics;

        private char Current => Peek(0);

        private char Lookahead => Peek(1);

        private char Peek(int offset)
        {
            int index = position + offset;
            if (index >= text.Length)
                return '\0';
            return text[index];
        }

        private void Next()
        {
            position++;
        }

        public SyntaxToken Lex()
        {
            if (position == text.Length)
                return new SyntaxToken(SyntaxKind.EndOfFileToken, position, "\0", null);

            if (char.IsDigit(Current))
            {
                int start = position;
                while (char.IsDigit(Current))
                    Next();
                int length = position - start;
                string text_token = text.Substring(start, length);
                if (!int.TryParse(text_token, out var value))
                    diagnostics.Add($"The number {text} isn't valid Int32.");

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

            if (char.IsLetter(Current))
            {
                int start = position;
                while (char.IsLetter(Current))
                    Next();
                int length = position - start;
                string text_token = text.Substring(start, length);
                SyntaxKind kind = SyntaxFacts.GetKeywordKind(text_token);
                return new SyntaxToken(kind, start, text_token, null);
            }

            switch (Current)
            {
                case '+':
                    return new SyntaxToken(SyntaxKind.PlusToken, position++, "+", null);
                case '-':
                    return new SyntaxToken(SyntaxKind.MinusToken, position++, "-", null);
                case '*':
                    return new SyntaxToken(SyntaxKind.StarToken, position++, "*", null);
                case '/':
                    return new SyntaxToken(SyntaxKind.SlashToken, position++, "/", null);
                case '(':
                    return new SyntaxToken(SyntaxKind.OpenParenthesisToken, position++, "(", null);
                case ')':
                    return new SyntaxToken(SyntaxKind.CloseParenthesisToken, position++, ")", null);
                case '&':
                    if (Lookahead == '&')
                        return new SyntaxToken(SyntaxKind.AmpersanAmpersanToken, position += 2, "&&", null);
                    break;
                case '|':
                    if (Lookahead == '|')
                        return new SyntaxToken(SyntaxKind.PipePipeToken, position += 2, "||", null);
                    break;
                case '=':
                    if (Lookahead == '=')
                        return new SyntaxToken(SyntaxKind.EqualsEqualsToken, position += 2, "==", null);
                    break;
                case '!':
                    if (Lookahead == '=')
                        return new SyntaxToken(SyntaxKind.BangEqualsToken, position += 2, "!=", null);
                    return new SyntaxToken(SyntaxKind.BangToken, position++, "!", null);
            }

            diagnostics.Add($"ERROR: bad character input '{Current}'");
            return new SyntaxToken(SyntaxKind.BadToken, position++, text.Substring(position - 1, 1), null);
        }
    }
}